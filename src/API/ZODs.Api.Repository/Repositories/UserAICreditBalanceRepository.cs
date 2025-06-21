using Microsoft.EntityFrameworkCore;
using ZODs.Api.Repository.Entities.AI;

namespace ZODs.Api.Repository;

public sealed class UserAICreditBalanceRepository : Repository<UserAICreditBalance, ZodsContext>, IUserAICreditBalanceRepository
{
    public UserAICreditBalanceRepository(ZodsContext context)
        : base(context)
    {
    }

    public async Task AddUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var balanceExists = await Context.UserAICreditBalances.AnyAsync(x => x.UserId == userId, cancellationToken);
        if (!balanceExists)
        {
            var userAICreditBalance = new UserAICreditBalance
            {
                UserId = userId,
                Gpt3Credits = credits,
            };

            await Insert(userAICreditBalance, cancellationToken);
        }
        else
        {
            await Context.UserAICreditBalances
                         .Where(x => x.UserId == userId)
                         .ExecuteUpdateAsync(
                              x => x.SetProperty(p => p.Gpt3Credits, v => v.Gpt3Credits + credits),
                              cancellationToken);
        }
    }

    public async Task AddUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var balanceExists = await Context.UserAICreditBalances.AnyAsync(x => x.UserId == userId, cancellationToken);
        if (!balanceExists)
        {
            var userAICreditBalance = new UserAICreditBalance
            {
                UserId = userId,
                Gpt4Credits = credits,
            };

            await Insert(userAICreditBalance, cancellationToken);
        }
        else
        {
            await Context.UserAICreditBalances
                         .Where(x => x.UserId == userId)
                         .ExecuteUpdateAsync(
                              x => x.SetProperty(p => p.Gpt4Credits, v => v.Gpt4Credits + credits),
                              cancellationToken);
        }
    }

    public async Task<int> UseUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        await Context.UserAICreditBalances
                     .Where(x => x.UserId == userId)
                     .ExecuteUpdateAsync(
                            x => x.SetProperty(p => p.Gpt3Credits, v => v.Gpt3Credits - credits),
                            cancellationToken);

        return await Context.UserAICreditBalances
            .Where(x => x.UserId == userId)
            .Select(x => x.Gpt3Credits)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> UseUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        await Context.UserAICreditBalances
                     .Where(x => x.UserId == userId)
                     .ExecuteUpdateAsync(
                            x => x.SetProperty(p => p.Gpt4Credits, v => v.Gpt4Credits - credits),
                            cancellationToken);

        return await Context.UserAICreditBalances
            .Where(x => x.UserId == userId)
            .Select(x => x.Gpt4Credits)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetUserGpt3CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var updatedItems = await Context.UserAICreditBalances
                                       .Where(x => x.UserId == userId)
                                       .ExecuteUpdateAsync(
                                              x => x.SetProperty(p => p.Gpt3Credits, v => credits),
                                              cancellationToken);

        if (updatedItems == 0)
        {
            var userAICreditBalance = new UserAICreditBalance
            {
                UserId = userId,
                Gpt3Credits = credits,
                CreatedAt = DateTime.UtcNow,
            };

            await Insert(userAICreditBalance, cancellationToken);
        }
    }

    public async Task SetUserGpt4CreditsAsync(Guid userId, int credits, CancellationToken cancellationToken)
    {
        var updatedItems = await Context.UserAICreditBalances
                     .Where(x => x.UserId == userId)
                     .ExecuteUpdateAsync(
                            x => x.SetProperty(p => p.Gpt4Credits, v => credits),
                            cancellationToken);

        if (updatedItems == 0)
        {
            var userAICreditBalance = new UserAICreditBalance
            {
                UserId = userId,
                Gpt4Credits = credits,
                CreatedAt = DateTime.UtcNow,
            };

            await Insert(userAICreditBalance, cancellationToken);
        }
    }
}