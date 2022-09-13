using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq.Extensions;
using System.Threading.Tasks;
using Haze.Logging;
using System.Linq;
using System;

namespace Haze.Commands
{
    /// <summary>
    /// Executes methods that are marked with the Command attribute.
    /// </summary>
    public abstract class CommandExecuter
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Command"/> is executed successfully.
        /// </summary>
        public delegate void CommandExecutedEventHandler(object sender, CommandExecutedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Command"/> throws an exception.
        /// </summary>
        public delegate void CommandFailedEventHandler(object sender, CommandFailedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a parsing operation for a command fails.
        /// </summary>
        public delegate void CommandParseFailedEventHandler(object sender, CommandParseFailedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Determines which commands cannot be executed.
        /// </summary>
        public abstract string[] DisabledCommands { get; }

        /// <summary>
        /// Determines if parsed commands are logged.
        /// </summary>
        public bool LogExecutions { get; set; }

        /// <summary>
        /// Determines if execptions are logged.
        /// </summary>
        public bool LogExceptions { get; set; }

        /// <summary>
        /// The current amount of active command executers.
        /// </summary>
        public static int ExecuterCount 
        {
            get => executers.Count;
        }

        /// <summary>
        /// Dtermines if command parsing or execution should throw on failure.
        /// </summary>
        protected bool Throw { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a command is successfully executed.
        /// </summary>
        public event CommandExecutedEventHandler CommandExecuted;

        /// <summary>
        /// Invoked when a command throws an exception.
        /// </summary>
        public event CommandFailedEventHandler CommandFailed;

        /// <summary>
        /// Invoked when a parsing operation fails.
        /// </summary>
        public event CommandParseFailedEventHandler CommandParseFailed;

        #endregion

        #region Fields

        static Logger logger = new Logger(null);
        static LinkedList<CommandExecuter> executers = new LinkedList<CommandExecuter>();
        internal static CommandExecuter selectedExecuter = null;

        LinkedListNode<CommandExecuter> instanceNode;
        HashSet<string> disables;
        bool removed = false;

        #endregion

        #region Constructor & Finalizer

        /// <summary>
        /// Creates a new instance of the <see cref="CommandExecuter"/> class.
        /// </summary>
        protected CommandExecuter()
        {
            executers.AddLast(this);
            instanceNode = executers.Last;
            disables = DisabledCommands.ToHashSet();

            if (selectedExecuter is null && executers.Count > 0) SelectExecuter(this);
        }

        /// <summary>
        /// Removes this instance from the list of active executers.
        /// </summary>
        ~CommandExecuter()
        {
            RemoveExecuter();
        }

        #endregion

        /// <summary>
        /// Removes this instance from the list of active executers.
        /// </summary>
        public void RemoveExecuter()
        {
            if (!removed)
            {
                LinkedListNode<CommandExecuter> next = instanceNode.Next,
                                prev = instanceNode.Previous;

                executers.Remove(instanceNode);
                selectedExecuter = (next is null ? prev : next)?.Value;
                removed = true;
            }
        }

        /// <summary>
        /// Checks if a command is disabled.
        /// <para>
        /// If <paramref name="cmdName"/> does not refer to an existing command, this method returns <see langword="null"/>.
        /// </para>
        /// </summary>
        public bool? IsDisabled(string cmdName)
        {
            return GetType().GetMethods().Where(x => Attribute.IsDefined(x, typeof(CommandAttribute)) && x.Name == char.ToUpper(cmdName[0]) + cmdName.Substring(1)).FirstOrDefault() is null ? null : (bool?)disables.Contains(cmdName);
        }

        /// <summary>
        /// Parses a command.
        /// </summary>
        /// <exception cref="ArgumentException">The argument syntax is invalid.</exception>
        public static Command Parse(string cmd)
        {
            //Check if command is disabled
            if (selectedExecuter.disables.Contains(cmd)) CommandException(cmd, new ArgumentException($"Command {cmd.Split()[0]} is disabled.", "cmd"));

            //Uppercase the first character if needed
            cmd = cmd.UpperFirst();

            //Get the method name and the arguments
            string methodName = cmd.Split()[0];
            string[] args = Regex.Replace(cmd, @"^\w+ ?", "").SplitSequence(", ", new Dictionary<char, char>() { { '{', '}' }, { '"', '"' } }).Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //Argument check (arguments can be identified only by name or by index)
            if (args.Where(x => Regex.IsMatch(x, @"^\w+\: ")).Count() != args.Length && args.Where(x => !Regex.IsMatch(x, @"^\w+\: ")).Count() != args.Length) CommandException(cmd, new ArgumentException("Argument identifiers cannot be mixed.", "cmd"));

            //Check if arguments are identified by name and get the right method
            bool namedArgs = args.Length != 0 && Regex.IsMatch(args[0], @"^\w+\: ");

            //Overloads can be detected in 2 ways:
            //if command arguments are identified by name, get the overload with those argument names,
            //otherwise, try to infer the types from the command arguments and get the overload with the respective types
            var method = selectedExecuter.GetType().GetMethods().Where(x => x.Name == methodName && x.GetParameters().Length == args.Length && (namedArgs ? args.Select(n => Regex.Match(n, @"^\w+(?=\: )").Value).SequenceEqual(x.GetParameters().Select(n => n.Name)) : args.Select(n => TypeHelper.DetectType(n)).SequenceEqual(x.GetParameters().Select(n => n.ParameterType)))).FirstOrDefault();

            //Check the method's existance
            if (method is null)
            {
                CommandException(cmd, new ArgumentException($"No overload of {methodName} matches the command's argument list."));
                return Command.Empty;
            }

            //Get the arguments' values
            object[] argValues = args.Select((x, i) => namedArgs ? TypeHelper.Parse(method.GetParameters()?.FirstOrDefault(n => n.Name == Regex.Match(x, @"^\w+(?=\: )").Value).ParameterType, Regex.Replace(x, @"^\w+\: ", "")) : TypeHelper.Parse(method.GetParameters()[i].ParameterType, x)).ToArray();
            return new Command(selectedExecuter, method, text: cmd, argValues);
        }

        /// <summary>
        /// Gets the currently active executer.
        /// </summary>
        public static CommandExecuter GetSelectedExecuter()
        {
            return selectedExecuter;
        }

        /// <summary>
        /// Gets the executer at index <paramref name="index"/>.
        /// <para>
        /// This operation performs a liniar search.
        /// </para>
        /// </summary>
        public static CommandExecuter GetExecuter(int index)
        {
            return executers.ElementAt(index);
        }

        #region SelectExecuter Method

        /// <summary>
        /// Selects the <see cref="CommandExecuter"/> on which commands are executed on.
        /// </summary>
        public static void SelectExecuter(CommandExecuter executer)
        {
            selectedExecuter = executer;
        }

        /// <summary>
        /// Selects the <see cref="CommandExecuter"/> on which commands are executed on at <paramref name="index"/> in the current list of executers.
        /// </summary>
        public static void SelectExecuter(int index)
        {
            selectedExecuter = executers.ElementAt(index);
        }

        #endregion

        /// <summary>
        /// Executes a command method. A command method cannot be <see langword="static"/> and must return <seealso cref="void"/>.
        /// </summary>
        internal static async Task Execute(Command cmd)
        {
            //Empty command check
            if (cmd.Equals(Command.Empty))
            {
                if (selectedExecuter.LogExceptions) logger.WriteLog(null, true, "cannot execute an empty command", ConsoleColor.Red);
                if (selectedExecuter.Throw) throw new ArgumentException("cannot execute empty command", "cmd");
                return;
            }

            //Get method
            var argTypes = cmd.Arguments.Select(x => x.GetType()).ToArray();
            var method = cmd.Method;

            //Method check
            if (method is null || !Attribute.IsDefined(method, typeof(CommandAttribute)) || (Attribute.IsDefined(method, typeof(AsyncStateMachineAttribute)) && method.ReturnType != typeof(Task)) || (!Attribute.IsDefined(method, typeof(AsyncStateMachineAttribute)) && method.ReturnType != typeof(void)) || cmd.Executer is null)
            {
                //Either the executer is null or the attribute isn't defined.
                string argName = cmd.Executer is null ? nameof(cmd.Executer) : nameof(cmd.Method);

                CommandException(cmd, string.IsNullOrEmpty(argName) ? new ArgumentException() : new ArgumentException(argName == nameof(cmd.Executer) ? "Executer cannot be null." :
                                                                                                      argName == nameof(cmd.Method) ?
                                                                                                      (method.ReturnType != typeof(void) ? $"Method {method.Name} mustn't return anything." :
                                                                                                       method.ReturnType != typeof(Task) ? $"Async method {method.Name} must only return a Task." :
                                                                                                                                           $"Method {method.Name} mmust be a command.") :
                                                                                                      "Command is invalid."));
            }

            //Invoke and log if necessary
            var result = method.Invoke(cmd.Executer, cmd.Arguments);

            //Async check
            if (result is Task) await (result as Task);

            selectedExecuter.CommandExecuted?.Invoke(selectedExecuter, new CommandExecutedEventArgs(cmd));
            if (selectedExecuter.LogExecutions && !string.IsNullOrEmpty(cmd.Text)) logger.WriteLog(null, true, selectedExecuter.GetType().Name + ": " + cmd.Text, ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs and throws an exception based on a command.
        /// </summary>
        static void CommandException(string text, Exception e)
        {
            selectedExecuter.CommandParseFailed?.Invoke(selectedExecuter, new CommandParseFailedEventArgs(text, e));
            if (selectedExecuter.LogExceptions) logger.WriteLog(null, true, selectedExecuter.GetType().Name + ": " + e.Message, ConsoleColor.Red);
            if (selectedExecuter.Throw) throw e;
        }

        /// <summary>
        /// Logs and throws an exception based on a command.
        /// </summary>
        static void CommandException(Command cmd, Exception e)
        {
            selectedExecuter.CommandFailed?.Invoke(selectedExecuter, new CommandFailedEventArgs(cmd, e));
            if (selectedExecuter.LogExceptions) logger.WriteLog(null, true, selectedExecuter.GetType().Name + ": " + e.Message, ConsoleColor.Red);
            if (selectedExecuter.Throw) throw e;
        }
    }
}
