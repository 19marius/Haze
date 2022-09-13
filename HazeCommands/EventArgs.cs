using System;

namespace Haze.Commands
{
    /// <summary>
    /// Represents data for a command which was executed.
    /// </summary>
    public class CommandExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// The command that was executed.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandExecutedEventArgs"/> class, with the specified command.
        /// </summary>
        internal CommandExecutedEventArgs(Command cmd)
        {
            Command = cmd;
        }
    }

    /// <summary>
    /// Represents data for a command which failed to execute.
    /// </summary>
    public class CommandFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The exception that caused the respective command to fail.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The command which failed to execute.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandFailedEventArgs"/> class with the failed command and the causing exception.
        /// </summary>
        internal CommandFailedEventArgs(Command cmd, Exception e)
        {
            (Command, Exception) = (cmd, e);
        }
    }

    /// <summary>
    /// Represents data for a failed command parsing operation.
    /// </summary>
    public class CommandParseFailedEventArgs : EventArgs
    {
        /// <summary>
        /// The text of the command which failed to be parsed.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// The exception that represents the failed operation.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CommandParseFailedEventArgs"/> class with the casuing command text and the exception which represents the failed operation.
        /// </summary>
        internal CommandParseFailedEventArgs(string cmd, Exception e)
        {
            (Command, Exception) = (cmd, e);
        }
    }
}
