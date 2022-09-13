using System.Linq.Extensions;
using System.ComponentModel;
using Haze.Commands;
using System.Linq;
using System.IO;
using System;

#region Warnings

#pragma warning disable CS1591 //Commands should only have the description attribute

#endregion

namespace Haze
{
    public abstract partial class RemoteUnit
    {
        [Command]
        [Description("displays info about every enabled command")]
        public void Help()
        {
            Logger.DisableTimeDetails = true;
            string output = string.Join("\r\n", GetType().GetMethods().Where(x => Attribute.IsDefined(x, typeof(CommandAttribute)) && (!IsDisabled(x.Name.LowerFirst()) ?? false)).Select(x => x.Name.LowerFirst() + (x.GetParameters().Length == 0 ? "" : "(" + string.Join(", ", x.GetParameters().Select(p => p.Name + ": " + p.ParameterType.Name)) + ")") + ": " + (Attribute.GetCustomAttribute(x, typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description));
            Logger.WriteLog(null, true, string.IsNullOrEmpty(output) ? "no commands to show" : output);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays info about an enabled command")]
        public void Help(string cmd)
        {
            Logger.DisableTimeDetails = true;
            string output = string.Join("\r\n", GetType().GetMethods().Where(x => Attribute.IsDefined(x, typeof(CommandAttribute)) && x.Name == cmd.UpperFirst() && (!IsDisabled(x.Name.LowerFirst()) ?? false)).Select(x => x.Name.LowerFirst() + (x.GetParameters().Length == 0 ? "" : "(" + string.Join(", ", x.GetParameters().Select(p => p.Name + ": " + p.ParameterType.Name)) + ")") + ": " + (Attribute.GetCustomAttribute(x, typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description));
            Logger.WriteLog(null, true, string.IsNullOrEmpty(output) ? "command not found" : output);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays this unit's name")]
        public void ShowName()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, string.IsNullOrEmpty(Name) ? "name is empty" : Name);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays this unit's id")]
        public void ShowId()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, ID);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("sets this unit's name")]
        public void SetName(string name)
        {
            Name = name;
        }

        [Command]
        [Description("sets the file logs will be sent to")]
        public void SetLogFile(string file)
        {
            if (!File.Exists(file))
            {
                Logger.DisableTimeDetails = true;
                Logger.WriteLog(null, true, "no such file exists", ConsoleColor.Red);
                Logger.DisableTimeDetails = false;

                return;
            }

            Logger.LogFile = file;
        }

        [Command]
        [Description("determines if logs are written to a file")]
        public void LogToFile(bool enable)
        {
            Logger.LogToFile = enable;
        }

        [Command]
        [Description("determines if logs are written to the console")]
        public void LogToConsole(bool enable)
        {
            Logger.LogToConsole = enable;
        }
    }
}
