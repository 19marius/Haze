using System;

namespace Haze.Logging
{
    /// <summary>
    /// Represents the data of a log written by a <see cref="Logger"/>.
    /// </summary>
    public class LogWrittenEventArgs : EventArgs
    {
        /// <summary>
        /// The log that was written to the logger's corresponding outputs.
        /// </summary>
        public Log Log { get; }

        /// <summary>
        /// Creates a new instace of the <see cref="LogWrittenEventArgs"/> class with the log that was written.
        /// </summary>
        internal LogWrittenEventArgs(Log log)
        {
            Log = log;
        }
    }

    /// <summary>
    /// Represents the conditions in which a <see cref="Log"/> will be written by a <see cref="Logger"/>.
    /// </summary>
    public class PreviewLogWriteEventArgs : EventArgs
    {
        /// <summary>
        /// Checks if the <see cref="Log"/> will be written to the console.
        /// </summary>
        public bool WriteToConsole { get; }

        /// <summary>
        /// Checks if the <see cref="Log"/> will be written to a file.
        /// </summary>
        public bool WriteToFile { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="PreviewLogWriteEventArgs"/> class with the specified arguments.
        /// </summary>
        internal PreviewLogWriteEventArgs(bool console, bool file)
        {
            (WriteToConsole, WriteToFile) = (console, file);
        }
    }
}
