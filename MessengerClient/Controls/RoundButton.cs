using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System;

namespace MessengerClient.Controls
{
    /// <summary>
    /// Represents a button with rounded corners.
    /// </summary>
    public class RoundButton : UserControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets a brush that describes the background of this control.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the background of the control. The default is a <see cref="SolidColorBrush"/> with the color #bababa.
        /// </returns>
        public new Brush Background
        {
            get => GetValue(BackgroundProperty) as Brush;
            set
            {
                SetValue(BackgroundProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the background of this control when the mouse hovers over it.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the background of this control when the mouse hovers over it. The default is a <see cref="SolidColorBrush"/> with the color #e6e6e6.
        /// </returns>
        public Brush HoverBackground
        {
            get => GetValue(HoverBackgroundProperty) as Brush;
            set
            {
                SetValue(HoverBackgroundProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets a brush that describes the background of this control when the left button of the mouse is held down over it.
        /// </summary>
        /// <returns>
        /// The brush that is used to fill the background of this control when the left button of the mouse is held down over it. The default is a <see cref="SolidColorBrush"/> with the color #f5f5f5.
        /// </returns>
        public Brush HoldBackground
        {
            get => GetValue(HoldBackgroundProperty) as Brush;
            set 
            {
                SetValue(HoldBackgroundProperty, value);
                UpdateAnimationValues();
            }
        }

        /// <summary>
        /// Gets or sets transform information that affects the rendering position of this element. This is a dependency property.
        /// </summary>
        /// <returns>
        /// Describes the specifics of the desired render transform. The default is <see cref="Transform.Identity"/>.
        /// </returns>
        public new Transform RenderTransform
        {
            get
            {
                var trans = GetValue(RenderTransformProperty) as Transform;
                return isTransformApplied ? (trans as TransformGroup).Children[0] : trans;
            }
            set
            {
                if (!isTransformApplied) SetValue(RenderTransformProperty, value);
                else (RenderTransform as TransformGroup).Children[0] = value;
            }
        }

        #endregion

        /// <summary>
        /// Occurs when this <see cref="RoundButton"/> is clicked.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #region Fields

        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(RoundButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(186, 186, 186))));
        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register("HoverBackground", typeof(Brush), typeof(RoundButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(230, 230, 230))));
        public static readonly DependencyProperty HoldBackgroundProperty = DependencyProperty.Register("HoldBackground", typeof(Brush), typeof(RoundButton), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(245, 245, 245))));
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RoundButton));
        protected readonly BrushAnimation bgEnterAnim, bgLeaveAnim, bgDownAnim, bgUpAnim;
        protected readonly DoubleAnimation wAnim, hAnim;
        protected Task delay = Task.Delay(150);
        protected ScaleTransform scale = new ScaleTransform();
        protected bool isDown = false,
                       canAnimate = true;

        Rectangle rect = new Rectangle();
        bool isTransformApplied = false;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundButton"/> class.
        /// </summary>
        public RoundButton()
        {
            bgEnterAnim = new BrushAnimation(rect.Fill, HoverBackground, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn } };
            bgLeaveAnim = new BrushAnimation(rect.Fill, Background, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
            bgDownAnim = new BrushAnimation(rect.Fill, HoldBackground, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseIn } };
            bgUpAnim = new BrushAnimation(rect.Fill, Background, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() { EasingMode = EasingMode.EaseOut } };
            wAnim = new DoubleAnimation(0, 0, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() };
            hAnim = new DoubleAnimation(0, 0, TimeSpan.FromMilliseconds(150), FillBehavior.HoldEnd) { EasingFunction = new SineEase() };
        }

        static RoundButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundButton), new FrameworkPropertyMetadata(typeof(RoundButton)));
            RenderTransformOriginProperty.OverrideMetadata(typeof(RoundButton), new FrameworkPropertyMetadata(new Point(0.5, 0.5)));
        }

        ~RoundButton()
        {
            rect.MouseEnter -= OnMouseEnterBtn;
            rect.MouseLeave -= OnMouseLeaveBtn;
            rect.MouseLeftButtonDown -= OnMouseLbDown;
            rect.MouseLeftButtonUp -= OnMouseLbUp;
        }

        public override void OnApplyTemplate()
        {
            //Set the rectangle
            rect = Template.FindName("btn", this) as Rectangle;

            //Set the transform
            RenderTransform = new TransformGroup() { Children = new TransformCollection(new Transform[] { RenderTransform, scale }) };
            isTransformApplied = true;

            //Assign events
            rect.MouseEnter += OnMouseEnterBtn;
            rect.MouseLeave += OnMouseLeaveBtn;
            rect.MouseLeftButtonDown += OnMouseLbDown;
            rect.MouseLeftButtonUp += OnMouseLbUp;

            base.OnApplyTemplate();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (rect.IsMouseOver) base.OnMouseEnter(e);
        }

        /// <summary>
        /// When overriden in a derived class, is invoked whenever a left click is registered on this control.
        /// </summary>
        protected virtual void OnClick()
        {
            var args = new RoutedEventArgs(ClickEvent, this);
            RaiseEvent(args);
        }

        /// <summary>
        /// When overriden in a derived class, this method should be used to add any other animations to the control whenever the mouse enters this control's area.
        /// </summary>
        protected virtual void OnMouseEnterAnimation()
        {

        }

        /// <summary>
        /// When overriden in a derived class, this method should be used to add any other animations to the control whenever the mouse leaves this control's area.
        /// </summary>
        protected virtual void OnMouseLeaveAnimation()
        {

        }

        /// <summary>
        /// When overriden in a derived class, this method should be used to add any other animations to the control whenever the left button is down over this control's area.
        /// </summary>
        protected virtual void OnMouseLeftButtonDownAnimation()
        {

        }

        /// <summary>
        /// When overriden in a derived class, this method should be used to add any other animations to the control whenever the left button is up over this control's area.
        /// </summary>
        protected virtual void OnMouseLeftButtonUpAnimation()
        {

        }

        /// <summary>
        /// Animates this control's scale.
        /// </summary>
        protected async void AnimateScale(Task delayer, bool doAwait, bool plus)
        {
            await CheckAnimation();

            wAnim.From = scale.ScaleX;
            hAnim.From = scale.ScaleY;
            wAnim.To = plus ? 1 : 0.85;
            hAnim.To = plus ? 1 : 0.85;

            (wAnim.EasingFunction as SineEase).EasingMode = plus ? EasingMode.EaseOut : EasingMode.EaseIn;
            (hAnim.EasingFunction as SineEase).EasingMode = plus ? EasingMode.EaseOut : EasingMode.EaseIn;

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, wAnim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, hAnim);

            if (doAwait)
            {
                await delayer;
                canAnimate = true;
            }
        }

        /// <summary>
        /// Checks if a scale change is in progress.
        /// </summary>
        /// <returns></returns>
        protected async Task CheckAnimation()
        {
            while (!canAnimate) await Task.Delay(1);
            canAnimate = false;
        }

        /// <summary>
        /// Updates all animations' To and From values.
        /// </summary>
        protected void UpdateAnimationValues()
        {
            bgEnterAnim.From = rect.Fill;
            bgEnterAnim.To = HoverBackground;

            bgLeaveAnim.From = rect.Fill;
            bgLeaveAnim.To = Background;

            bgDownAnim.From = rect.Fill;
            bgDownAnim.To = HoldBackground;

            bgUpAnim.From = rect.Fill;
            bgUpAnim.To = Background;
        }

        async void OnMouseEnterBtn(object sender, MouseEventArgs e)
        {
            await Task.WhenAny(delay);

            //Animation on background rect
            rect.BeginAnimation(Shape.FillProperty, bgEnterAnim);

            OnMouseEnterAnimation();

            await delay;
        }

        async void OnMouseLeaveBtn(object sender, MouseEventArgs e)
        {
            await Task.WhenAny(delay);

            //Animation on background rect
            rect.BeginAnimation(Shape.FillProperty, bgLeaveAnim);

            OnMouseLeaveAnimation();

            //Change scale if needed
            if (isDown)
            {
                AnimateScale(delay, true, true);
                isDown = false;
                return;
            }

            await delay;
        }

        async void OnMouseLbDown(object sender, MouseButtonEventArgs e)
        {
            await Task.WhenAny(delay);

            isDown = true;

            //Animation on background rect
            rect.BeginAnimation(Shape.FillProperty, bgDownAnim);

            OnMouseLeftButtonDownAnimation();

            AnimateScale(delay, true, false);
        }

        async void OnMouseLbUp(object sender, MouseButtonEventArgs e)
        {
            await Task.WhenAny(delay);

            //Correct to values if needed
            bgUpAnim.To = HoverBackground;

            //Animation on background rect
            rect.BeginAnimation(Shape.FillProperty, bgUpAnim);

            OnMouseLeftButtonUpAnimation();

            AnimateScale(null, false, true);

            await delay;
            if (isDown) OnClick();
            isDown = false;
            canAnimate = true;
        }
    }
}
