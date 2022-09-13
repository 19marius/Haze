using System.Threading.Tasks;
using System.Reflection;
using System;

#region Warnings

#pragma warning disable CS1591

#endregion

namespace Haze.Commands
{
    /// <summary>
    /// When used on a non-static method, it can be invoked from the <see cref="CommandExecuter"/> class.
    /// <para>
    /// Methods marked with this attribute can only contain arguments which can be represented as a string.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        
    }

    /// <summary>
    /// Represents a method that can be invoked through <see cref="CommandExecuter"/>.
    /// </summary>
    public struct Command
    {
        #region Static Fields

        public static readonly Command Empty = new Command();

        #endregion

        /// <summary>
        /// The object on which the command is executed on.
        /// </summary>
        public CommandExecuter Executer { get; }

        /// <summary>
        /// The name of the method which represents the command.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// The arguments of the command.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// The text from which this <see cref="Command"/> was parsed.
        /// <para>
        /// If this <see cref="Command"/> wasn't parsed, this value is <see langword="null"/>.
        /// </para>
        /// </summary>
        public string Text { get; }

        #region Constructors

        public Command(MethodInfo method, params object[] args)
        {
            (Executer, Method, Arguments, Text) = (CommandExecuter.selectedExecuter, method, args, null);
        }

        internal Command(CommandExecuter obj, MethodInfo method, string text, params object[] args)
        {
            (Executer, Method, Arguments, Text) = (obj, method, args, text);
        }

        #endregion

        /// <summary>
        /// Executes the method represented by this <see cref="Command"/>.
        /// </summary>
        public async Task Execute()
        {
            await CommandExecuter.Execute(this);
        }
    }
}
