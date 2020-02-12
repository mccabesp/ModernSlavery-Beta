﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ModernSlavery.Core.Interfaces
{

    public interface IDataRepository : IDisposable, IDataTransaction
    {

        IDbContext GetDbContext();

        void Delete<TEntity>(TEntity entity) where TEntity : class;

        TEntity Get<TEntity>(params object[] keyValues) where TEntity : class;

        IQueryable<TEntity> GetAll<TEntity>() where TEntity : class;

        void Insert<TEntity>(TEntity entity) where TEntity : class;

        DbSet<TEntity> GetEntities<TEntity>() where TEntity : class;

        Task SaveChangesAsync();

        void UpdateChangesInBulk<TEntity>(IEnumerable<TEntity> listOfOrganisations) where TEntity : class;

    }

}