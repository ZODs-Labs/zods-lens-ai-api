using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository.Entities.Entities;

namespace ZODs.Api.Repository.Extensions;

public static class AILensDbSetExtensions
{
    public static IQueryable<AILens> BuildProtectedAILensReadAccessQuery(
        this DbSet<AILens> dbSet,
        Guid executingUserId)
    {
        if (executingUserId == default)
        {
            throw new ArgumentNullException(nameof(executingUserId));
        }

        return dbSet.Where(x => x.UserId == executingUserId || x.IsBuiltIn);
    }
}