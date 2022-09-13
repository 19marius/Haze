using System.Windows;

namespace HazeClient.Controls
{
    /// <summary>
    /// Represents data for a text finish event in a <see cref="RoundTextBox"/>.
    /// </summary>
    public class TextFinishedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the text after the finish.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets a value indicating wheter the text was finished by key or not.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the text was finished by pressing Enter or Escape; <see langword="false"/> if the text was finished by unfocusing the <see cref="RoundTextBox"/>.
        /// </returns>
        public bool ByKey { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFinishedEventArgs"/> class, specifying the finished text, and if the finish was caused by key-press or not.
        /// </summary>
        public TextFinishedEventArgs(string text, bool byKey)
        {
            Text = text;
            ByKey = byKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFinishedEventArgs"/> class, specifying the finished text, and if the finish was caused by key-press or not, also using the supplied routed event identifier.
        /// </summary>
        public TextFinishedEventArgs(string text, bool byKey, RoutedEvent routedEvent) : base(routedEvent)
        {
            Text = text;
            ByKey = byKey;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextFinishedEventArgs"/> class, specifying the finished text, and if the finish was caused by key-press or not, also using the supplied routed event identifier, and providing the opportunity to declare a different source for the event.
        /// </summary>
        public TextFinishedEventArgs(string text, bool byKey, RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
            Text = text;
            ByKey = byKey;
        }
    }
}
