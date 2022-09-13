using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Windows.Media.Effects;
using Txt = HazeClient.Text;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Controls;
using HazeClient.Helpers;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Linq;
using System;

namespace HazeClient.Controls
{
    /// <summary>
    /// Represents a text block which supports the use of formatted text.
    /// </summary>
    public class FormattedTextBlock : TextBlock
    {
        #region Delegates

        /// <summary>
        /// Represents the method(s) that will be called when the text in a <see cref="FormattedTextBlock"/> changes.
        /// </summary>
        public delegate void TextUpdatedEventHandler(object sender, Txt.TextUpdatedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when a <see cref="Txt.LinkHandler"/> is added to a <see cref="FormattedTextBlock"/>.
        /// </summary>
        public delegate void LinkHandlerAddedEventHandler(object sender, Txt.LinkHandlerAddedEventArgs e);

        /// <summary>
        /// Represents the method(s) that will be called when all <see cref="Txt.LinkHandler"/> objects are cleared from a <see cref="FormattedTextBlock"/>.
        /// </summary>
        public delegate void LinkHandlersClearedEventHandler(object sender, Txt.LinkHandlersClearedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the string of formatted text that will be displayed on this <see cref="FormattedTextBlock"/>.
        /// </summary>
        /// <returns>
        /// The unformatted text that is displayed on this <see cref="FormattedTextBlock"/>. To get the formatted version, use the <see cref="GetFormattedText"/> method instead.
        /// </returns>
        public new string Text
        {
            get => GetValue(TextProperty) as string;
            set
            {
                text = Txt.FormattedText.Parse(value);
                SetValue(TextProperty, text.Text);
                AddToQueue(text, false);
            }
        }

        /// <summary>
        /// Gets the text that is curretly visible in this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public string CurrentText
        {
            get => executingText.Text;
        }

        /// <summary>
        /// Determines, when text fades in, if characters will appear one after another.
        /// </summary>
        /// <returns>
        /// The value indicating if characters will appear one after another. If <see langword="true"/>, the FadingDuration property will represent the interval between characters. This default is <see langword="true"/>.
        /// </returns>
        public bool SequentialFading { get; set; } = true;

        /// <summary>
        /// Gets or sets the time, in milliseconds, for text changes to fade in or out.
        /// </summary>
        /// <returns>
        /// The amount of time, in milliseconds, for text changes to fade in or out. If the SequentialFading property is <see langword="true"/>, represents the interval between appearing characters. The default is 200.
        /// </returns>
        public int FadingDuration { get; set; } = 50;

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> to the text contents of the <see cref="FormattedTextBlock"/>.
        /// <para>
        /// Text fragments whose color was changed by an effect will not be affected by the Foreground property.
        /// </para>
        /// </summary>
        /// <returns>
        /// The brush used to apply to the text contents. The default is <see cref="Brushes.AliceBlue"/>.
        /// </returns>
        public new Brush Foreground
        {
            get => GetValue(ForegroundProperty) as Brush;
            set
            {
                SetValue(ForegroundProperty, value);

                //Get a clone of the foreground in case the current value is frozen
                var foreground = Foreground.Clone();

                //Transform the inlines into an array for easier access
                var inlines = Inlines.ToArray();
                for (int group = 0; group < inlines.Length; group++)
                {
                    //If the group has a color modifier, don't override it
                    if (group % 2 == 1 && executingText.Effects[group / 2].EffectGroup.Where(x => x.Type.Equals(Txt.FormattedTextEffectType.Color)).Any()) continue;

                    for (int effect = 0; effect < inlines[group].TextEffects.Count; effect++)
                    {
                        //Save the initial opacity and reuse it after the foreground was changed.
                        var opacity = inlines[group].TextEffects[effect].Foreground.Opacity;
                        inlines[group].TextEffects[effect].Foreground = foreground;
                        inlines[group].TextEffects[effect].Foreground.Opacity = opacity;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="Txt.LinkHandler"/> objects which represent the handlers of the currently showing text.
        /// </summary>
        public ReadOnlyCollection<Txt.LinkHandler> Handlers { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a text update is complete. That is, when text finishes fading into the text block.
        /// </summary>
        public event TextUpdatedEventHandler TextUpdated
        {
            add => AddHandler(TextUpdatedEvent, value);
            remove => RemoveHandler(TextUpdatedEvent, value);
        }

        /// <summary>
        /// Occurs when a <see cref="Txt.LinkHandler"/> is added to this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public event LinkHandlerAddedEventHandler LinkHandlerAdded
        {
            add => AddHandler(LinkHandlerAddedEvent, value);
            remove => RemoveHandler(LinkHandlerAddedEvent, value);
        }

        /// <summary>
        /// Occurs when all <see cref="Txt.LinkHandler"/> objects are cleared from this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public event LinkHandlersClearedEventHandler LinkHandlersCleared
        {
            add => AddHandler(LinkHandlersClearedEvent, value);
            remove => RemoveHandler(LinkHandlersClearedEvent, value);
        }

        /// <summary>
        /// Occurs when this <see cref="FormattedTextBlock"/> is clicked.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #endregion

        #region Fields

        public static readonly new DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(FormattedTextBlock), new PropertyMetadata(""));
        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(FormattedTextBlock), new PropertyMetadata(Brushes.AliceBlue.Clone()));
        public static readonly RoutedEvent TextUpdatedEvent = EventManager.RegisterRoutedEvent("TextUpdated", RoutingStrategy.Bubble, typeof(TextUpdatedEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent LinkHandlerAddedEvent = EventManager.RegisterRoutedEvent("LinkHandlerAdded", RoutingStrategy.Bubble, typeof(LinkHandlerAddedEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent LinkHandlersClearedEvent = EventManager.RegisterRoutedEvent("LinkHandlersCleared", RoutingStrategy.Bubble, typeof(LinkHandlersClearedEventHandler), typeof(FormattedTextBlock));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FormattedTextBlock));

        Random random = new Random();

        List<Txt.LinkHandler> handlers = new List<Txt.LinkHandler>();

        DoubleAnimation anim = new DoubleAnimation()
        {
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };

        Dictionary<Txt.FormattedText, bool> textQueue = new Dictionary<Txt.FormattedText, bool>();
        Txt.FormattedText text, executingText;
        bool wasMouseDown = true;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattedTextBlock"/> class.
        /// </summary>
        public FormattedTextBlock()
        {
            Effect = new DropShadowEffect() { BlurRadius = 20, ShadowDepth = 10 };
            Handlers = handlers.AsReadOnly();
        }

        /// <summary>
        /// Gets the Text property with all original formatting patterns applied to it.
        /// <para>
        /// If <paramref name="current"/> is <see langword="true"/>, gets the CurrentText property instead.
        /// </para>
        /// </summary>
        public string GetFormattedText(bool current)
        {
            return current ? executingText.AsFormatted() : text.AsFormatted();
        }

        /// <summary>
        /// Appends a string of formatted text to this <see cref="FormattedTextBlock"/>.
        /// </summary>
        public void Append(string formattedText)
        {
            //Parse the new text
            var newText = Txt.FormattedText.Parse(formattedText);

            //Set the Text property from this method to not trigger the queue from the setter
            text += newText;
            SetValue(TextProperty, text.Text);

            //Add the new text to the queue
            AddToQueue(newText, true);
        }

        /// <summary>
        /// Clears the text queue of any pending updates, setting the Text property of this <see cref="FormattedTextBlock"/> to the last update.
        /// </summary>
        public void ClearQueue()
        {
            //Keep the current text's add value
            var execAdd = textQueue[executingText];

            //Clear the queue and re-add the text with its corresponding add value
            textQueue.Clear();
            textQueue.Add(executingText, execAdd);

            //Set the text property to match with the executing text
            text = executingText;
            SetValue(TextProperty, text.Text);
        }

        /// <summary>
        /// Waits for the text queue to finish. When this task finishes, all text updates will have been processed.
        /// </summary>
        public async Task AwaitQueue()
        {
            while (textQueue.Count >= 1) await Task.Delay(1);
        }

        /// <summary>
        /// Fades in a <see cref="FormattedText"/> object to this text block. Fades and clears out the previous text if <paramref name="add"/> is <see langword="false"/>.
        /// <para>
        /// Each letter appears after <paramref name="interval"/> milliseconds if <paramref name="allAtOnce"/> is <see langword="false"/>, otherwise, <paramref name="interval"/> servers as the total duration for the whole text to appear.
        /// </para>
        /// </summary>
        async Task FadeText(Txt.FormattedText text, int interval, bool allAtOnce = false, bool add = false)
        {
            bool instant = text.Modifiers.Contains(Txt.FormattedTextModifier.Instant),
                 noFade = text.Modifiers.Contains(Txt.FormattedTextModifier.RemoveFade);

            //Clear the text block through an animation, or instantly
            if (!add)
            {
                if (instant) ClearInstant();
                else await Clear(interval, !allAtOnce);
            }

            //Get all groups in the string, including the non affected ones
            var groups = GetGroups(text);

            //Unfreeze the foreground of the text block if necessary
            var foreground = Foreground.Clone();

            //Iterate through all groups and check if the text starts with an affected group
            for (int group = 0; group < groups.Length; group++)
            {
                //Create the run with the according text
                var run = new Run(groups[group])
                {
                    //Populate the run with effects for each character
                    TextEffects = new TextEffectCollection(Enumerable.Range(0, groups[group].Length).Select((x, i) => { var e = new TextEffect() { PositionCount = 1, PositionStart = GetCurrentStartingPosition() + i, Transform = new TransformGroup(), Foreground = foreground }; e.Foreground.Opacity = instant ? Opacity : 0; return e; }))
                };

                //Add the run to the text block
                Inlines.Add(run);

                //Iterate through the TextEffect array that will affect every character only if it's an affected group
                for (int charEffect = 0; charEffect < groups[group].Length; charEffect++)
                {
                    //Modify the effect's start position accordingly
                    var effect = run.TextEffects[charEffect];
                    var transform = effect.Transform as TransformGroup;

                    //Add the scale to the effect's transform group
                    var translation = new TranslateTransform(0, -4.5);
                    transform.Children.Add(translation);

                    //Dont await whitespaces
                    if (char.IsWhiteSpace(groups[group][charEffect])) continue;

                    //Iterate through the effect group to add animation to the effect according to the effect type (this can only loop up to 7 times)
                    for (int effectGroup = 0; group % 2 == 1 && effectGroup < text.Effects[group / 2].EffectGroup.Length; effectGroup++)
                    {
                        double value;
                        switch (text.Effects[group / 2].EffectGroup[effectGroup].Type)
                        {
                            #region Shake

                            case Txt.FormattedTextEffectType.Shake:

                                //Create a variable to store the sense of the shake and get the effect's value
                                int sense;
                                value = (double)text.Effects[group / 2].EffectGroup[effectGroup].Value;

                                //Add the translation to the transform group and set the animation's duration
                                transform.Children.Add(new TranslateTransform());
                                anim.BeginTime = allAtOnce || instant ? TimeSpan.FromMilliseconds(50 * charEffect) : TimeSpan.FromMilliseconds(0);
                                anim.Duration = TimeSpan.FromSeconds(value / 50);
                                anim.EasingFunction = null;


                                //Prepare and begin X animation
                                anim.From = ((sense = random.Next(0, 11)) % 2 == 0 ? -1 : 1) * (value / 10);
                                anim.To = (sense % 2 == 0 ? 1 : -1) * (value / 5);
                                transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.XProperty, anim);

                                //Prepare and begin Y animation
                                anim.From = ((sense = random.Next(0, 11)) % 2 == 0 ? -1 : 1) * (value / 10);
                                anim.To = (sense % 2 == 0 ? 1 : -1) * (value / 5);
                                transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);

                                break;

                            #endregion

                            #region Wave

                            case Txt.FormattedTextEffectType.Wave:

                                //Get the effect's value
                                value = (double)text.Effects[group / 2].EffectGroup[effectGroup].Value;

                                //Set up the animation and specify and add the delay
                                anim.Duration = TimeSpan.FromSeconds(0.5);
                                anim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                                anim.BeginTime = allAtOnce || instant ? TimeSpan.FromMilliseconds(50 * charEffect) : TimeSpan.FromMilliseconds(0);
                                anim.From = -value;
                                anim.To = value;

                                //Create a clock for the animation to correct the starting value of the translation
                                var clock = anim.CreateClock();
                                clock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0), TimeSeekOrigin.BeginTime);

                                //Add the translation using the correct Y value and begin the animation
                                transform.Children.Add(new TranslateTransform(0, (double)anim.GetCurrentValue(-value, value, clock)));
                                transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                                break;

                            #endregion

                            #region Color

                            case Txt.FormattedTextEffectType.Color:

                                //Check if the value is a single color first
                                if (text.Effects[group / 2].EffectGroup[effectGroup].Value.GetType().Equals(typeof(Color)))
                                {
                                    effect.Foreground = new SolidColorBrush((Color)text.Effects[group / 2].EffectGroup[effectGroup].Value);
                                    break;
                                }

                                //Set up the animation
                                var colorAnim = text.Effects[group / 2].EffectGroup[effectGroup].Value as ColorAnimation;
                                colorAnim.BeginTime = allAtOnce || instant ? TimeSpan.FromMilliseconds(50 * charEffect) : TimeSpan.FromMilliseconds(0);
                                colorAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                                colorAnim.Duration = TimeSpan.FromSeconds(0.5);
                                colorAnim.RepeatBehavior = RepeatBehavior.Forever;
                                colorAnim.AutoReverse = true;

                                //Create a clock for the animation to correct the starting value of the translation
                                var colorClock = colorAnim.CreateClock();
                                colorClock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50 * charEffect), TimeSeekOrigin.BeginTime);

                                //Create the brush, assign it and begin the animation
                                effect.Foreground = new SolidColorBrush((Color)colorAnim.GetCurrentValue(colorAnim.From, colorAnim.To, colorClock));
                                effect.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                                break;

                            #endregion

                            #region FontSize

                            case Txt.FormattedTextEffectType.FontSize:

                                //Get the effect's value and set the font size
                                value = (double)text.Effects[group / 2].EffectGroup[effectGroup].Value;
                                run.FontSize = FontSize * value;

                                break;

                            #endregion

                            #region Bold

                            case Txt.FormattedTextEffectType.Bold:

                                run.FontWeight = FontWeights.Bold;
                                break;

                            #endregion

                            #region Italic

                            case Txt.FormattedTextEffectType.Italic:

                                run.FontStyle = FontStyles.Italic;
                                break;

                            #endregion

                            #region Link

                            case Txt.FormattedTextEffectType.Link:

                                //Get the link handler and add it to the instance's list
                                var handler = text.Effects[group / 2].EffectGroup[effectGroup].Value as Txt.LinkHandler;
                                handlers.Add(handler);

                                //Only add event handlers to the run on the first character
                                if (charEffect != 0) break;

                                //Add tags to the run
                                run.Cursor = Cursors.Hand;
                                run.Tag = new List<object>() { false, handler, text.Effects[group / 2], new List<Transform>() };
                                run.MouseLeftButtonDown += OnRunMouseDown;
                                run.MouseLeftButtonUp += OnRunMouseUp;
                                run.MouseLeave += OnRunMouseLeave;

                                //Raise the link added event
                                RaiseEvent(new Txt.LinkHandlerAddedEventArgs(handler, handlers.Count - 1, LinkHandlerAddedEvent, this));

                                break;

                                #endregion
                        }
                    }

                    //Fade in the character the effect is applied on, depending on the conditions
                    effect.Foreground.Opacity = noFade ? Opacity : effect.Foreground.Opacity;
                    if (!instant && !noFade)
                    {
                        effect.Foreground.AnimateBrushOpacity(0, Opacity, interval * 5, typeof(SineEase), EasingMode.EaseOut);
                        translation.AnimatePositionY(-4.5, 0, interval * 5, typeof(SineEase), EasingMode.EaseOut);
                    }

                    if (!allAtOnce && !instant) await Task.Delay(interval);
                }
            }

            if (allAtOnce && !instant) await Task.Delay(interval);
            RaiseEvent(new Txt.TextUpdatedEventArgs(executingText, add, TextUpdatedEvent, this));
        }

        /// <summary>
        /// Clears this text block's inlines by fading them out and back in, also stopping all the inlines' effects' animations.
        /// </summary>
        async Task Clear(int duration, bool sequential)
        {
            //Fade the text block out to clear runs
            var opacity = Opacity;
            if (Inlines.Count > 0) await this.AnimateOpacityAsync(Opacity, 0, sequential ? 150 : duration, typeof(SineEase), EasingMode.EaseOut);
            
            //Go through all the runs to stop animations on all effects
            foreach (var inline in Inlines)
            {
                for (int effectIndex = 0; effectIndex < (inline as Run).TextEffects.Count; effectIndex++)
                {
                    var effect = (inline as Run).TextEffects[effectIndex];
                    var transform = effect.Transform as TransformGroup;

                    //Stop the color animation
                    effect.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, null);

                    //Remove the event handlers
                    inline.MouseLeave -= OnRunMouseLeave;
                    inline.MouseLeftButtonDown -= OnRunMouseDown;
                    inline.MouseLeftButtonUp -= OnRunMouseUp;

                    //Stop the translate animations
                    for (int transformIndex = 0; transformIndex < transform.Children.Count; transformIndex++)
                    {
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
                    }
                }
            }

            //Clear the LinkHandlers and raise the event
            RaiseEvent(new Txt.LinkHandlersClearedEventArgs(handlers.ToArray(), LinkHandlersClearedEvent, this));
            handlers.Clear();

            //Clear the runs and restore the opacity
            Inlines.Clear();
            Opacity = opacity;
        }

        /// <summary>
        /// Immediately clears this text block's inlines, also stopping all the inlines' effects' animations.
        /// </summary>
        private void ClearInstant()
        {
            //Fade the text block out to clear runs
            var opacity = Opacity;
            Opacity = 0;

            //Go through all the runs to stop animations on all effects
            foreach (var inline in Inlines)
            {
                for (int effectIndex = 0; effectIndex < (inline as Run).TextEffects.Count; effectIndex++)
                {
                    var effect = (inline as Run).TextEffects[effectIndex];
                    var transform = effect.Transform as TransformGroup;

                    //Stop the color animation
                    effect.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, null);

                    //Remove the event handlers
                    inline.MouseLeave -= OnRunMouseLeave;
                    inline.MouseLeftButtonDown -= OnRunMouseDown;
                    inline.MouseLeftButtonUp -= OnRunMouseUp;

                    //Stop the translate animations
                    for (int transformIndex = 0; transformIndex < transform.Children.Count; transformIndex++)
                    {
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                        transform.Children[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
                    }
                }
            }

            //Clear the LinkHandlers and raise the event
            RaiseEvent(new Txt.LinkHandlersClearedEventArgs(handlers.ToArray(), LinkHandlersClearedEvent, this));
            handlers.Clear();

            //Clear the runs and restore the opacity
            Inlines.Clear();
            Opacity = opacity;
        }

        /// <summary>
        /// Gets all groups of text in a <see cref="FormattedText"/> object, including groups that have no effects.
        /// </summary>
        string[] GetGroups(Txt.FormattedText text)
        {
            return text.Effects.Take(text.Effects.Count - 1).Select((x, i) => new string[] { x.Text, text.Text.Substring(x.Index + x.Text.Length, text.Effects[i + 1].Index - (x.Index + x.Text.Length)) })

                                                                  //Flatten the groups
                                                                  .SelectMany(x => x)

                                                                  //Add the last effect and the remaining unaffected group
                                                                  .Concat(text.Effects.Count > 0 ? new string[] { text.Effects[text.Effects.Count - 1].Text, text.Text.Substring(text.Effects[text.Effects.Count - 1].Index + text.Effects[text.Effects.Count - 1].Text.Length, text.Text.Length - (text.Effects[text.Effects.Count - 1].Index + text.Effects[text.Effects.Count - 1].Text.Length)) } : new string[0])

                                                                  //Add the first unaffected group if it exists
                                                                  .Prepend(text.Effects.Count > 0 ? text.Text.Substring(0, text.Effects[0].Index) : text.Text)

                                                                  //Transform to array
                                                                  .ToArray();
        }

        /// <summary>
        /// Gets the starting positon for the first character a <see cref="Run"/> in the context of a <see cref="TextEffect"/>.
        /// </summary>
        int GetCurrentStartingPosition()
        {
            return Inlines.Select(x => x as Run).Aggregate(1, (total, next) => total + MathHelper.Clamp(next.TextEffects.Count - 1, 0) + 3 - Convert.ToInt32(next.Text.Length == 0));
        }

        /// <summary>
        /// Adds a task to the text queue. The task will be executed only if the queue is empty.
        /// </summary>
        void AddToQueue(Txt.FormattedText text, bool add)
        {
            textQueue.Add(text, add);

            //Only execute if the queue was empty
            if (textQueue.Count == 1) RunQueue(text, add);
        }

        /// <summary>
        /// Runs the text queue until there are no tasks left to complete.
        /// </summary>
        async void RunQueue(Txt.FormattedText text, bool add)
        {
            executingText = text;
            await FadeText(text, FadingDuration, !SequentialFading, add);

            //Remove from the queue and run again until the queue is empty
            if (textQueue.ContainsKey(text)) textQueue.Remove(text);
            if (textQueue.Count >= 1) RunQueue(textQueue.First().Key, textQueue.First().Value);
        }

        /// <summary>
        /// Called when the left mouse button is depressed onto a <see cref="Run"/>. Applies a link effect's click animation to the <see cref="Run"/>.
        /// </summary>
        void OnRunMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Get the run and its tags
            var run = sender as Run;
            var tags = run.Tag as List<object>;

            //Tag 0 means the left mouse button was pressed over the run
            tags[0] = true;

            //Tag 3 is a list of all link transforms on the run
            var transforms = tags[3] as List<Transform>;

            //Apply the handler's effect
            double value;
            var handler = tags[1] as Txt.LinkHandler;

            //Go through all characters to apply the effect
            for (int charEffect = 0; !handler.ClickEffect.Equals(default(Txt.FormattedTextEffect)) && charEffect < run.Text.Length; charEffect++)
            {
                //Get the transform group of the effect
                var transform = run.TextEffects[charEffect].Transform as TransformGroup;

                //Stop the color animation
                run.TextEffects[charEffect].Foreground.BeginAnimation(SolidColorBrush.ColorProperty, null);

                //Stop the translate animations
                for (int transformIndex = 0; transformIndex < transform.Children.Count; transformIndex++)
                {
                    transform.Children[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                    transform.Children[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
                }

                //Begin new animations
                switch (handler.ClickEffect.Type)
                {
                    #region Shake

                    case Txt.FormattedTextEffectType.Shake:

                        //Create a variable to store the sense of the shake and get the effect's value
                        int sense;
                        value = (double)handler.ClickEffect.Value;

                        //Add the translation to the transform group and set the animation's duration
                        transform.Children.Add(new TranslateTransform());
                        transforms.Add(transform.Children[transform.Children.Count - 1]);

                        anim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                        anim.Duration = TimeSpan.FromSeconds(value / 50);
                        anim.EasingFunction = null;

                        //Prepare and begin X animation
                        anim.From = ((sense = random.Next(0, 11)) % 2 == 0 ? -1 : 1) * (value / 10);
                        anim.To = (sense % 2 == 0 ? 1 : -1) * (value / 5);
                        transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.XProperty, anim);

                        //Prepare and begin Y animation
                        anim.From = ((sense = random.Next(0, 11)) % 2 == 0 ? -1 : 1) * (value / 10);
                        anim.To = (sense % 2 == 0 ? 1 : -1) * (value / 5);
                        transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                        break;

                    #endregion

                    #region Wave

                    case Txt.FormattedTextEffectType.Wave:

                        //Get the effect's value
                        value = (double)handler.ClickEffect.Value;

                        //Set up the animation and specify and add the delay
                        anim.Duration = TimeSpan.FromSeconds(0.5);
                        anim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                        anim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                        anim.From = -value;
                        anim.To = value;

                        //Create a clock for the animation to correct the starting value of the translation
                        var clock = anim.CreateClock();
                        clock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0), TimeSeekOrigin.BeginTime);

                        //Add the translation using the correct Y value and begin the animation
                        transform.Children.Add(new TranslateTransform(0, (double)anim.GetCurrentValue(-value, value, clock)));
                        transforms.Add(transform.Children[transform.Children.Count - 1]);

                        transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                        break;

                    #endregion

                    #region Color

                    case Txt.FormattedTextEffectType.Color:

                        //Check if the value is a single color first
                        if (handler.ClickEffect.Value.GetType().Equals(typeof(Color)))
                        {
                            run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)handler.ClickEffect.Value);
                            break;
                        }

                        //Set up the animation
                        var colorAnim = handler.ClickEffect.Value as ColorAnimation;
                        colorAnim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                        colorAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                        colorAnim.Duration = TimeSpan.FromSeconds(0.5);
                        colorAnim.RepeatBehavior = RepeatBehavior.Forever;
                        colorAnim.AutoReverse = true;

                        //Create a clock for the animation to correct the starting value of the translation
                        var colorClock = colorAnim.CreateClock();
                        colorClock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50 * charEffect), TimeSeekOrigin.BeginTime);

                        //Create the brush, assign it and begin the animation
                        run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)colorAnim.GetCurrentValue(colorAnim.From, colorAnim.To, colorClock));
                        run.TextEffects[charEffect].Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                        break;

                    #endregion

                    #region FontSize

                    case Txt.FormattedTextEffectType.FontSize:

                        //Get the effect's value and set the font size
                        value = (double)handler.ClickEffect.Value;
                        run.FontSize = FontSize * value;

                        break;

                    #endregion

                    #region Bold

                    case Txt.FormattedTextEffectType.Bold:

                        run.FontWeight = FontWeights.Bold;
                        break;

                    #endregion

                    #region Italic

                    case Txt.FormattedTextEffectType.Italic:

                        run.FontStyle = FontStyles.Italic;
                        break;

                        #endregion
                }
            }
        }

        /// <summary>
        /// Called when the left mouse button is raised from a <see cref="Run"/>.
        /// </summary>
        void OnRunMouseUp(object sender, MouseButtonEventArgs e)
        {
            OnRunMouseUpOrLeft(sender);
        }

        /// <summary>
        /// Called when the mouse leaves a <see cref="Run"/>.
        /// </summary>
        void OnRunMouseLeave(object sender, MouseEventArgs e)
        {
            OnRunMouseUpOrLeft(sender);
        }

        /// <summary>
        /// Called either when the mouse leaves or is raised from a <see cref="Run"/> only if the left mouse button was initially depressed onto the <see cref="Run"/>. Resets all the <see cref="Run"/>'s effects to their initial state.
        /// </summary>
        void OnRunMouseUpOrLeft(object sender)
        {
            //Get the run and its tags
            var run = sender as Run;
            var tags = run.Tag as List<object>;

            //If the mouse wasn't depressed on the run, this event shouldn't occur
            if ((bool)tags[0] == false) return;
            tags[0] = false;

            //Stop the animations on the link's transforms
            var transforms = tags[3] as List<Transform>;
            for (int transformIndex = 0; transformIndex < transforms.Count; transformIndex++)
            {
                transforms[transformIndex].BeginAnimation(TranslateTransform.XProperty, null);
                transforms[transformIndex].BeginAnimation(TranslateTransform.YProperty, null);
            }

            //Reset the run's bold and italic states
            run.FontWeight = FontWeight;
            run.FontStyle = FontStyle;

            //Get the effect group and readd the animations to each effect
            var group = (Txt.FormattedTextEffectGroup)tags[2];
            for (int charEffect = 0; !(tags[1] as Txt.LinkHandler).ClickEffect.Equals(default(Txt.FormattedTextEffect)) && charEffect < run.Text.Length; charEffect++)
            {
                //Stop the foreground animation, if needed
                run.TextEffects[charEffect].Foreground?.BeginAnimation(SolidColorBrush.ColorProperty, null);

                //Remove the effect's link transforms
                var transform = run.TextEffects[charEffect].Transform as TransformGroup;
                transform.Children = new TransformCollection(transform.Children.Except(transforms));

                //Iterate through the effect group to readd animation to the effect according to the effect type (this can only loop up to 6 times)
                for (int effectGroup = 0; effectGroup < group.EffectGroup.Length; effectGroup++)
                {
                    double value;
                    switch (group.EffectGroup[effectGroup].Type)
                    {
                        #region Shake

                        case Txt.FormattedTextEffectType.Shake:

                            //Create a variable to store the sense of the shake and get the effect's value
                            int sense;
                            value = (double)group.EffectGroup[effectGroup].Value;

                            //Add the translation to the transform group and set the animation's duration
                            transform.Children.Add(new TranslateTransform());
                            anim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                            anim.Duration = TimeSpan.FromSeconds(value / 50);
                            anim.EasingFunction = null;


                            //Prepare and begin X animation
                            anim.From = ((sense = random.Next(0, 11)) % 2 == 0 ? -1 : 1) * (value / 10);
                            anim.To = (sense % 2 == 0 ? 1 : -1) * (value / 5);
                            transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.XProperty, anim);

                            //Prepare and begin Y animation
                            anim.From = ((sense = random.Next(0, 11)) % 2 == 0 ? -1 : 1) * (value / 10);
                            anim.To = (sense % 2 == 0 ? 1 : -1) * (value / 5);
                            transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);

                            break;

                        #endregion

                        #region Wave

                        case Txt.FormattedTextEffectType.Wave:

                            //Get the effect's value
                            value = (double)group.EffectGroup[effectGroup].Value;

                            //Set up the animation and specify and add the delay
                            anim.Duration = TimeSpan.FromSeconds(0.5);
                            anim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                            anim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                            anim.From = -value;
                            anim.To = value;

                            //Create a clock for the animation to correct the starting value of the translation
                            var clock = anim.CreateClock();
                            clock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(0), TimeSeekOrigin.BeginTime);

                            //Add the translation using the correct Y value and begin the animation
                            transform.Children.Add(new TranslateTransform(0, (double)anim.GetCurrentValue(-value, value, clock)));
                            transform.Children[transform.Children.Count - 1].BeginAnimation(TranslateTransform.YProperty, anim);
                            break;

                        #endregion

                        #region Color

                        case Txt.FormattedTextEffectType.Color:

                            //Check if the value is a single color first
                            if (group.EffectGroup[effectGroup].Value.GetType().Equals(typeof(Color)))
                            {
                                run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)group.EffectGroup[effectGroup].Value);
                                break;
                            }

                            //Set up the animation
                            var colorAnim = group.EffectGroup[effectGroup].Value as ColorAnimation;
                            colorAnim.BeginTime = TimeSpan.FromMilliseconds(50 * charEffect);
                            colorAnim.EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut };
                            colorAnim.Duration = TimeSpan.FromSeconds(0.5);
                            colorAnim.RepeatBehavior = RepeatBehavior.Forever;
                            colorAnim.AutoReverse = true;

                            //Create a clock for the animation to correct the starting value of the translation
                            var colorClock = colorAnim.CreateClock();
                            colorClock.Controller.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(50 * charEffect), TimeSeekOrigin.BeginTime);

                            //Create the brush, assign it and begin the animation
                            run.TextEffects[charEffect].Foreground = new SolidColorBrush((Color)colorAnim.GetCurrentValue(colorAnim.From, colorAnim.To, colorClock));
                            run.TextEffects[charEffect].Foreground.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                            break;

                        #endregion

                        #region FontSize

                        case Txt.FormattedTextEffectType.FontSize:

                            //Get the effect's value and set the font size
                            value = (double)group.EffectGroup[effectGroup].Value;
                            run.FontSize = FontSize * value;

                            break;

                        #endregion

                        #region Bold

                        case Txt.FormattedTextEffectType.Bold:

                            run.FontWeight = FontWeights.Bold;
                            break;

                        #endregion

                        #region Italic

                        case Txt.FormattedTextEffectType.Italic:

                            run.FontStyle = FontStyles.Italic;
                            break;

                            #endregion
                    }
                }
            }

            var handler = tags[1] as Txt.LinkHandler;
            if (!(handler?.Handler is null)) handler.Handler();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            wasMouseDown = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (!wasMouseDown) return;
            wasMouseDown = false;
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Text = Text;
        }
    }
}


#region RUN TAG INFO
#if false

- all Run objects of a FormattedTextBlock object (which have the link effect) have the same tag, a List<object>

tag[0]: a bool which confirms if the left mouse button was pressed on the Run (used for MouseLeftButtonUp and MouseLeave events)

tag[1]: a LinkHandler (used for ease of access to the click effect)

tag[2]: the FormattedTextEffectGroup which is applid to all the Run's characters (used to keep track of the original effects)

tag[3]: a List<Transform> which holds all transforms the link effect applied over the run (used to remove them from the run later)

#endif
#endregion
