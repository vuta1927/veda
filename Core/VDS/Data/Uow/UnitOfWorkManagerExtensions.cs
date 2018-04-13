using System;
using System.Threading.Tasks;
using VDS.Threading;

namespace VDS.Data.Uow
{
    public static class UnitOfWorkManagerExtensions
    {
        private static readonly UnitOfWorkOptions DefaultUoWOptions = new UnitOfWorkOptions();

        public static void PerformSyncUow(this IUnitOfWorkManager unitOfWorkManager, Action action, UnitOfWorkOptions options = null)
        {
            using (var uow = unitOfWorkManager.Begin(options ?? DefaultUoWOptions))
            {
                action();
                uow.Complete();
            }
        }
        
        public static T PerformAsyncUow<T>(this IUnitOfWorkManager unitOfWorkManager, Func<T> action, UnitOfWorkOptions options = null)
        {
            var uow = unitOfWorkManager.Begin(options ?? DefaultUoWOptions);
            object returnValue;

            try
            {
                returnValue = action();
            }
            catch
            {
                uow.Dispose();
                throw;
            }

            if (typeof(T) == typeof(Task))
            {
                returnValue = InternalAsyncHelper.AwaitTaskWithPostActionAndFinally(
                    (Task) returnValue,
                    async () => await uow.CompleteAsync(),
                    exception => uow.Dispose());
            }
            else //Task<TResult>
            {
                returnValue = InternalAsyncHelper.CallAwaitTaskWithPostActionAndFinallyAndGetResult(
                    typeof(T).GenericTypeArguments[0],
                    returnValue,
                    async () => await uow.CompleteAsync(),
                    exception => uow.Dispose());
            }

            return (T)returnValue;
        }
    }
}