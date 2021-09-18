using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CommandStore
{
    public class Engine<TRepository>
    {
        private Dictionary<string, (ICommandHandler<TRepository> commandHandler, MethodInfo itemDeserializer)> commandCommandHandlerPair = new Dictionary<string, (ICommandHandler<TRepository> commandHandler, MethodInfo itemDeserializer)>();
        private Dictionary<string, dynamic> queryQueryHandlerPair = new Dictionary<string, dynamic>();

        public TRepository repository;

        private List<Command> commands = new List<Command>();

        private int lastCommandNumber;

        private IRepositoryFactory<TRepository> repositoryFactory;

        public string LocationPath { get; private set; }

        public Engine(IRepositoryFactory<TRepository> repositoryFactory, string locationPath)
        {
            this.repositoryFactory = repositoryFactory;
            this.repository = this.repositoryFactory.CreateNewRepository();
            this.LocationPath = locationPath;
        }

        public void CreateSnapshot()
        {
            var repositorySerialized = this.SerializeRepository();
            var snapshots = Directory.GetFiles(this.LocationPath, "*.Snapshot");
            var snapshotNumber = snapshots
                .Select(a => Convert.ToInt32(a.Substring(a.LastIndexOf('\\') + 1, 10)))
                .Append(0)
                .Max();
            var path = @$"{this.LocationPath}\{snapshotNumber + 1:D10}.{this.lastCommandNumber:D10}.Snapshot";
            File.WriteAllBytes(path, repositorySerialized);
        }

        public void SaveCommandsToJournal()
        {
            var commandsSerialized = this.SerializeCommands();
            var journals = Directory.GetFiles(this.LocationPath, "*.Journal");
            var journalNumber = journals
                .Select(a => Convert.ToInt32(a.Substring(a.LastIndexOf('\\') + 1, 10)))
                .Append(0)
                .Max();
            var path = @$"{this.LocationPath}\{journalNumber + 1:D10}.{this.lastCommandNumber:D10}.Journal";
            File.WriteAllBytes(path, commandsSerialized);
            commands.Clear();
        }

        public void Replay()
        {
            Dictionary<int, string> snapshotDictionary = new Dictionary<int, string>();
            var snapshots = Directory.GetFiles(this.LocationPath, "*.Snapshot")
                .OrderBy(a => a)
                .ToList();

            snapshots
                   .ForEach(a =>
                   {
                       var lastComandNumber = Convert.ToInt32(a.Substring(a.LastIndexOf('\\') + 12, 10));
                       snapshotDictionary.Add(lastComandNumber, a);
                   });


            Dictionary<int, string> journalDictionary = new Dictionary<int, string>();
            var journals = Directory.GetFiles(this.LocationPath, "*.Journal")
                .OrderBy(a => a)
                .ToList();


            journals
                .ForEach(a =>
                {
                    var lastComandNumber = Convert.ToInt32(a.Substring(a.LastIndexOf('\\') + 12, 10));
                    journalDictionary.Add(lastComandNumber, a);
                });

            if (snapshotDictionary.Count > 0)
            {
                var lastSnapshotPath = snapshotDictionary.Last();
                var rawData = File.ReadAllBytes(lastSnapshotPath.Value);
                this.repository = DeserialiserHelper.Deserialize<TRepository>(rawData);

                var journalsToApply = journalDictionary
                    .Where(a => a.Key > lastSnapshotPath.Key)
                    .Select(a => a.Value)
                    .ToList();
                foreach (var path in journalsToApply)
                {
                    var rawDataJournal = File.ReadAllBytes(path);
                    this.ReplayCommands(rawDataJournal);
                }

                return;
            }

            this.ReplayCommands();
        }

        public void ReplayCommands()
        {
            var journals = Directory.GetFiles(this.LocationPath, "*.Journal");
            foreach (var path in journals)
            {
                var rawData = File.ReadAllBytes(path);
                this.ReplayCommands(rawData);
            }
        }

        public void ReplayCommands(byte[] commands)
        {
            var commandsDeserialized = DeserialiserHelper.Deserialize<List<Command>>(commands);
            commandsDeserialized.ForEach(a =>
            {
                object[] args = { a.ItemInBytes };
                a.ItemObject = this.commandCommandHandlerPair[a.CommandType].itemDeserializer.Invoke(null, args);
                this.ReplayCommand(a);
            });
        }

        public Engine<TRepository> RegisterCommandAndCommandHander<TCommand, TCommandHandler>()
        {
            var interfaceCommandType = typeof(ICommand<>).GetGenericTypeDefinition();
            var interfaceCommand = typeof(TCommand)
                        .GetInterfaces()
                        .First(a => a.GetGenericTypeDefinition() == interfaceCommandType);

            var methodDeserializedItem = typeof(DeserialiserHelper)
                .GetMethod(nameof(DeserialiserHelper.Deserialize))
                .MakeGenericMethod(interfaceCommand.GetGenericArguments());

            var commandHandlerType = typeof(TCommandHandler);
            ICommandHandler<TRepository> commandHandler = (ICommandHandler<TRepository>)commandHandlerType.Assembly.CreateInstance(commandHandlerType.FullName);

            this.commandCommandHandlerPair.Add(typeof(TCommand).Name, (commandHandler, itemDeserializer: methodDeserializedItem));

            return this;
        }

        public Engine<TRepository> RegisterQueryAndQueryHander<TQuery, TQueryHandler>()
        {
            var queryHandlerType = typeof(TQueryHandler);
            TQueryHandler queryHandler = (TQueryHandler)queryHandlerType.Assembly.CreateInstance(queryHandlerType.FullName);

            this.queryQueryHandlerPair.Add(typeof(TQuery).Name, queryHandler);

            return this;
        }

        public void ExecuteCommmand(Command command)
        {
            this.ExecuteCommmandWithoutAdding(command);
            this.commands.Add(command);
            command.CommandNumber = ++this.lastCommandNumber;
        }

        public TResult ExecuteQuery<TResult>(IQuery<TResult> query)
        {
            dynamic queryHandler = this.queryQueryHandlerPair[query.GetType().Name];
            return queryHandler.Execute(this.repository, (dynamic)query);
        }

        public void ExecuteCommmands(List<Command> commands)
        {
            commands.ForEach(command => this.ExecuteCommmand(command));
        }

        private void ReplayCommand(Command command)
        {
            this.ExecuteCommmandWithoutAdding(command);
            this.lastCommandNumber = command.CommandNumber;
        }

        private void ExecuteCommmandWithoutAdding(Command command)
        {
            var commandHandler = commandCommandHandlerPair[command.CommandType].commandHandler;
            commandHandler.Execute(this.repository, command.ItemObject);
        }

        public byte[] SerializeRepository()
        {
            return SerializerHelper.Serialize(this.repository);
        }

        public byte[] SerializeCommands()
        {
            return SerializerHelper.Serialize(this.commands);
        }
    }
}
