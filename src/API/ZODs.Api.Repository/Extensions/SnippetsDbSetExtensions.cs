using ZODs.Api.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace ZODs.Api.Repository.Extensions
{
    public static class SnippetsDbSetExtensions
    {
        public static IQueryable<Snippet> BuildProtectedSnippetReadAccessQuery(
            this DbSet<Snippet> dbSet,
            Guid executingUserId)
        {
            if (executingUserId == default)
            {
                throw new ArgumentNullException(nameof(executingUserId));
            }

            return dbSet.Where(x => x.UserId == executingUserId ||
                                    x.SharedWorkspaces.Any(w => w.Workspace!.Members.Any(m => m.UserId == executingUserId)) ||
                                    (x.IsWorkspaceOwned &&
                                     x.Workspace!.Members.Any(m => m.UserId == executingUserId)));
        }
    }
}