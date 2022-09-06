using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System;

namespace MessengerClient.Controls
{
    public partial class MinimizeButton : RoundButton
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
        /// Gets or sets a brush that describes the fill of the line symbol.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the X symbol. The default is <see cref="Brushes"/>.Black.
        /// </returns>
        public Brush SymbolMinimizeFill
        {
            get => GetValue(SymbolMinimizeFillProperty) as Brush;
            set
            {
                SetValue(SymbolMinimizeFillProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the fill of the line symbol when the mouse hovers over the control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill of the line symbol when the mouse hovers over the control. The default is a <see cref="SolidColorBrush"/> with the color #282828.
        /// </returns>
        public Brush SymbolMinimizeHoverFill
        {
            get => GetValue(SymbolMinimizeHoverFillProperty) as Brush;
            set
            {
                SetValue(SymbolMinimizeHoverFillProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the fill of the X symbol when the left button of the mouse is held down over the control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill of the X symbol when the left button of the mouse is held down over the control. The default is a <see cref="SolidColorBrush"/> with the color #282828.
        /// </returns>
        public Brush SymbolMinimizeHoldFill
        {
            get => GetValue(SymbolMinimizeHoldFillProperty) as Brush;
            set
            {
                SetValue(SymbolMinimizeHoldFillProperty, value);
                UpdateAnimationValues();
            }
        }

        #endregion

        #region Fields

        public static readonly DependencyProperty SymbolMinimizeFillProperty = DependencyProperty.Register("SymbolMinimizeFill", typeof(Brush), typeof(ExitButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0, 0, 0))));
        public static readonly DependencyProperty SymbolMinimizeHoverFillProperty = DependencyProperty.Register("SymbolMinimizeHoverFill", typeof(Brush), typeof(ExitButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(40, 40, 40))));
        public static readonly DependencyProperty SymbolMinimizeHoldFillProperty = DependencyProperty.Register("SymbolMinimizeHoldFill", typeof(Brush), typeof(ExitButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(40, 40, 40))));
        readonly BrushAnimation mEnterAnim, mLeaveAnim, mDownAnim, mUpAnim;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ExitButton"/> class.
        /// </summary>
        public MinimizeButton()
        {
            InitializeComponent();
            mEnterAnim = new BrushAnimation(mbar.Fill, SymbolMinimizeHoverFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn } };
            mLeaveAnim = new BrushAnimation(mbar.Fill, SymbolMinimizeFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
            mDownAnim = new BrushAnimation(mbar.Fill, SymbolMinimizeHoldFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn } };
            mUpAnim = new BrushAnimation(mbar.Fill, SymbolMinimizeFill, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
        }

        protected override void OnMouseEnterAnimation()
        {
            mbar.BeginAnimation(Shape.FillProperty, mEnterAnim);
        }

        protected override void OnMouseLeaveAnimation()
        {
            mbar.BeginAnimation(Shape.FillProperty, mLeaveAnim);
        }

        protected override void OnMouseLeftButtonDownAnimation()
        {
            mbar.BeginAnimation(Shape.FillProperty, mDownAnim);
        }

        protected override void OnMouseLeftButtonUpAnimation()
        {
            mUpAnim.To = IsMouseOver ? SymbolMinimizeHoverFill : SymbolMinimizeFill;
            mbar.BeginAnimation(Shape.FillProperty, mUpAnim);
        }

        new void UpdateAnimationValues()
        {
            mEnterAnim.From = mbar.Fill;
            mEnterAnim.To = SymbolMinimizeHoverFill;

            mLeaveAnim.From = mbar.Fill;
            mLeaveAnim.To = SymbolMinimizeFill;

            mDownAnim.From = mbar.Fill;
            mDownAnim.To = SymbolMinimizeHoldFill;

            mUpAnim.From = mbar.Fill;
            mUpAnim.To = SymbolMinimizeFill;
        }
    }
}
