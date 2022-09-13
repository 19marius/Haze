using System.Collections.Generic;
using Haze.Packets;
using System.Linq;
using System.IO;
using System;

namespace Haze.Logging
{
    /// <summary>
    /// Logs messages to the console or a file.
    /// </summary>
    public class Logger : IDisposable
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) called when a <see cref="Log"/> was written to a <see cref="Logger"/>'s corresponding output.
        /// </summary>
        public delegate void LogWrittenEventHandler(object sender, LogWrittenEventArgs e);

        /// <summary>
        /// Represents the method(s) called before a <see cref="Log"/> is written to a <see cref="Logger"/>'s corresponding output.
        /// </summary>
        public delegate void PreviewLogWriteEventHandler(object sender, PreviewLogWriteEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// The owner of this <see cref="Logger"/>.
        /// </summary>
        public RemoteUnit Owner { get; }

        /// <summary>
        /// The file this <see cref="Logger"/> should log to.
        /// </summary>
        public string LogFile
        {
            get => logFile;
            set
            {
                logFile = value;
                logStream?.Dispose();
                logStream = new StreamWriter(value, true);
            }
        }

        /// <summary>
        /// Determines if this <see cref="Logger"/> should write logs to the console.
        /// </summary>
        public bool LogToConsole { get; set; }

        /// <summary>
        /// Determines if this <see cref="Logger"/> should write logs to a file.
        /// <para>
        /// An exception will be thrown if this property is set to <see langword="true"/> and LogFile is <see langword="null"/> or empty.
        /// </para>
        /// </summary>
        public bool LogToFile
        {
            get => logToFile;
            set
            {
                logToFile = value;
                if (value && string.IsNullOrEmpty(LogFile)) throw new InvalidOperationException("The log file path cannot be null or empty.");
            }
        }

        /// <summary>
        /// Determines if written logs are to include date and time deatils.
        /// </summary>
        public bool DisableTimeDetails { get; set; } = false;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when a <see cref="Log"/> was written to the according output.
        /// </summary>
        public event LogWrittenEventHandler LogWritten;

        /// <summary>
        /// Invoked before a <see cref="Log"/> was written to te according output.
        /// </summary>
        public event PreviewLogWriteEventHandler PreviewLogWrite;

        #endregion

        #region Indexer

        /// <summary>
        /// Returns the <see cref="Log"/> at the specified index.
        /// <para>
        /// Indexing is instance dependent. That is, if two <see cref="Logger"/>s write to the same file, <paramref name="index"/> will only keep track of the logs that belong to its instance.
        /// </para>
        /// </summary>
        public Log this[int index]
        {
            get => logs[index];
        }

        #endregion

        #region Fields

        List<Log> logs = new List<Log>(100);
        StreamWriter logStream = null;
        string logFile = null;
        bool logToFile;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="Logger"/> class with a specific <see cref="RemoteUnit"/> as its owner.
        /// <para>
        /// This <see cref="Logger"/> will automatically write logs to the console.
        /// </para>
        /// </summary>
        public Logger(RemoteUnit owner)
        {
            (Owner, LogToConsole) = (owner, true);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Logger"/> class with a specific <see cref="RemoteUnit"/> as its owner and a file this <see cref="Logger"/> should write logs to.
        /// </summary>
        public Logger(RemoteUnit owner, string filename)
        {
            (Owner, LogFile, LogToConsole) = (owner, filename, false);
        }

        #endregion

        #region WriteLog Method

        /// <summary>
        /// Writes a simple message to the corresponding output.
        /// </summary>
        public void WriteLog(RemoteUnit peer, bool isSender, object message)
        {
            //Create the log and get the time
            Log log;
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            logs.Add(log = new Log(isSender ? peer : Owner, !isSender ? peer : Owner, time, time, message.ToString()));

            PreviewLogWrite?.Invoke(this, new PreviewLogWriteEventArgs(LogToConsole, LogToFile));

            //Log to the according output
            if (LogToConsole)
            {
                //Reset color
                Console.ResetColor();

                //Keep the deatils uncolored
                if (!DisableTimeDetails) Console.Write("[" + log.GetTimeDetails() + "] ");
                var resetColor = Console.ForegroundColor;

                //Write the sender
                WriteUnit(log.Sender, resetColor);

                Console.Write(log.Sender != null && log.Reciever != null ? " to " : "");

                //Write the reciever
                WriteUnit(log.Reciever, resetColor);

                Console.Write(log.Sender is null && log.Reciever is null ? "" : ": ");

                Console.Write(log.Content);
                Console.WriteLine("");
            }

            if (LogToFile) logStream.WriteLine(log);

            LogWritten?.Invoke(this, new LogWrittenEventArgs(log));
        }

        /// <summary>
        /// Writes a <see cref="Log"/> to the corresponding output.
        /// </summary>
        public void WriteLog(RemoteUnit peer, bool isSender, Packet content)
        {
            //Create the log
            Log log;
            logs.Add(log = new Log(isSender ? peer : Owner, !isSender ? peer : Owner, TimeZoneInfo.ConvertTime(content.SendTime, TimeZoneInfo.Local), TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local), content.Unpack()?.ToString() ?? content.ToString()));

            //Log to the according output
            if (LogToConsole)
            {
                //Reset color
                Console.ResetColor();

                //Keep the deatils uncolored
                if (!DisableTimeDetails) Console.Write("[" + log.GetTimeDetails() + "] ");
                var resetColor = Console.ForegroundColor;

                //Write the sender
                WriteUnit(log.Sender, resetColor);

                Console.Write(log.Sender != null && log.Reciever != null ? " to " : "");

                //Write the reciever
                WriteUnit(log.Reciever, resetColor);

                Console.Write(log.Sender is null && log.Reciever is null ? "" : ": ");

                Console.Write(log.Content);
                Console.WriteLine("");
            }

            if (LogToFile) logStream.WriteLine(log);

            LogWritten?.Invoke(this, new LogWrittenEventArgs(log));
        }

        /// <summary>
        /// Writes a <see cref="Log"/> to the corresponding output.
        /// </summary>
        public void WriteLog(RemoteUnit peer, bool isSender, Packet content, object addition)
        {
            //Create the log
            Log log;
            logs.Add(log = new Log(isSender ? peer : Owner, !isSender ? peer : Owner, TimeZoneInfo.ConvertTime(content.SendTime, TimeZoneInfo.Local), TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local), (content.Unpack()?.ToString() ?? content.ToString()) + addition));

            //Log to the according output
            if (LogToConsole)
            {
                //Reset color
                Console.ResetColor();

                //Keep the deatils uncolored
                if (!DisableTimeDetails) Console.Write("[" + log.GetTimeDetails() + "] ");
                var resetColor = Console.ForegroundColor;

                //Write the sender
                WriteUnit(log.Sender, resetColor);

                Console.Write(log.Sender != null && log.Reciever != null ? " to " : "");

                //Write the reciever
                WriteUnit(log.Reciever, resetColor);

                Console.Write(log.Sender is null && log.Reciever is null ? "" : ": ");

                Console.Write(log.Content);
                Console.WriteLine("");
            }

            if (LogToFile) logStream.WriteLine(log);

            LogWritten?.Invoke(this, new LogWrittenEventArgs(log));
        }

        /// <summary>
        /// Writes a simple message to the corresponding output. The text is written in <paramref name="color"/> if the log is written to the console.
        /// </summary>
        public void WriteLog(RemoteUnit peer, bool isSender, object message, ConsoleColor color)
        {
            //Create the log
            Log log;
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            logs.Add(log = new Log(isSender ? peer : Owner, !isSender ? peer : Owner, time, time, message.ToString()));

            //Log to the according output
            if (LogToConsole)
            {
                //Reset color
                Console.ResetColor();

                //Keep the deatils uncolored
                if (!DisableTimeDetails) Console.Write("[" + log.GetTimeDetails() + "] ");
                var resetColor = Console.ForegroundColor;

                //Write the sender
                WriteUnit(log.Sender, resetColor);

                Console.Write(log.Sender != null && log.Reciever != null ? " to " : "");

                //Write the reciever
                WriteUnit(log.Reciever, resetColor);

                Console.Write(log.Sender is null && log.Reciever is null ? "" : ": ");

                Console.ForegroundColor = color;
                Console.Write(log.Content);
                Console.WriteLine("");

                //Log & reset color
                Console.ForegroundColor = resetColor;
            }

            if (LogToFile) logStream.WriteLine(log);

            LogWritten?.Invoke(this, new LogWrittenEventArgs(log));
        }

        /// <summary>
        /// Writes a <see cref="Log"/> to the corresponding output. The <see cref="Log"/> is written in <paramref name="color"/> if the log is written to the console.
        /// </summary>
        public void WriteLog(RemoteUnit peer, bool isSender, Packet content, ConsoleColor color)
        {
            //Create the log
            Log log;
            logs.Add(log = new Log(isSender ? peer : Owner, !isSender ? peer : Owner, TimeZoneInfo.ConvertTime(content.SendTime, TimeZoneInfo.Local), TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local), content.Unpack()?.ToString() ?? content.ToString()));

            //Log to the according output
            if (LogToConsole)
            {
                //Reset color
                Console.ResetColor();

                //Keep the deatils uncolored
                if (!DisableTimeDetails) Console.Write("[" + log.GetTimeDetails() + "] ");
                var resetColor = Console.ForegroundColor;

                //Write the sender
                WriteUnit(log.Sender, resetColor);

                Console.Write(log.Sender != null && log.Reciever != null ? " to " : "");

                //Write the reciever
                WriteUnit(log.Reciever, resetColor);

                Console.Write(log.Sender is null && log.Reciever is null ? "" : ": ");

                Console.ForegroundColor = color;
                Console.Write(log.Content);
                Console.WriteLine("");

                //Log & reset color
                Console.ForegroundColor = resetColor;
            }

            if (LogToFile) logStream.WriteLine(log);

            LogWritten?.Invoke(this, new LogWrittenEventArgs(log));
        }

        /// <summary>
        /// Writes a <see cref="Log"/> to the corresponding output. The <see cref="Log"/> is written in <paramref name="color"/> if the log is written to the console.
        /// </summary>
        public void WriteLog(RemoteUnit peer, bool isSender, Packet content, object addition, ConsoleColor color)
        {
            //Create the log
            Log log;
            logs.Add(log = new Log(isSender ? peer : Owner, !isSender ? peer : Owner, TimeZoneInfo.ConvertTime(content.SendTime, TimeZoneInfo.Local), TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local), (content.Unpack()?.ToString() ?? content.ToString()) + addition));

            //Log to the according output
            if (LogToConsole)
            {
                //Reset color
                Console.ResetColor();

                //Keep the deatils uncolored
                if (!DisableTimeDetails) Console.Write("[" + log.GetTimeDetails() + "] ");
                var resetColor = Console.ForegroundColor;

                //Write the sender
                WriteUnit(log.Sender, resetColor);

                Console.Write(log.Sender != null && log.Reciever != null ? " to " : "");

                //Write the reciever
                WriteUnit(log.Reciever, resetColor);

                Console.Write(log.Sender is null && log.Reciever is null ? "" : ": ");

                Console.ForegroundColor = color;
                Console.Write(log.Content);
                Console.WriteLine("");

                //Log & reset color
                Console.ForegroundColor = resetColor;
            }

            if (LogToFile) logStream.WriteLine(log);

            LogWritten?.Invoke(this, new LogWrittenEventArgs(log));
        }

        #endregion

        /// <summary>
        /// Releases all resources used by the <see cref="StreamWriter"/> this <see cref="Logger"/> used to write to.
        /// </summary>
        public void Dispose()
        {
            LogWritten = null;
            logStream?.Dispose();
        }

        /// <summary>
        /// Clears the logs and resets the capacity of this <see cref="Logger"/>.
        /// </summary>
        public void Clear()
        {
            logs.Clear();
            logs.TrimExcess();
            logs.Capacity = 100;
        }

        /// <summary>
        /// Saves all the current logs to <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename)
        {
            File.WriteAllLines(filename, logs.Select(x => x.ToString()).ToArray());
        }

        #region Private Methods

        /// <summary>
        /// Adds a <see cref="RemoteUnit"/> to the currently writing log.
        /// </summary>
        void WriteUnit(RemoteUnit unit, ConsoleColor colOriginal)
        {
            Console.ForegroundColor = unit?.Color ?? ConsoleColor.White;
            Console.Write(unit?.GetType().Name ?? "");
            Console.ForegroundColor = colOriginal;
            Console.Write(unit is null ? "" : " " + (unit.Name is null || unit.Name == "" ? unit.ID : unit.Name));
        }

        #endregion
    }
}
