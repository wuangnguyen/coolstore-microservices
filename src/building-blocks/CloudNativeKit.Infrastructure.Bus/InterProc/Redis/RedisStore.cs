using System;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CloudNativeKit.Infrastructure.Bus.InterProc.Redis
{
    public class RedisStore
    {
        private static Lazy<ConnectionMultiplexer> _lazyConnection;

        public RedisStore(IOptions<RedisOptions> redisOptions)
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints =
                {
                    redisOptions.Value.Host
                },
                Password = redisOptions.Value.Password
            };

            _lazyConnection =
                new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        public ConnectionMultiplexer Connection => _lazyConnection.Value;

        public IDatabase RedisCache => Connection.GetDatabase();
    }
}
