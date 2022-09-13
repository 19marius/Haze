using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Linq;
using System;

namespace HazeClient.Text
{
    /// <summary>
    /// Represents a group of effects applied to a fragment of formatted text.
    /// </summary>
    public struct FormattedTextEffectGroup
    {
        /// <summary>
        /// Gets the index of the first character of the text over which this effect group is applied to in the unformatted string.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the the text over which this effect group is applied to.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the effects that make up this group.
        /// </summary>
        public FormattedTextEffect[] EffectGroup { get; }

        public FormattedTextEffectGroup(int index, string text, params FormattedTextEffect[] group)
        {
            Index = index;
            Text = text;
            EffectGroup = group;
        }
    }

    /// <summary>
    /// Represents a single effect in a <see cref="FormattedTextEffectGroup"/>.
    /// </summary>
    public struct FormattedTextEffect
    {
        /// <summary>
        /// Gets the effect type of this <see cref="FormattedTextEffect"/>.
        /// </summary>
        public FormattedTextEffectType Type { get; }

        /// <summary>
        /// Gets the value used for the effect.
        /// </summary>
        public object Value { get; }

        public FormattedTextEffect(FormattedTextEffectType type, object value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Converts a <see cref="char"/> to a <see cref="FormattedTextEffectType"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        internal static FormattedTextEffectType FromChar(char type)
        {
            switch (type)
            {
                case 's':
                    return FormattedTextEffectType.Shake;

                case 'w':
                    return FormattedTextEffectType.Wave;

                case 'c':
                    return FormattedTextEffectType.Color;

                case 'f':
                    return FormattedTextEffectType.FontSize;

                case 'b':
                    return FormattedTextEffectType.Bold;

                case 'i':
                    return FormattedTextEffectType.Italic;

                case 'l':
                    return FormattedTextEffectType.Link;
            }

            throw new ArgumentException("Type character does not correspond to any FormattedTextEffectType.");
        }
    }

    /// <summary>
    /// Represents a formatted text effect types.
    /// </summary>
    public enum FormattedTextEffectType
    {
        /// <summary>
        /// Text with this effect will shake depending on the specified intensity. This effect is of type <see cref="double"/>. <para>In a formatting pattern, this value corresponds to 's'.</para>
        /// </summary>
        Shake,

        /// <summary>
        /// Text with this effect will move in a wave pattern depending on the specified intensity. This effect is of type <see cref="double"/>. <para>In a formatting pattern, this value corresponds to 'w'.</para>
        /// </summary>
        Wave,

        /// <summary>
        /// Text with this effect will have the specified color, or, if two colors are specified, cycle between them. This effect is either of type <see cref="System.Windows.Media.Color"/> if a single color is selected, or <see cref="ColorAnimation"/> otherwise. <para>In a formatting pattern, this value corresponds to 'c'.</para>
        /// </summary>
        Color,

        /// <summary>
        /// Text with this effect will have its font size amplified by the specified value. This effect is of type <see cref="double"/>. <para>In a formatting pattern, this value corresponds to 'f'.</para>
        /// </summary>
        FontSize,

        /// <summary>
        /// Text with this effect will be bold. This effect is of type <see cref="bool"/> and its value will be <see langword="true"/> by default. <para>In a formatting pattern, this value corresponds to 'b'.</para>
        /// </summary>
        Bold,

        /// <summary>
        /// Text with this effect will be inclined. This effect is of type <see cref="bool"/> and its value will be <see langword="true"/> by default. <para>In a formatting pattern, this value corresponds to 'i'.</para>
        /// </summary>
        Italic,

        /// <summary>
        /// Text with this effect will refer to an action. This effect is of type <see cref="LinkHandler"/>. <para>In a formatting pattern, this value corresponds to 'l' and can only be assigned at most one effect that can be in a formatting pattern, that effect, if existent, activating when the affected text is clicked. Any other action the link should refer to will be assigned manually.</para>
        /// </summary>
        Link
    }
}
