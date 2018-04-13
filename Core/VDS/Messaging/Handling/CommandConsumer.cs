//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using VDS.Helpers;
//
//namespace VDS.Messaging.Handling
//{
//    /// <summary>
//    /// Represents the default command consumer which will simply iterate
//    /// each registered command handler and handle the command within it.
//    /// </summary>
//    public sealed class CommandConsumer : DisposableObject, ICommandConsumer
//    {
//        private readonly IEnumerable<ICommandHandler> _commandHandlers;
//        private readonly IMessageSubscriber _subscriber;
//        private bool _disposed;
//
//        public CommandConsumer(IMessageSubscriber subscriber, IEnumerable<ICommandHandler> commandHandlers)
//        {
//            _subscriber = subscriber;
//            _commandHandlers = commandHandlers;
//            subscriber.MessageReceived += async (sender, e) =>
//            {
//                if (_commandHandlers != null)
//                {
//                    foreach (var handler in _commandHandlers)
//                    {
//                        var handlerType = handler.GetType();
//                        var messageType = e.Message.GetType();
//                        var methodInfoQuery = from method in handlerType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
//                                              let parameters = method.GetParameters()
//                                              where method.Name == "HandleAsync" &&
//                                              method.ReturnType == typeof(Task) &&
//                                              parameters.Length == 1 &&
//                                              parameters[0].ParameterType == messageType
//                                              select method;
//                        var methodInfo = methodInfoQuery.FirstOrDefault();
//                        if (methodInfo != null)
//                        {
//                            await (Task)methodInfo.Invoke(handler, new object[] { e.Message });
//                        }
//                    }
//                }
//            };
//        }
//
//        public IEnumerable<ICommandHandler> CommandHandlers => _commandHandlers;
//
//        public IMessageSubscriber Subscriber => _subscriber;
//
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                if (!_disposed)
//                {
//                    _subscriber.Dispose();
//                    _disposed = true;
//                }
//            }
//        }
//    }
//}