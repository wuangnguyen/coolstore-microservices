using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VND.CoolStore.ShoppingCart.Domain.Cart;

namespace VND.CoolStore.ShoppingCart.Data.TypeConfig
{
    public class CartTypeConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts", "cart");

            builder.HasKey(x => x.Id);
        }
    }
}
