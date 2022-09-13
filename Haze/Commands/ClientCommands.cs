using System.Threading.Tasks;
using System.ComponentModel;
using Haze.Commands;
using System.Linq;

#region Warnings

#pragma warning disable CS1591 //Commands should only have the description attribute

#endregion

namespace Haze
{
    public partial class Client
    {
        [Command]
        [Description("displays the speed of the client")]
        public async Task Ping()
        {
            Logger.DisableTimeDetails = true;

            var ping = await GetPing();
            Logger.WriteLog(null, true, ping.Speed + "(up: " + ping.UploadSpeed + ", down: " + ping.DownloadSpeed + ")");

            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays every client in the server")]
        public void Clients()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, string.Join("\r\n", Server.Select((x, i) => "client " + i + ": " + x)));
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays the number of clients in the server")]
        public void ClientCount()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, Server.CurrentClients);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays the port number")]
        public void ShowPort()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, Port);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("shows the current number or packets sent to the server")]
        public void SendCount()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, SentData.Count);
            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays information about a client")]
        public void ShowClient(int index)
        {
            Logger.DisableTimeDetails = true;

            var client = Server[index];
            Logger.WriteLog(null, true, "client at index " + index + " has the ID of " + client.ID + " and " + (string.IsNullOrEmpty(client.Name) ? "no name" : "the name of " + client.Name));

            Logger.DisableTimeDetails = false;
        }

        [Command]
        [Description("displays your index in the server")]
        public void MyIndex()
        {
            Logger.DisableTimeDetails = true;
            Logger.WriteLog(null, true, Server.RecieverIndex);
            Logger.DisableTimeDetails = false;
        }
    }
}
