using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System;

namespace HazeClient
{
    /// <summary>
    /// Animates the value of a <see cref="Brush"/> property between two target values using linear interpolation over a specified <see cref="Timeline"/>.Duration.
    /// </summary>
    public class BrushAnimation : AnimationTimeline
    {
        /// <summary>
        /// Gets or sets the animation's starting value.
        /// </summary>
        /// <returns>
        /// The starting value of the animation. The default value is <see langword="null"/>.
        /// </returns>
        public Brush From
        {
            get => GetValue(FromProperty) as Brush;
            set => SetValue(FromProperty, value);
        }

        /// <summary>
        /// Gets or sets the animation's ending value.
        /// </summary>
        /// <returns>
        /// The ending value of the animation. The default value is <see langword="null"/>.
        /// </returns>
        public Brush To
        {
            get => GetValue(ToProperty) as Brush;
            set => SetValue(ToProperty, value);
        }

        /// <summary>
        /// Gets or sets the easing function applied to this animation.
        /// </summary>
        /// <returns>
        /// The easing function applied to this animation.
        /// </returns>
        public IEasingFunction EasingFunction
        {
            get => GetValue(EasingFunctionProperty) as IEasingFunction;
            set => SetValue(EasingFunctionProperty, value);
        }

        public override Type TargetPropertyType
        {
            get => typeof(Brush);
        }

        #region Fields

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(Brush), typeof(BrushAnimation));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(Brush), typeof(BrushAnimation));
        public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(BrushAnimation));

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushAnimation"/> class.
        /// </summary>
        public BrushAnimation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushAnimation"/>
        /// class that animates to the specified value over the specified duration. The starting
        /// value for the animation is the base value of the property being animated or the
        /// output from another animation.
        /// </summary>
        /// <param name="toValue">The destination value of the animation.</param>
        /// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="Timeline"/>.Duration property for more information.</param>
        public BrushAnimation(Brush toValue, Duration duration)
        {
            To = toValue;
            Duration = duration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushAnimation"/>
        /// class that animates from the specified starting value to the specified destination
        /// value over the specified duration.
        /// </summary>
        /// <param name="fromValue">The starting value of the animation.</param>
        /// <param name="toValue">The destination value of the animation.</param>
        /// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="Timeline"/>.Duration property for more information.</param>
        public BrushAnimation(Brush fromValue, Brush toValue, Duration duration)
        {
            From = fromValue;
            To = toValue;
            Duration = duration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushAnimation"/>
        /// class that animates to the specified value over the specified duration and has
        /// the specified fill behavior. The starting value for the animation is the base
        /// value of the property being animated or the output from another animation.
        /// </summary>
        /// <param name="toValue">The destination value of the animation.</param>
        /// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="Timeline"/>.Duration property for more information.</param>
        /// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
        public BrushAnimation(Brush toValue, Duration duration, FillBehavior fillBehavior)
        {
            To = toValue;
            Duration = duration;
            FillBehavior = fillBehavior;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BrushAnimation"/>
        /// class that animates from the specified starting value to the specified destination
        /// value over the specified duration and has the specified fill behavior.
        /// </summary>
        /// <param name="fromValue">The starting value of the animation.</param>
        /// <param name="toValue">The destination value of the animation.</param>
        /// <param name="duration">The length of time the animation takes to play from start to finish, once. See the <see cref="Timeline"/>.Duration property for more information.</param>
        /// <param name="fillBehavior">Specifies how the animation behaves when it is not active.</param>
        public BrushAnimation(Brush fromValue, Brush toValue, Duration duration, FillBehavior fillBehavior)
        {
            From = fromValue;
            To = toValue;
            FillBehavior = fillBehavior;
            Duration = duration;
        }

        #endregion

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            return GetCurrentValue(defaultOriginValue as Brush,
                                   defaultDestinationValue as Brush,
                                   animationClock);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BrushAnimation();
        }

        object GetCurrentValue(Brush defaultOriginValue, Brush defaultDestinationValue, AnimationClock animationClock)
        {
            if (!animationClock.CurrentProgress.HasValue) return Brushes.Transparent;

            defaultOriginValue = From ?? defaultOriginValue;
            defaultDestinationValue = To ?? defaultDestinationValue;

            return animationClock.CurrentProgress.Value == 0 ? defaultOriginValue : 
                   animationClock.CurrentProgress.Value == 1 ? defaultDestinationValue :
                   new VisualBrush(new Border() { Width = 1, Height = 1, Background = defaultOriginValue, Child = new Border() { Background = defaultDestinationValue, Opacity = animationClock.CurrentProgress.Value } });
        }
    }
}
