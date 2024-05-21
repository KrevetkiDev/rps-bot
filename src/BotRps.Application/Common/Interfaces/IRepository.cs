using BotRpc.Domain.Entities.Base;

namespace BotRps.Application.Common.Interfaces;

public interface IRepository
{
    ITransaction<TEntity> BeginTransaction<TEntity>()
        where TEntity : EntityBase;

    Task<ITransaction<TEntity>> BeginTransactionAsync<TEntity>(CancellationToken cancellationToken)
        where TEntity : EntityBase;
}