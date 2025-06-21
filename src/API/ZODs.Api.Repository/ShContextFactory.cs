using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository;

public class ZodsContextFactory : IDesignTimeDbContextFactory<ZodsContext>
{
    public ZodsContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ZodsContext>();
        optionsBuilder.UseNpgsql("Host=zods.db; Database=zods; Username=postgres; Password=123456; Include Error Detail=true");

        return new ZodsContext(optionsBuilder.Options);
    }
}
