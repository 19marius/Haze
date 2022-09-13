using System;

namespace HazeClient.Text
{
    /// <summary>
    /// Represents a handler for links in <see cref="FormattedText"/> objects.
    /// </summary>
    public class LinkHandler
    {
        /// <summary>
        /// Gets the index of the first character of the text over which this effect group is applied to in the unformatted string.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the the text over which this link is applied to.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the effect which will be applied when clicking on the affected text of the Owner object.
        /// </summary>
        public FormattedTextEffect ClickEffect { get; }

        /// <summary>
        /// Gets the action this <see cref="LinkHandler"/> refers to.
        /// </summary>
        public Action Handler { get; set; }

        /// <summary>
        /// Gets a value indicating if this <see cref="LinkHandler"/> is active. That is, if the click effect and handler execute.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinkHandler"/> class from an owner and the speicfied action the link refers to.
        /// </summary>
        public LinkHandler(FormattedTextEffect effect, string text, int index, Action handler)
        {
            Text = text;
            Index = index;
            Handler = handler;
            ClickEffect = effect;
        }
    }
}
