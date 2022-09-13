using System.Text.RegularExpressions;
using HazeClient.Helpers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows;
using System;

namespace HazeClient.Controls
{
    /// <summary>
    /// Represents a text box with rounded corners.
    /// </summary>
    public partial class RoundTextBox : UserControl
    {
        #region Delegates

        public delegate void TextFinishedEventHandler(object sender, TextFinishedEventArgs e);

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the text contents of the text box.
        /// </summary>
        /// <returns>
        /// A string containing the text contents of the text box. The default is an empty string ("").
        /// </returns>
        public string Text
        {
            get => GetValue(TextProperty) as string;
            set
            {
                //Set the value
                SetValue(TextProperty, value);
                textBox.Text = value;

                //Preview check
                if (string.IsNullOrEmpty(value) && !textBox.IsFocused) EnablePreview();
                else DisablePreview();
            }
        }

        /// <summary>
        /// Gets or sets the preview text of the text box. The preview text is present when the textbox is unfocused and empty
        /// </summary>
        /// <returns>
        /// A string containing the text contents of the text box. The default is "Enter text here".
        /// </returns>
        public string PreviewText
        {
            get => GetValue(PreviewTextProperty) as string;
            set
            {
                if (PreviewText != value) RaiseEvent(new TextChangedEventArgs(PreviewTextChangedEvent, UndoAction.None));
                SetValue(PreviewTextProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the font family of the control.
        /// </summary>
        /// <returns>
        /// A font family. The default is Segoe UI.
        /// </returns>
        public new FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        /// <summary>
        /// Gets or sets a brush that describes the border background of a control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the control's border; the default is <see cref="Brushes.Gray"/>.
        /// </returns>
        public new Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets a brush that describes the background of a control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the background of the control. The default is <see cref="Brushes.White"/>.
        /// </returns>
        public new Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        /// Gets or sets a brush that describes the foreground color.
        /// </summary>
        /// <returns>
        /// The brush that paints the foreground of the control. The default value is <see cref="Brushes.Black"/>.
        /// </returns>
        public new Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the brush that is used to paint the caret of the text box.
        /// </summary>
        /// <returns>
        /// The brush that is used to paint the caret of the text box.
        /// </returns>
        public Brush CaretBrush
        {
            get => (Brush)GetValue(CaretBrushProperty);
            set => SetValue(CaretBrushProperty, value);
        }

        /// <summary>
        /// Gets or sets the border thickness of a control.
        /// </summary>
        /// <returns>
        /// An int value; the default is 10.
        /// </returns>
        public new int BorderThickness
        {
            get => (int)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        /// <summary>
        /// Gets or sets the maximum font size.
        /// </summary>
        /// <returns>
        /// The maximum font size of this text box.
        /// </returns>
        public int MaxFontSize
        {
            get => (int)GetValue(MaxFontSizeProperty);
            set => SetValue(MaxFontSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the insertion position index of the caret.
        /// </summary>
        /// <returns>
        /// The zero-based insertion position index of the caret.
        /// </returns>
        public int CaretIndex
        {
            get => (int)GetValue(CaretIndexProperty);
            set => SetValue(CaretIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets if the text inside the text box is italic or not.
        /// </summary>
        /// <returns>
        /// If the text is italic or not.
        /// </returns>
        public bool Italic
        {
            get => (bool)GetValue(ItalicProperty);
            set
            {
                SetValue(ItalicProperty, value);
                textBox.FontStyle = value ? FontStyles.Italic : FontStyles.Normal;
            }
        }

        /// <summary>
        /// Gets or sets if the text inside the text box is bold or not.
        /// </summary>
        /// <returns>
        /// If the text is bold or not.
        /// </returns>
        public bool Bold
        {
            get => (bool)GetValue(BoldProperty);
            set
            {
                SetValue(BoldProperty, value);
                textBox.FontWeight = value ? FontWeights.Bold : FontWeights.DemiBold;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates how the text editing control responds when  the user presses the ENTER key.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if pressing the ENTER key inserts a new line at the current cursor position; otherwise, the ENTER key is ignored. The default value is <see cref="false"/>.
        /// </returns>
        public bool AcceptsReturn
        {
            get => accReturn;
            set
            {
                accReturn = value;
                textBox.AcceptsReturn = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates how the text editing control responds when  the user presses the TAB key.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if pressing the TAB key inserts a new line at the current cursor position; otherwise, the TAB key is ignored. The default value is <see cref="false"/>.
        /// </returns>
        public bool AcceptsTab
        {
            get => accTab;
            set
            {
                accTab = value;
                textBox.AcceptsTab = value;
            }
        }

        /// <summary>
        /// Gets or sets how characters are cased when they are manually entered into the text box.
        /// </summary>
        /// <returns>
        /// One of the <see cref="System.Windows.Controls.CharacterCasing"/> values that specifies how manually entered characters are cased. The default is <see cref="System.Windows.Controls.CharacterCasing"/>.Normal.
        /// </returns>
        public CharacterCasing CharacterCasing
        {
            get => (CharacterCasing)GetValue(CharacterCasingProperty);
            set
            {
                SetValue(CharacterCasingProperty, value);
                textBox.CharacterCasing = value;
            }
        }

        /// <summary>
        /// Gets the text decorations to apply to the text box.
        /// </summary>
        /// <returns>
        ///  A <see cref="TextDecorationCollection"/> collection that contains text decorations to apply to the text box. The default is <see langword="null"/>.
        /// </returns>
        public TextDecorationCollection TextDecorations
        {
            get => (TextDecorationCollection)GetValue(TextDecorationsProperty);
            set => SetValue(TextDecorationsProperty, value);
        }

        /// <summary>
        /// Gets or sets the masking character for the System.Windows.Controls.PasswordBox.
        /// </summary>
        /// <returns>
        ///  A masking character to echo when the user enters text into the <see cref="RoundTextBox"/>. The default is <see langword="null"/>.
        /// </returns>
        public char? PasswordChar
        {
            get => GetValue(PasswordCharProperty) as char?;
            set
            {
                SetValue(PasswordCharProperty, value);
                textBox.Text = Text;
            }
        }

        /// <summary>
        /// Gets a value that determines whether this element has logical focus. This is  a dependency property.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this element has logical focus; otherwise, <see langword="false"/>.
        /// </returns>
        public new bool IsFocused
        {
            get => (bool)textBox.IsFocused;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when content changes in the text element.
        /// </summary>
        public event TextChangedEventHandler TextChanged
        {
            add => AddHandler(TextChangedEvent, value);
            remove => RemoveHandler(TextChangedEvent, value);
        }

        /// <summary>
        /// Occurs when the text in the text element is confirmed.
        /// </summary>
        public event TextFinishedEventHandler TextFinished
        {
            add => AddHandler(TextFinishedEvent, value);
            remove => RemoveHandler(TextChangedEvent, value);
        }

        /// <summary>
        /// Occurs when the preview text changes.
        /// </summary>
        public event TextChangedEventHandler PreviewTextChanged
        {
            add => AddHandler(PreviewTextChangedEvent, value);
            remove => RemoveHandler(PreviewTextChangedEvent, value);
        }

        #endregion

        #region Fields

        public static readonly new DependencyProperty IsFocusedProperty = DependencyProperty.Register("IsFocused", typeof(bool), typeof(RoundTextBox));
        public static readonly new DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(RoundTextBox), new PropertyMetadata(new FontFamily("Segoe UI")));
        public static readonly new DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(int), typeof(RoundTextBox), new PropertyMetadata(10));
        public static readonly new DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(RoundTextBox), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(128, 128, 128))));
        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(RoundTextBox), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 255, 255))));
        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(RoundTextBox), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));
        public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register("PreviewText", typeof(string), typeof(RoundTextBox), new PropertyMetadata("Enter text here"));
        public static readonly DependencyProperty CaretBrushProperty = DependencyProperty.Register("CaretBrush", typeof(Brush), typeof(RoundTextBox), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(128, 128, 128))));
        public static readonly DependencyProperty MaxFontSizeProperty = DependencyProperty.Register("MaxFontSize", typeof(int), typeof(RoundTextBox), new PropertyMetadata(60));
        public static readonly DependencyProperty CaretIndexProperty = DependencyProperty.Register("CaretIndex", typeof(int), typeof(RoundTextBox), new PropertyMetadata(0));
        public static readonly DependencyProperty ItalicProperty = DependencyProperty.Register("Italic", typeof(bool), typeof(RoundTextBox), new PropertyMetadata(false));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(RoundTextBox), new PropertyMetadata(""));
        public static readonly DependencyProperty BoldProperty = DependencyProperty.Register("Bold", typeof(bool), typeof(RoundTextBox), new PropertyMetadata(false));
        public static readonly DependencyProperty PasswordCharProperty = DependencyProperty.Register("PasswordChar", typeof(char?), typeof(RoundTextBox), new PropertyMetadata(null));
        public static readonly DependencyProperty CharacterCasingProperty = DependencyProperty.Register("CharacterCasing", typeof(CharacterCasing), typeof(RoundTextBox), new PropertyMetadata(CharacterCasing.Normal));
        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register("TextDecorations", typeof(TextDecorationCollection), typeof(RoundTextBox), new PropertyMetadata(new TextDecorationCollection()));

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(TextChangedEventHandler), typeof(RoundTextBox));
        public static readonly RoutedEvent TextFinishedEvent = EventManager.RegisterRoutedEvent("TextFinished", RoutingStrategy.Bubble, typeof(TextFinishedEventHandler), typeof(RoundTextBox));
        public static readonly RoutedEvent PreviewTextChangedEvent = EventManager.RegisterRoutedEvent("PreviewTextChanged", RoutingStrategy.Bubble, typeof(TextChangedEventHandler), typeof(RoundTextBox));

        bool isPreviewActive = false,
             accReturn = false,
             accTab = false,
             keyUp = false;

        #endregion

        #region Static Constructor

        static RoundTextBox()
        {
            FocusableProperty.OverrideMetadata(typeof(RoundTextBox), new FrameworkPropertyMetadata(true));
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundTextBox"/> class.
        /// </summary>
        public RoundTextBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Attempts to set focus to this element.
        /// </summary>
        /// <returns><see langword="true"/> if keyboard focus and logical focus were set to this element; <see langword="false"/> if only logical focus was set to this element, or if the call to this method did not force the focus to change.</returns>
        public new bool Focus()
        {
            return textBox.Focus();
        }

        /// <summary>
        /// Is called when content in this editing control is confirmed.
        /// </summary>
        /// <param name="e">
        /// The arguments that are associated with the TextFinished event.
        /// </param>
        protected virtual void OnTextFinished(TextFinishedEventArgs e)
        {
            RaiseEvent(e);
        }

        /// <summary>
        /// Caled when the text box is initially loaded. Sets the preview text if necessary.
        /// </summary>
        private void OnTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            isPreviewActive = !string.IsNullOrEmpty(Text);
            if (!isPreviewActive)
            {
                SetForegroundOpacity(1);
                EnablePreview();
                return;
            }

            SetForegroundOpacity(0.5);
            DisablePreview();
        }

        /// <summary>
        /// Called when the text in the text box changes. Adds or removes any changes to the Text property and replaces the text inside the text box with password characters, if necessary.
        /// </summary>
        private void OnTextChange(object sender, TextChangedEventArgs e)
        {
            textBox.CaretIndex = textBox.Text.Length;
            if (isPreviewActive) return;

            if (textBox.Text != Text)
            {
                var change = e.Changes.GetEnumerator().GetFirst();
                Text = Text.Substring(0, Text.Length - change.RemovedLength) + textBox.Text.Remove(0, textBox.Text.Length - change.AddedLength);
            }
            if (PasswordChar.HasValue) textBox.Text = Regex.Replace(Text, ".", PasswordChar + "");

            e.RoutedEvent = TextChangedEvent;
            e.Source = this;
            RaiseEvent(e);
        }

        /// <summary>
        /// Called when the text box loses its focus. If the focus was lost intantionally by pressing Enter or Escape, this method will raise the TextFinishedEvent.
        /// </summary>
        private void OnFocusLost(object sender, RoutedEventArgs e)
        {
            if (textBox.Text == "") EnablePreview();

            //Set the keyUp field back to false before raising the event
            var kup = keyUp;
            keyUp = false;
            OnTextFinished(new TextFinishedEventArgs(Text, kup, TextFinishedEvent, this));
        }

        /// <summary>
        /// Called when the text box recieves focus.
        /// </summary>
        private void OnFocusRecieve(object sender, RoutedEventArgs e)
        {
            if (isPreviewActive) Text = "";
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Called when a key is raised when the text box is focused. If the key is Enter or Escape, unfocuses the text box.
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //Only remove the focus if the ESC or ENTER (without LSHIFT) keys were pressed
            if (!((e.Key == Key.Enter && !KeyboardHelper.IsKeyDown(Key.LeftShift)) || e.Key == Key.Escape)) return;

            keyUp = true;
            base.Focus();
            Keyboard.ClearFocus();
        }

        /// <summary>
        /// Enables the preview text, binding the text of the text box to the preview text.
        /// </summary>
        private void EnablePreview()
        {
            if (isPreviewActive) return;

            isPreviewActive = true;
            SetBinding(new Binding()
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor) { AncestorType = typeof(RoundTextBox) },
                Path = new PropertyPath("PreviewText")
            });

            SetForegroundOpacity(textBox.Foreground.Opacity / 2);
        }

        /// <summary>
        /// Disables the preview text, binding the text of the text box to the actual text.
        /// </summary>
        private void DisablePreview()
        {
            if (!isPreviewActive) return;
            isPreviewActive = false;

            SetBinding(null);
            textBox.Text = Text;

            SetForegroundOpacity(textBox.Foreground.Opacity * 2);
        }

        /// <summary>
        /// Sets any binding to the text box.
        /// </summary>
        private void SetBinding(BindingBase bind)
        {
            BindingOperations.ClearBinding(textBox, TextBox.TextProperty);
            if (!(bind is null)) textBox.SetBinding(TextBox.TextProperty, bind);
        }

        /// <summary>
        /// Sets the text box's foreground opacity. If the foreground is frozen, this method will replace it with an unfrozen copy.
        /// </summary>
        private void SetForegroundOpacity(double value)
        {
            if (textBox.Foreground.IsSealed || textBox.Foreground.IsFrozen) textBox.Foreground = textBox.Foreground.Clone();
            textBox.Foreground.Opacity = value;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {

        }
    }
}
