using Haze.Packets;

#region Warnings
 
#pragma warning disable CS1591 //Contructor of struct left undescribed
 
#endregion

namespace Haze
{
    /// <summary>
    /// Encapsulates the time a <see cref="Client"/> takes to send and recieve a packet from the server.
    /// </summary>
    public struct NetworkSpeed
    {
        /// <summary>
        /// The time a <see cref="PingPacket"/> sent by a <see cref="Client"/> took to reach its <see cref="Server"/>.
        /// </summary>
        public double UploadSpeed { get; }

        /// <summary>
        /// The time a <see cref="PingPacket"/> sent to the <see cref="Server"/> took to reach back to the corresponding <see cref="Client"/>.
        /// </summary>
        public double DownloadSpeed { get; }

        /// <summary>
        /// The total time a <see cref="PingPacket"/> took to be transported from a <see cref="Client"/> to a <see cref="Server"/> and back.
        /// </summary>
        public double Speed 
        { 
            get => UploadSpeed + DownloadSpeed;
        }

        public NetworkSpeed(double upSpeed, double downSpeed)
        {
            (UploadSpeed, DownloadSpeed) = (upSpeed, downSpeed);
        }
    }
}