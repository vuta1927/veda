//using System;
//using System.Collections.Generic;
//using System.Linq;
//using VDS.Dependency;
//using VDS.Serialization;
//
//namespace VDS.Messaging.Commands
//{
//    /// <summary>
//    /// A command bus that sends serialized object payloads through a <see cref="IMessagePublisher"/>.
//    /// </summary>
//    public class CommandBus : ICommandBus, ISingletonDependency
//    {
//        private readonly IMessagePublisher _publisher;
//        private readonly ISerializer _serializer;
//
//        /// <summary>
//        /// Initializes a new instance of the <see cref="CommandBus"/> class.
//        /// </summary>
//        public CommandBus(IMessagePublisher publisher, ISerializer serializer)
//        {
//            _publisher = publisher;
//            _serializer = serializer;
//        }
//
//        /// <summary>
//        /// Sends the specified command.
//        /// </summary>
//        public void Send(Envelope<ICommand> command)
//        {
//            var message = BuildMessage(command);
//
//            _publisher.Send(message);
//        }
//
//        public void Send(IEnumerable<Envelope<ICommand>> commands)
//        {
//            var messages = commands.Select(BuildMessage);
//
//            _publisher.Send(messages);
//        }
//
//        private Message BuildMessage(Envelope<ICommand> command)
//        {
//            // TODO: should use the Command ID as a unique constraint when storing it.
//
//            return new Message(_serializer.Serialize(command.Body), command.Delay != TimeSpan.Zero ? (DateTime?)DateTime.UtcNow.Add(command.Delay) : null, command.CorrelationId);
//        }
//    }
//}