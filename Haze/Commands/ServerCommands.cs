using System.Text.RegularExpressions;
using System.ComponentModel;
using Haze.Commands;
using System.Linq;

#region Warnings

#pragma warning disable CS1591 //Commands should only have the description attribute

#endregion

namespace Haze
{
    public partial class Server
    {
        [Command]
        [Description("displays info about the client at the specified index")]
        public void ShowClient(int index)
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, "client " + index + " has the respective properties:\r\n" + string.Join("\r\n", typeof(ServerClient).GetProperties().Where(x => Regex.IsMatch(x.Name, @"ConnectionTime|Tags|Name|ID|IsConnected")).Select(x => x.Name + ": " + x.GetValue(this[index]))));
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays the current number of clients")]
        public void ClientCount()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, CurrentClients);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("removes the client at the specified index")]
        public void Kick(int index, string reason)
        {
            //Index check
            if (index < 0 || index >= clients.Count) return;
            this[index].Disconnect(reason);
        }

        [Command]
        [Description("removes ALL clients with the specified names")]
        public void Kick(string[] names, string reason)
        {
            for (int i = 0; i < names.Length; i++) foreach (var client in this.Where(x => x.Name == names[i])) client.Disconnect(reason);
        }

        [Command]
        [Description("removes the first occurence of clients with the specified names")]
        public void KickFirst(string[] names, string reason)
        {
            for (int i = 0; i < names.Length; i++) this.Where(x => x.Name == names[i]).FirstOrDefault()?.Disconnect(reason);
        }

        [Command]
        [Description("displays the port the server is listening for connections on")]
        public void ShowPort()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, Port);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays the max amount of clients this server can have")]
        public void Max()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, MaxClients);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("sets the number of maximum clients allowed in this server. if the current number is reduced, newer clients will be removed")]
        public void Max(int max)
        {
            MaxClients = max;
        }

        [Command]
        [Description("sends updates to every client in this server")]
        public void Update()
        {
            SendUpdates();
        }

        [Command]
        [Description("sets if sending packets will be a logged action")]
        public void LogSending(bool enable)
        {
            LogSends = enable;
        }

        [Command]
        [Description("sets if recieving packets will be a logged action")]
        public void LogRecieving(bool enable)
        {
            LogRecieves = enable;
        }
    }
}
