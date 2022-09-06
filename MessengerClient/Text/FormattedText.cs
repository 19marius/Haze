using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using MessengerClient.Controls;
using System.Windows.Media;
using System.Linq;
using System;

namespace MessengerClient.Text
{
    /// <summary>
    /// Represents a string of formatted text to be used in a <see cref="FormattedTextBlock"/>.
    /// </summary>
    public class FormattedText
    {
        #region Properties

        /// <summary>
        /// Gets the unformatted text represented by this instance.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the groups of effects that should be applied to the text.
        /// </summary>
        public FormattedTextEffectGroupCollection Effects { get; }

        /// <summary>
        /// Gets the modifiers applied to this instance.
        /// </summary>
        public FormattedTextModifier[] Modifiers { get; }

        #endregion

        /// <summary>
        /// Gets the character at <paramref name="index"/> in the unformatted string.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>The character at <paramref name="index"/> in the unformatted string.</returns>
        public char this[int index]
        {
            get => Text[index];
        }

        #region Fields

        string formatted;

        #endregion

        FormattedText(string formatted, string text, FormattedTextEffectGroupCollection effects, params FormattedTextModifier[] modifiers)
        {
            Text = text;
            Effects = effects;
            this.formatted = formatted;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Returns the text represented by this instance, with formatting applied.
        /// </summary>
        public string AsFormatted()
        {
            return formatted;
        }

        /// <summary>
        /// Parses a string of formatted text into a <see cref="FormattedText"/> object.
        /// <para>
        /// A formatting pattern will appear as <c>(</c>{<c>effectType</c>}{<c>value</c>}<c>,</c>etc.<c>)[</c>{<c>affectedText</c>}<c>]</c>. Any brackets inside <c>affectedText</c> have to be escaped with \.
        /// </para>
        /// <para>
        /// <c>effectType</c> can be the following:
        /// <list type="bullet">
        /// <item>s (double): <c>affectedText</c> will shake with the intensity of <c>value</c>.</item>
        /// <item>w (double): <c>affectedText</c> will wave with the intensity of <c>value</c>.</item>
        /// <item>f (double): The font size of <c>affectedText</c> will be amplified by <c>value</c>.</item>
        /// <item>c (color(s)): <c>afftectedText</c> will have the color <c>value</c>. Two colors can be specified through a dash; this will animate <c>value</c> between the two colors.</item>
        /// <item>b (no value): <c>afftectedText</c> will be bold.</item>
        /// <item>i (no value): <c>afftectedText</c> will be italic.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="formattedText">
        /// The string of formatted text to be parsed into a <see cref="FormattedText"/> object. Any desired modifiers for the <see cref="FormattedText"/> will be specified at the beginning of the string.
        /// <para>
        /// The modifiers appear as <c>&lt;</c>{<c>modifierType</c>}<c>,</c>etc.<c>&gt;</c>{<c>text</c>}.
        /// </para>
        /// <para>
        /// <c>modifierType</c> can be the following:
        /// <list type="bullet">
        /// <item>- : No modifiers. If used, no other modifiers can be specified.</item>
        /// <item>i : In a <see cref="FormattedTextBlock"/>, <c>text</c> will appear instantly instead of fading in. Previous text will also disappear instantly instead of fading out.</item>
        /// <item>r : In a <see cref="FormattedTextBlock"/>, each character of <c>text</c> will appear instantly instead of fading in. Different to the 'i' modifier, the character dealy will still apply.</item>
        /// </list>
        /// </para>
        /// </param>
        public static FormattedText Parse(string formattedText)
        {
            //If the string is empty, just return
            if (string.IsNullOrEmpty(formattedText)) return new FormattedText("", "", new FormattedTextEffectGroupCollection());

            //Get the modifier section and remove it from the original string
            var modifiers = Regex.Replace(Regex.Match(formattedText, @"(?<=^<)([a-z](,[a-z])*|-)(?=>)").Value, @" +", "").Split(',').Where(x => !x.Equals("-") && x.Length >= 1).Select(x => FromChar(x[0])).ToArray();
            formattedText = Regex.Replace(formattedText, @"^<([a-z](,[a-z])*|-)>", "");

            //Use a copy of the formatted text to later store the unformatted version
            string originalText = string.Copy(formattedText);

            //The regex for the formatting pattern
            var pattern = new Regex(@"\((s|c|w|f|b|i|l)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|((s|c|w|f|b|i)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|))|)(,(s|c|w|f|b|i|l)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|((s|c|w|f|b|i)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|))|))*\)\[(([^()\[\]]*(\\\]|\\\[|\\\(|\\\))*)*|((\\\]|\\\[|\\\(|\\\))*[^()\[\]]*)*)(?<!\\)\]");

            //Get all patterns and create a list to store all original matches
            var matches = pattern.Matches(formattedText).Cast<Match>().ToList();
            var matchesOriginal = new List<string>();

            //Check for syntax errors
            for (int i = 0; i < matches.Count; i++)
            {
                //Check for the two types of errors
                var duplicationMatch = Regex.Match(matches[i].Value, @"^\((?<=(.*))((s|w|c|f|b|i|l)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|((s|c|w|f|b|i)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|))|).*(,\3(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|((s|c|w|f|b|i)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|))|)))\)\[").Value;
                var invalidMatch = Regex.Match(Regex.Match(matches[i].Value, @"^\(.+\)(?=\[)").Value, @"(?<=[,(])(c(?!#[A-Fa-f0-9]{6})|[swf](?!\d+(\.\d+)?)|[bi](?![,)])|l(?!([,)]|c#[A-Fa-f0-9]{6}|[swf]\d+(\.\d+)?|[bi][,)])))").Value;

                //If any error is not empty, throw
                if (!string.IsNullOrEmpty(duplicationMatch) || !string.IsNullOrEmpty(invalidMatch)) throw new InvalidFormatPatternExcpetion(!string.IsNullOrEmpty(duplicationMatch) ? "A group cannot contain 2 identical effect types." : "An effect type was assigned an invalid value.",
                                                                                                                                            Regex.Replace(Regex.Replace(matches[i].Value, @"^\(.+\)\[(.+)\]$", @"$1"), @"\\(?=\)|\(|\]|\[)", @""),
                                                                                                                                            !string.IsNullOrEmpty(duplicationMatch) ? duplicationMatch : invalidMatch);
            }

            //Replace the patterns in the copy with the patterns' respective strings and store 
            for (int i = 0; i < matches.Count; i++)
            {
                matchesOriginal.Add(Regex.Replace(Regex.Replace(matches[i].Value, @"^\(.+\)\[(.+)\]$", @"$1"), @"\\(?=\)|\(|\]|\[)", @""));
                originalText = originalText.Replace(matches[i].Value, matchesOriginal[i]);
            }

            //Get the number of skips each match has to do, in case of duplicates, and add the groups to a collection
            var collection = new FormattedTextEffectGroupCollection();
            var skips = matchesOriginal.ToDictionary(x => x, x => 0);
            for (int i = 0; i < matches.Count; i++)
            {
                //Get the only effect group string, excluding the text and get the starting index of the affected text
                string effectGroup = Regex.Match(matches[i].Value, @"(?<=^\().+(?=\)\[)").Value;
                int index = Regex.Matches(originalText, Regex.Escape(matchesOriginal[i]))[(skips[matchesOriginal[i]] += 1) - 1].Index;

                //Parse the effect group string into a FormattedTextEffectGroup
                var effects = Regex.Matches(effectGroup, @"[swcfbil](\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|((s|c|w|f|b|i)(\d+(\.\d+)?|#[a-fA-F0-9]+(-#[a-fA-F0-9]+)?|))|)").Cast<Match>().Select(x => FromPatternEffect(matchesOriginal[i], index, x.Value));

                //Add to the collection
                collection.Add(new FormattedTextEffectGroup(index, matchesOriginal[i], effects.ToArray()));
            }

            //Create the FormattedText object
            var text = new FormattedText(formattedText, originalText, collection, modifiers);
            return text;
        }

        /// <summary>
        /// Gets a <see cref="FormattedTextEffect"/> object from an effect in a formatting pattern.
        /// </summary>
        /// <param name="effect">A single effect from a formatting pattern</param>
        /// <param name="text">The affected text.</param>
        /// <param name="index">The index of the affected text's first character in the unformatted string.</param>
        static FormattedTextEffect FromPatternEffect(string text, int index, string effect)
        {
            //Remove the effect type character
            var str = effect.Substring(1);

            //Go through all possible value cases
            return new FormattedTextEffect(FormattedTextEffect.FromChar(effect[0]), !effect.StartsWith("l") ? str.Length >= 1 ? str.Contains("#") ? str.Contains("-") ? new ColorAnimation() { To = (Color)ColorConverter.ConvertFromString(Regex.Match(str, @"^#[a-fA-F0-9]+(?=-)").Value), From = (Color)ColorConverter.ConvertFromString(Regex.Match(str, @"(?<=-)#[a-fA-F0-9]+$").Value) } : //Animation case
                                                                                                                                                                     ColorConverter.ConvertFromString(str) : //Single color case
                                                                                                                                                                     Convert.ToDouble(str) : //Double case
                                                                                                                                                                     true : //Bold or italic case
                                                                                                                                                                     new LinkHandler(effect.Length == 1 ? default : FromPatternEffect(text, index, str), text, index, null)); //Link case
        }

        /// <summary>
        /// Converts a <see cref="char"/> to a <see cref="FormattedTextModifier"/>.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        static FormattedTextModifier FromChar(char type)
        {
            switch (type)
            {
                case 'i':
                    return FormattedTextModifier.Instant;

                case 'r':
                    return FormattedTextModifier.RemoveFade;
            }

            throw new ArgumentException("Type character does not correspond to any FormattedTextModifier.");
        }

        #region Operators

        public static FormattedText operator +(FormattedText t, string str)
        {
            return Parse(t.AsFormatted() + str);
        }

        public static FormattedText operator +(FormattedText text0, FormattedText text1)
        {
            return Parse(text0.AsFormatted() + text1.AsFormatted());
        }

        #endregion
    }

    /// <summary>
    /// Represents a modifier that can be applied to a <see cref="FormattedText"/> object.
    /// </summary>
    public enum FormattedTextModifier
    {
        /// <summary>
        /// In a <see cref="FormattedTextBlock"/>, the <see cref="FormattedText"/> will appear instantly. Previous text will also disappear instantly.
        /// <para>
        /// In text form, this value is equal to 'i'.
        /// </para>
        /// </summary>
        Instant,

        /// <summary>
        /// In a <see cref="FormattedTextBlock"/>, the characters of the <see cref="FormattedText"/> will no longer fade in, appearing instantly. The character dealy still applies.
        /// <para>
        /// In text form, this value is equal to 'r'.
        /// </para>
        /// </summary>
        RemoveFade
    }

    /// <summary>
    /// The exception that is thrown when a text formatting pattern is invalid. 
    /// </summary>
    public class InvalidFormatPatternExcpetion : Exception
    {
        /// <summary>
        /// Gets the section of the formatting patten that was invalid.
        /// </summary>
        public string InvalidSection { get; }

        /// <summary>
        /// Gets the text that was supposed to have the pattern applied to.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidFormatPatternExcpetion"/>.
        /// </summary>
        public InvalidFormatPatternExcpetion(string message, string affectedText, string invlidFragment) : base(message)
        {
            InvalidSection = invlidFragment;
            Text = affectedText;
        }
    }
}
