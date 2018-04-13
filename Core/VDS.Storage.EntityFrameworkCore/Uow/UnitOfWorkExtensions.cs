﻿using System;
using VDS.Data.Uow;
using VDS.Helpers.Exception;
using Microsoft.EntityFrameworkCore;

namespace VDS.Storage.EntityFrameworkCore.Uow
{
    /// <summary>
    /// Extension methods for UnitOfWork.
    /// </summary>
    public static class UnitOfWorkExtensions
    {
        /// <summary>
        /// Gets a DbContext as a part of active unit of work.
        /// This method can be called when current unit of work is an <see cref="EfCoreUnitOfWork"/>.
        /// </summary>
        /// <typeparam name="TDbContext">Type of the DbContext</typeparam>
        /// <param name="unitOfWork">Current (active) unit of work</param>

        public static TDbContext GetDbContext<TDbContext>(this IActiveUnitOfWork unitOfWork)
            where TDbContext : DbContext
        {
            Throw.IfArgumentNull(unitOfWork, nameof(unitOfWork));

            if (!(unitOfWork is EfCoreUnitOfWork))
            {
                throw new ArgumentException("unitOfWork is not type of " + typeof(EfCoreUnitOfWork).FullName, "unitOfWork");
            }

            return ((EfCoreUnitOfWork) unitOfWork).GetOrCreateDbContext<TDbContext>();
        }
    }
}