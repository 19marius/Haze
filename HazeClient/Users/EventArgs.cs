using System;

namespace HazeClient.Users
{
    /// <summary>
    /// Represents data for a failed password confirmation.
    /// </summary>
    public class PasswordConfimationFailEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the initial password.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Gets the confirmed password. This is different from the initial password.
        /// </summary>
        public string ConfirmedPassword { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConfimationFailEventArgs"/> class, using the initial and confirmed passwords.
        /// </summary>
        public PasswordConfimationFailEventArgs(string password, string confirmation)
        {
            Password = password;
            ConfirmedPassword = confirmation;
        }
    }
}
