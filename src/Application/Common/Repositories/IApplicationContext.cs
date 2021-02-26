﻿using DaprCleanArchitecture.Domain.Common;
using System.Threading;
using System.Threading.Tasks;

namespace DaprCleanArchitecture.Application.Common.Repositories
{
    public interface IApplicationContext
    {
        ICommandRepository<TEntity> CommandSet<TEntity>() where TEntity : Entity;

        IQueryRepository<TEntity> QuerySet<TEntity>() where TEntity : Entity;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
