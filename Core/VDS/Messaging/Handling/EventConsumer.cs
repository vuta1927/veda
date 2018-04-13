//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Threading.Tasks;
//using VDS.Helpers;
//
//namespace VDS.Messaging.Handling
//{
//    public sealed class EventConsumer : DisposableObject, IEventConsumer
//    {
//        private readonly IEnumerable<IEventHandler> _eventHandlers;
//        private readonly IMessageSubscriber _subscriber;
//        private bool _disposed;
//
//        public EventConsumer(IMessageSubscriber subscriber, IEnumerable<IEventHandler> eventHandlers)
//        {
//            _subscriber = subscriber;
//            _eventHandlers = eventHandlers;
//            subscriber.MessageReceived += async (sender, e) =>
//            {
//                if (_eventHandlers != null)
//                {
//                    foreach (var handler in _eventHandlers)
//                    {
//                        //await handler.HandleAsync(e.Message);
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
//        public IEnumerable<IEventHandler> EventHandlers => _eventHandlers;
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