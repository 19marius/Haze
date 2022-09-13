using System.Net.Mail;

namespace HazeClient.Users
{
    /// <summary>
    /// Represents a log in session.
    /// </summary>
    public class LoginSession
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called whenever a password confirmation fails.
        /// </summary>
        public delegate void PasswordConfimationFailEventHandler(object sender, PasswordConfimationFailEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Gets the address used by this session.
        /// </summary>
        public MailAddress Address { get; set; }

        /// <summary>
        /// Gets the password used by this session.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets a value indicating wether this session requires signing in.
        /// </summary>
        public bool SignInRequired { get; set; }

        /// <summary>
        /// Gets a value indicating wether this session will make the user's account persistent across multiple sessions.
        /// </summary>
        public bool ShouldPersist { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a password confirmation fails.
        /// </summary>
        public event PasswordConfimationFailEventHandler PasswordConfimationFail;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginSession"/> class.
        /// </summary>
        public LoginSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginSession"/> class, utilizing an address and a password.
        /// </summary>
        public LoginSession(MailAddress address, string password)
        {
            Address = address;
            Password = password;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginSession"/> class, utilizing an address and a password, and specifying if this session requires signing in.
        /// </summary>
        public LoginSession(MailAddress address, string password, bool signInRequired)
        {
            Address = address;
            Password = password;
            SignInRequired = signInRequired;
        }

        #endregion

        /// <summary>
        /// Confirms a password against the initial password. If not equal, invokes the PasswordConfirmationFail event.
        /// </summary>
        public void ConfirmPassword(string confirmedPassword)
        {
            if (SignInRequired && !confirmedPassword.Equals(Password)) PasswordConfimationFail?.Invoke(this, new PasswordConfimationFailEventArgs(Password, confirmedPassword));
        }
    }
}
