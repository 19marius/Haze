using System;

#region Warnings
 
#pragma warning disable CS1591 //Contructor of struct left undescribed
 
#endregion

namespace Haze.Logging
{
    /// <summary>
    /// Represents a message or command sent from a <see cref="RemoteUnit"/> to another.
    /// </summary>
    public struct Log
    {
        #region Properties

        /// <summary>
        /// The sender of this <see cref="Log"/>.
        /// </summary>
        public RemoteUnit Sender { get; }

        /// <summary>
        /// The reciever of this <see cref="Log"/>.
        /// </summary>
        public RemoteUnit Reciever { get; }

        /// <summary>
        /// The content of this <see cref="Log"/>. It can represent either a plain message or an executable command.
        /// <para>
        /// Commands are only executed in the context of the <see cref="Logger"/> which creates this <see cref="Log"/>.
        /// </para>
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// The time at which the message was supposedly sent.
        /// </summary>
        public DateTime SentTime { get; }

        /// <summary>
        /// The time at which the message was supposedly recieved.
        /// </summary>
        public DateTime RecievalTime { get; }

        #endregion

        #region Constructor

        public Log(RemoteUnit sender, RemoteUnit reciever, DateTime sentTime, DateTime recieveTime, string content)
        {
            (Sender, Reciever, Content, RecievalTime, SentTime) = (sender, sender is null || reciever is null ? null : sender.ID == reciever.ID ? null : reciever, content, recieveTime, sentTime);
        }

        #endregion

        /// <summary>
        /// Returns the <see cref="string"/> representation of this <see cref="Log"/>.
        /// <para>
        /// A <see cref="Log"/> is formatted as [sent {SentTime}, recieved {RecievalTime}] {Sender} to {Reciever}: {Content}
        /// </para>
        /// </summary>
        public override string ToString()
        {
            return "[sent " + SentTime + ", recieved " + RecievalTime + "] " + (Sender != null ? Sender.GetType().Name + " " + (Sender.Name is null || Sender.Name == "" ? Sender.ID : Sender.Name) : "") + (Reciever is null || Sender is null ? "" : " to ") + (Reciever != null ? Reciever.GetType().Name + " " + (Reciever.Name is null || Reciever.Name == "" ? Reciever.ID : Reciever.Name) : "") + ": " + Content;
        }

        /// <summary>
        /// Gets the user-related details of this <see cref="Log"/>.
        /// <para>
        /// The return string is formatted as "{Sender} to {Reciever}" or, if one is <see langword="null"/>, "{Sender|Reciever}", and <see cref="string.Empty"/> if both are <see langword="null"/>.
        /// </para>
        /// </summary>
        public string GetUserDetails()
        {
            return (Sender != null ? Sender.GetType().Name + " " + (Sender.Name is null || Sender.Name == "" ? Sender.ID : Sender.Name) : "") + (Reciever is null || Sender is null ? "" : " to ") + (Reciever != null ? Reciever.GetType().Name + " " + (Reciever.Name is null || Reciever.Name == "" ? Reciever.ID : Reciever.Name) : "");
        }

        /// <summary>
        /// Gets the time-related details of this <see cref="Log"/>.
        /// <para>
        /// The return string is formatted as "sent {SentTime}, recieved {RecievalTime}".
        /// </para>
        /// </summary>
        /// <returns></returns>
        public string GetTimeDetails()
        {
            return "sent " + SentTime + ", recieved " + RecievalTime;
        }
    }
}
