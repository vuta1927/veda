using System;
using System.Threading.Tasks;
using VDS.Data.Uow;
using NServiceBus.Logging;
using NServiceBus.Pipeline;

namespace VDS.Messaging.NServiceBus.UnitOfWork
{
    public class NServiceBusUnitOfWork : Behavior<IInvokeHandlerContext>
    {
        private ILog Logger = LogManager.GetLogger<NServiceBusUnitOfWork>();
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public NServiceBusUnitOfWork(IUnitOfWorkManager unitOfWorkManager)
        {
            _unitOfWorkManager = unitOfWorkManager;
        }

        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            Logger.DebugFormat("Message: {0}", context.MessageId);

            var unitOfWork = _unitOfWorkManager.Begin();

            await next();

            await unitOfWork.CompleteAsync();
        }
    }
}