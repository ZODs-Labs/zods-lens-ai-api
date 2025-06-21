using ZODs.Api.Repository;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Service
{
    public sealed class DataSeedService : IDataSeedService
    {
        private readonly IUnitOfWork<ZodsContext> unitOfWork;
        private readonly RoleManager<Role> roleManager;

        public DataSeedService(
            IUnitOfWork<ZodsContext> unitOfWork,
            RoleManager<Role> roleManager)
        {
            this.unitOfWork = unitOfWork;
            this.roleManager = roleManager;
        }

        private IDataSeedRepository DataSeedRepository => unitOfWork.GetRepository<IDataSeedRepository>();

        public async Task SeedAsync()
        {
            if (!await roleManager.Roles.AnyAsync())
            {
                await SeedPricingPlanRolesAsync();
            }

            await DataSeedRepository.SeedAsync();
        }

        private async Task SeedPricingPlanRolesAsync()
        {
            var roles = new string[]
            {
                "BasicUser",
                "StandardUser",
                "PremiumUser",
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new Role
                {
                    Name = role,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    NormalizedName = role.ToUpper(),
                });
            }
        }
    }
}