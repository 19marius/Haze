using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System;

namespace HazeClient.Controls
{
    /// <summary>
    /// Represents an exit button with rounded corners.
    /// </summary>
    public partial class ExitButton : RoundButton
    {
        #region Properties

        /// <summary>
        /// Gets or sets a brush that describes the background of this control when the mouse hovers over it.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the background of this control when the mouse hovers over it. The default is <see cref="Brushes.Red"/>.
        /// </returns>
        public new Brush HoverBackground
        {
            get => GetValue(HoverBackgroundProperty) as Brush;
            set
            {
                SetValue(HoverBackgroundProperty, value);
                base.UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the background of this control when the left button of the mouse is held down over it.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the background of this control when the left button of the mouse is held down over it. The default is a <see cref="SolidColorBrush"/> with the color #ff5c5c.
        /// </returns>
        public new Brush HoldBackground
        {
            get => GetValue(HoldBackgroundProperty) as Brush;
            set
            {
                SetValue(HoldBackgroundProperty, value);
                base.UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the fill of the X symbol.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the X symbol. The default is <see cref="Brushes"/>.Black.
        /// </returns>
        public Brush SymbolExitFill
        {
            get => GetValue(SymbolExitFillProperty) as Brush;
            set
            {
                SetValue(SymbolExitFillProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the fill of the X symbol when the mouse hovers over the control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill of the X symbol when the mouse hovers over the control. The default is <see cref="Brushes"/>.White.
        /// </returns>
        public Brush SymbolExitHoverFill
        {
            get => GetValue(SymbolExitHoverFillProperty) as Brush;
            set
            {
                SetValue(SymbolExitHoverFillProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the fill of the X symbol when the left button of the mouse is held down over the control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill of the X symbol when the left button of the mouse is held down over the control. The default is <see cref="Brushes"/>.White.
        /// </returns>
        public Brush SymbolExitHoldFill
        {
            get => GetValue(SymbolExitHoldFillProperty) as Brush;
            set
            {
                SetValue(SymbolExitHoldFillProperty, value);
                UpdateAnimationValues();
            }
        }

        #endregion

        #region Fields

        public static readonly DependencyProperty SymbolExitFillProperty = DependencyProperty.Register("SymbolExitFill", typeof(Brush), typeof(ExitButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));
        public static readonly DependencyProperty SymbolExitHoverFillProperty = DependencyProperty.Register("SymbolExitHoverFill", typeof(Brush), typeof(ExitButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 255, 255))));
        public static readonly DependencyProperty SymbolExitHoldFillProperty = DependencyProperty.Register("SymbolExitHoldFill", typeof(Brush), typeof(ExitButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 255, 255))));
        readonly BrushAnimation xEnterAnim, xLeaveAnim, xDownAnim, xUpAnim;

        #endregion

        static ExitButton()
        {
            HoverBackgroundProperty.OverrideMetadata(typeof(ExitButton), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 0, 0))));
            HoldBackgroundProperty.OverrideMetadata(typeof(ExitButton), new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 92, 92))));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitButton"/> class.
        /// </summary>
        public ExitButton()
        {
            InitializeComponent();
            xEnterAnim = new BrushAnimation(xbar.Fill, SymbolExitHoverFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn } };
            xLeaveAnim = new BrushAnimation(xbar.Fill, SymbolExitFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
            xDownAnim = new BrushAnimation(xbar.Fill, SymbolExitHoldFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn } };
            xUpAnim = new BrushAnimation(xbar.Fill, SymbolExitFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
        }

        protected override void OnMouseEnterAnimation()
        {
            xbar1.BeginAnimation(Shape.FillProperty, xEnterAnim);
            xbar.BeginAnimation(Shape.FillProperty, xEnterAnim);
        }

        protected override void OnMouseLeaveAnimation()
        {
            xbar1.BeginAnimation(Shape.FillProperty, xLeaveAnim);
            xbar.BeginAnimation(Shape.FillProperty, xLeaveAnim);
        }

        protected override void OnMouseLeftButtonDownAnimation()
        {
            xbar1.BeginAnimation(Shape.FillProperty, xDownAnim);
            xbar.BeginAnimation(Shape.FillProperty, xDownAnim);
        }

        protected override void OnMouseLeftButtonUpAnimation()
        {
            xUpAnim.To = IsMouseOver ? SymbolExitHoverFill : SymbolExitFill;
            xbar1.BeginAnimation(Shape.FillProperty, xUpAnim);
            xbar.BeginAnimation(Shape.FillProperty, xUpAnim);
        }

        new void UpdateAnimationValues()
        {
            xEnterAnim.From = xbar.Fill;
            xEnterAnim.To = SymbolExitHoverFill;

            xLeaveAnim.From = xbar.Fill;
            xLeaveAnim.To = SymbolExitFill;

            xDownAnim.From = xbar.Fill;
            xDownAnim.To = SymbolExitHoldFill;

            xUpAnim.From = xbar.Fill;
            xUpAnim.To = SymbolExitFill;
        }
    }
}
