using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloudNativeKit.Domain;
using Dapper;
using ReflectionMagic;

namespace CloudNativeKit.Infrastructure.Data.Dapper.Core
{
    public class GenericRepository<TEntity, TId> : IRepositoryAsync<TEntity, TId>, IQueryRepository<TEntity, TId>
        where TEntity : class, IAggregateRoot<TId>
    {
        public ISqlConnectionFactory SqlConnectionFactory { get; }
        public IEnumerable<IDomainEventDispatcher> EventBuses { get; }

        public GenericRepository(ISqlConnectionFactory sqlConnectionFactory, IEnumerable<IDomainEventDispatcher> eventBuses)
        {
            SqlConnectionFactory = sqlConnectionFactory;
            EventBuses = eventBuses;
        }

        public IQueryable<TEntity> Queryable()
        {
            using var conn = SqlConnectionFactory.GetOpenConnection();
            var entities = conn.GetList<TEntity>();
            return entities.AsQueryable();
        }

        public async Task<TEntity> GetByIdAsync(TId id)
        {
            using var conn = SqlConnectionFactory.GetOpenConnection();
            var entities = await conn.GetListAsync<TEntity>(new { id });
            return entities.FirstOrDefault();
        }

        public async Task<IReadOnlyCollection<TEntity>> GetByConditionAsync(object whereConditions)
        {
            using var conn = SqlConnectionFactory.GetOpenConnection();
            var entities = await conn.GetListAsync<TEntity>(whereConditions);
            return entities.ToList();
        }

        public async Task<TEntity> AddAsync(TEntity value)
        {
            using var conn = SqlConnectionFactory.GetOpenConnection();
            var newId = await conn.InsertAsync<TId, TEntity>(value);
            if (value is IAggregateRoot<TId> returnValue)
            {
                returnValue.AsDynamic().Id = newId;
                return (TEntity)returnValue;
            }

            foreach (var @event in value.GetUncommittedEvents())
            {
                EventBuses.Select(b => b.Dispatch(@event)).ToList();
            }

            value.ClearUncommittedEvents();

            return value;
        }

        public async Task<TEntity> UpdateAsync(TEntity value)
        {
            using var conn = SqlConnectionFactory.GetOpenConnection();
            var numberRecordAffected = await conn.UpdateAsync(value);
            if (numberRecordAffected <= 0)
            {
                throw new Exception("Could not update record to the database.");
            }

            foreach (var @event in value.GetUncommittedEvents())
            {
                EventBuses.Select(b => b.Dispatch(@event)).ToList();
            }

            value.ClearUncommittedEvents();

            return await GetByIdAsync(value.Id);
        }

        public async Task<int> DeleteAsync(TEntity entity)
        {
            using var conn = SqlConnectionFactory.GetOpenConnection();
            var numberRecordAffected = await conn.DeleteAsync(entity);

            foreach (var @event in entity.GetUncommittedEvents())
            {
                EventBuses.Select(b => b.Dispatch(@event)).ToList();
            }

            entity.ClearUncommittedEvents();

            return numberRecordAffected;
        }
    }
}
