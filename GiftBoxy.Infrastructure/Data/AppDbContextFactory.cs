using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GiftBoxy.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql("Host=dpg-d85a9qog4nts73fpgd30-a.oregon-postgres.render.com;Port=5432;Database=giftboxy_db;Username=giftboxy_db_user;Password=nGftEIcjxBaAagprBw7M9sDw0L1g8XHf;TrustServerCertificate=true;SslMode=Require");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
