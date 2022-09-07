using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Data;
using System.Linq;
using System;

namespace MessengerClient.Helpers
{
    /// <summary>
    /// Provides static methods which simplify animating objects.
    /// </summary>
    public static class AnimationHelper
    {
        static DoubleAnimation opacityAnim = new DoubleAnimation() { FillBehavior = FillBehavior.HoldEnd },
                               wAnim = new DoubleAnimation() { FillBehavior = FillBehavior.HoldEnd },
                               hAnim = new DoubleAnimation() { FillBehavior = FillBehavior.HoldEnd };

        static ThicknessAnimation marginAnim = new ThicknessAnimation() { FillBehavior = FillBehavior.HoldEnd };
        static ColorAnimation colorAnim = new ColorAnimation() { FillBehavior = FillBehavior.HoldEnd };

        static Dictionary<Type, EasingFunctionBase> funcs = new Dictionary<Type, EasingFunctionBase>()
        {
            { typeof(BackEase), new BackEase() },
            { typeof(BounceEase), new BounceEase() },
            { typeof(CircleEase), new CircleEase() },
            { typeof(CubicEase), new CubicEase() },
            { typeof(ElasticEase), new ElasticEase() },
            { typeof(ExponentialEase), new ExponentialEase() },
            { typeof(PowerEase), new PowerEase() },
            { typeof(QuadraticEase), new QuadraticEase() },
            { typeof(QuarticEase), new QuarticEase() },
            { typeof(QuinticEase), new QuinticEase() },
            { typeof(SineEase), new SineEase() },
        };

        /// <summary>
        /// Animates an object's opacity.
        /// </summary>
        public static async void AnimateOpacity(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            obj.Opacity = to;
            obj.BeginAnimation(UIElement.OpacityProperty, null);
        }

        /// <summary>
        /// Animates an object's opacity.
        /// </summary>
        public static async Task AnimateOpacityAsync(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(UIElement.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            obj.Opacity = to;
            obj.BeginAnimation(UIElement.OpacityProperty, null);
        }

        /// <summary>
        /// Animates a brush's opacity.
        /// </summary>
        public static async void AnimateBrushOpacity(this Brush brush, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(Brush.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            brush.Opacity = to;
            brush.BeginAnimation(Brush.OpacityProperty, null);
        }

        /// <summary>
        /// Animates a brush's opacity.
        /// </summary>
        public static async Task AnimateBrushOpacityAsync(this Brush brush, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            opacityAnim.From = from;
            opacityAnim.To = to;
            opacityAnim.Duration = TimeSpan.FromMilliseconds(duration);
            opacityAnim.EasingFunction = easing is null ? null : funcs[easing];
            (opacityAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(Brush.OpacityProperty, opacityAnim);

            await Task.Delay(duration);
            brush.Opacity = to;
            brush.BeginAnimation(Brush.OpacityProperty, null);
        }

        /// <summary>
        /// Animates a <see cref="SolidColorBrush"/>'s color.
        /// </summary>
        public static async void AnimateColor(this SolidColorBrush brush, Color from, Color to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            colorAnim.From = from;
            colorAnim.To = to;
            colorAnim.Duration = TimeSpan.FromMilliseconds(duration);
            colorAnim.EasingFunction = easing is null ? null : funcs[easing];
            (colorAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            await Task.Delay(duration);
            brush.Color = to;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        /// <summary>
        /// Animates a <see cref="SolidColorBrush"/>'s color.
        /// </summary>
        public static async Task AnimateColorAsync(this SolidColorBrush brush, Color from, Color to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            colorAnim.From = from;
            colorAnim.To = to;
            colorAnim.Duration = TimeSpan.FromMilliseconds(duration);
            colorAnim.EasingFunction = easing is null ? null : funcs[easing];
            (colorAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

            await Task.Delay(duration);
            brush.Color = to;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's X scale.
        /// </summary>
        public static async void AnimateWidth(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, wAnim);

            await Task.Delay(duration);
            obj.ScaleX = to;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's X scale.
        /// </summary>
        public static async Task AnimateWidthAsync(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, wAnim);

            await Task.Delay(duration);
            obj.ScaleX = to;
            obj.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's Y scale.
        /// </summary>
        public static async void AnimateHeight(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, hAnim);

            await Task.Delay(duration);
            obj.ScaleY = to;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        /// <summary>
        /// Animates a scaling transform's Y scale.
        /// </summary>
        public static async Task AnimateHeightAsync(this ScaleTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, hAnim);

            await Task.Delay(duration);
            obj.ScaleY = to;
            obj.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        }

        /// <summary>
        /// Animates a framework element's width.
        /// </summary>
        public static async void AnimateWidth(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.WidthProperty, wAnim);

            await Task.Delay(duration);
            obj.Width = to;
            obj.BeginAnimation(FrameworkElement.WidthProperty, null);
        }

        /// <summary>
        /// Animates a framework element's width.
        /// </summary>
        public static async void AnimateWidthAsync(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.WidthProperty, wAnim);

            await Task.Delay(duration);
            obj.Width = to;
            obj.BeginAnimation(FrameworkElement.WidthProperty, null);
        }

        /// <summary>
        /// Animates a framework element's height.
        /// </summary>
        public static async void AnimateHeight(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.HeightProperty, hAnim);

            await Task.Delay(duration);
            obj.Height = to;
            obj.BeginAnimation(FrameworkElement.HeightProperty, null);
        }

        /// <summary>
        /// Animates a framework element's height.
        /// </summary>
        public static async void AnimateHeightAsync(this FrameworkElement obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.HeightProperty, hAnim);

            await Task.Delay(duration);
            obj.Height = to;
            obj.BeginAnimation(FrameworkElement.HeightProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's X positon.
        /// </summary>
        public static async void AnimatePositionX(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.XProperty, wAnim);

            await Task.Delay(duration);
            obj.X = to;
            obj.BeginAnimation(TranslateTransform.XProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's X positon.
        /// </summary>
        public static async Task AnimatePositionXAsync(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            wAnim.From = from;
            wAnim.To = to;
            wAnim.Duration = TimeSpan.FromMilliseconds(duration);
            wAnim.EasingFunction = easing is null ? null : funcs[easing];
            (wAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.XProperty, wAnim);

            await Task.Delay(duration);
            obj.X = to;
            obj.BeginAnimation(TranslateTransform.XProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's Y positon.
        /// </summary>
        public static async void AnimatePositionY(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.YProperty, hAnim);

            await Task.Delay(duration);
            obj.Y = to;
            obj.BeginAnimation(TranslateTransform.YProperty, null);
        }

        /// <summary>
        /// Animates a translate transform's Y positon.
        /// </summary>
        public static async Task AnimatePositionYAsync(this TranslateTransform obj, double from, double to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            hAnim.From = from;
            hAnim.To = to;
            hAnim.Duration = TimeSpan.FromMilliseconds(duration);
            hAnim.EasingFunction = easing is null ? null : funcs[easing];
            (hAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(TranslateTransform.YProperty, hAnim);

            await Task.Delay(duration);
            obj.Y = to;
            obj.BeginAnimation(TranslateTransform.YProperty, null);
        }

        /// <summary>
        /// Animates an object's margin.
        /// </summary>
        public static async void AnimateMargin(this FrameworkElement obj, Thickness from, Thickness to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            marginAnim.From = from;
            marginAnim.To = to;
            marginAnim.Duration = TimeSpan.FromMilliseconds(duration);
            marginAnim.EasingFunction = easing is null ? null : funcs[easing];
            (marginAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);

            await Task.Delay(duration);
            obj.Margin = to;
            obj.BeginAnimation(FrameworkElement.MarginProperty, null);
        }

        /// <summary>
        /// Animates an object's margin.
        /// </summary>
        public static async Task AnimateMarginAsync(this FrameworkElement obj, Thickness from, Thickness to, int duration, Type easing, EasingMode mode)
        {
            if (!funcs.ContainsKey(easing)) throw new ArgumentException("The type of " + easing.Name + " does not represent a valid EasingFunctionBase", "easing");

            marginAnim.From = from;
            marginAnim.To = to;
            marginAnim.Duration = TimeSpan.FromMilliseconds(duration);
            marginAnim.EasingFunction = easing is null ? null : funcs[easing];
            (marginAnim.EasingFunction as EasingFunctionBase).EasingMode = mode;
            obj.BeginAnimation(FrameworkElement.MarginProperty, marginAnim);

            await Task.Delay(duration);
            obj.Margin = to;
            obj.BeginAnimation(FrameworkElement.MarginProperty, null);
        }
    }
    
    /// <summary>
    /// Provides static methods useful for mathematical operations.
    /// </summary>
    public static class MathHelper
    {
        /// <summary>
        /// Computes an arithmetic expression. The expression can be combined with methods inside the <see cref="Math"/> class and can use ternary expressions.
        /// </summary>
        public static double Compute(string expression)
        {
            //Check if the expression contains NaN objects
            if (expression.Contains("NaN")) return 0;

            //Remove space chars from expression string and get the number of function calls
            expression = Regex.Replace(expression, @" +", "");
            int max = new Regex(@"Math").Matches(expression).Count;

            using (var dt = new DataTable())
            {
                expression = ReplaceTernary(expression, dt);

                //Replace operations inside function arguments
                foreach (var match in Regex.Matches(expression, @"(?<=Math\.[A-Z][A-Za-z0-9]+\(((\d+(\.\d+)?[*+/%^-]\d+(\.\d+)?[*+/%^-]?)*|,))(\d+(\.\d+)?[*+/%^-]\d+(\.\d+)?[*+/%^-]?)+(?=,|\))").Cast<Match>()) expression = expression.Replace(match.Value, dt.Compute(match.Value, "").ToString());

                //Execute all functions
                for (int i = 0; i < max; i++) foreach (var match in Regex.Matches(expression, @"Math\.[A-Z][A-Za-z0-9]+\(\d+(\.\d+)?\)").Cast<Match>()) expression = expression.Replace(match.Value, typeof(Math).GetMethod(Regex.Match(match.Value, @"(?<=Math\.)[A-Z][A-Za-z0-9]+(?=\()").Value, Enumerable.Repeat(typeof(double), Regex.Matches(match.Value, ",").Count + 1).ToArray()).Invoke(null, new object[] { Convert.ToDouble(Regex.Match(match.Value, @"(?<=Math\.[A-Z][A-Za-z0-9]+\()\d+(\.\d+)?(?=\))").Value) }).ToString());

                expression = ReplaceTernary(expression, dt);

                //Get final result
                return Convert.ToDouble(dt.Compute(expression, ""));
            }

            string ReplaceTernary(string str, DataTable dt)
            {
                foreach (var match in Regex.Matches(str, @"\[[^A-Za-z?><=\[\]]+?[><]\=?[^A-Za-z?><=\[\]]+?\?[^A-Za-z?><=:\[\]]+?\:[^A-Za-z?><=:\[\]]+\]").Cast<Match>())
                {
                    var condition = Regex.Match(match.Value, @"(?<=\[[^A-Za-z?><=\[\]]+?)[><]=?").Value;

                    //Calculate all operands
                    var op0 = Convert.ToDouble(dt.Compute(Regex.Match(match.Value, @"(?<=\[)[^A-Za-z?><=\[\]]+?(?=[><]=?)").Value, ""));
                    var op1 = Convert.ToDouble(dt.Compute(Regex.Match(match.Value, @"(?<=[><]=?)[^A-Za-z?><=\[\]]+?(?=\?)").Value, ""));
                    var op2 = dt.Compute(Regex.Match(match.Value, @"(?<=\?)[^A-Za-z?><=\[\]]+?(?=\:)").Value, "");
                    var op3 = dt.Compute(Regex.Match(match.Value, @"(?<=\:)[^A-Za-z?><=\[\]]+?(?=\])").Value, "");

                    if (condition.Contains(">")) str = str.Replace(match.Value, condition.Contains(">") ? (condition.Contains("=") ? (op0 >= op1 ? op2 : op3).ToString() :
                                                                                                                                                  (op0 > op1 ? op2 : op3).ToString()) :
                                                                                              condition.Contains("<") ? (condition.Contains("=") ? (op0 <= op1 ? op2 : op3).ToString() :
                                                                                                                                                   (op0 > op1 ? op2 : op3).ToString()) :
                                                                                                                                                   "");
                }

                return str;
            }
        }

        /// <summary>
        /// Clamps an <see cref="IComparable"/> between two values.
        /// </summary>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable
        {
            return value.CompareTo(min) < 0 ? min : (value.CompareTo(max) > 0 ? max : value);
        }

        /// <summary>
        /// Clamps an <see cref="IComparable"/> to be higher than <paramref name="min"/>.
        /// </summary>
        public static T Clamp<T>(T value, T min) where T : IComparable
        {
            return value.CompareTo(min) < 0 ? min : value;
        }
    }

    /// <summary>
    /// Provides static methods which simplify obtaining and sending information to the keyboard.
    /// </summary>
    public static class KeyboardHelper
    {
        /// <summary>
        /// Presses a key.
        /// </summary>
        public static void PressKey(Key key)
        {
            var vk = KeyInterop.VirtualKeyFromKey(key);
            keybd_event((byte)vk, (byte)MapVirtualKey((uint)vk, 4), 0, 0);
            keybd_event((byte)vk, (byte)MapVirtualKey((uint)vk, 4), 0x0002, 0);
        }

        /// <summary>
        /// Checks if <paramref name="key"/> is currently held down.
        /// </summary>
        public static bool IsKeyDown(Key key)
        {
            return GetKeyState(KeyInterop.VirtualKeyFromKey(key)) < 0;
        }

        /// <summary>
        /// Checks if <paramref name="key"/> is toggled.
        /// </summary>
        public static bool IsKeyToggled(Key key)
        {
            return GetKeyState(KeyInterop.VirtualKeyFromKey(key)) == 1;
        }

        [DllImport("user32.dll")]
        static extern short GetKeyState(int virtKey);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte virtKey, byte virtScan, uint flags, int info);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint code, uint type);
    }

    /// <summary>
    /// Proivdes static methods which extend the functionality of <see cref="IEnumerable{T}"/> and <see cref="IEnumerator{T}"/> objects.
    /// </summary>
    public static class EnumeratorHelper
    {
        /// <summary>
        /// Gets the first element in <paramref name="e"/>, and resets the enumerator.
        /// </summary>
        public static T GetFirst<T>(this IEnumerator<T> e)
        {
            e.Reset();
            e.MoveNext();
            return e.Current;
        }

        /// <summary>
        /// Creates <paramref name="amount"/> of objects based on <paramref name="obj"/> by using <paramref name="creator"/>. The first parameter of <paramref name="creator"/> is <paramref name="obj"/>.
        /// </summary>
        public static IEnumerable<T> Create<T>(this T obj, Func<T, T> creator, int amount)
        {
            for (int i = 0; i < amount; i++) yield return creator(obj);
        }

        /// <summary>
        /// Creates <paramref name="amount"/> of objects based on <paramref name="obj"/> by using <paramref name="creator"/>. The first parameter of <paramref name="creator"/> is <paramref name="obj"/> and the second is the index of the current creation.
        /// </summary>
        public static IEnumerable<T> Create<T>(this T obj, Func<T, int, T> creator, int amount)
        {
            for (int i = 0; i < amount; i++) yield return creator(obj, i);
        }
    }

    /// <summary>
    /// Provides static methods which allow the addition of handlers to the events of <see cref="UIElement"/> objects.
    /// </summary>
    public static class EventHelper
    {
        #region Fields

        static Dictionary<UIElement, object[]> clickDictionary = new Dictionary<UIElement, object[]>();

        #endregion

        /// <summary>
        /// Adds a <see cref="RoutedEventHandler"/> to an <see cref="UIElement"/> to be invoked when it's clicked.
        /// <para>
        /// If this method is called multiple times on the same <see cref="UIElement"/>, <paramref name="handler"/> will be added to the original handler.
        /// </para>
        /// </summary>
        public static void AddClickHandler(UIElement element, RoutedEventHandler handler)
        {
            //If the element is already registered, just add the new handler to the rest
            if (clickDictionary.ContainsKey(element))
            {
                clickDictionary[element][1] = (clickDictionary[element][1] as RoutedEventHandler) + handler;
                return;
            }

            clickDictionary.Add(element, new object[] { false, handler });

            element.MouseLeftButtonDown += OnElementLeftMouseButtonDown;
            element.MouseLeftButtonUp += OnElementLeftMouseButtonUp;
            element.MouseLeave += OnElementMouseLeave;
        }

        /// <summary>
        /// Called when the left mouse button is depressed onto the <see cref="UIElement"/>.
        /// </summary>
        private static void OnElementLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickDictionary[sender as UIElement][0] = true;
        }

        /// <summary>
        /// Called when the left mouse button is released from the <see cref="UIElement"/>. If the button was initially depressed onto the element, this will invoke the click event.
        /// </summary>
        private static void OnElementLeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Get the object array of the element and return if the mouse wasn't initially depressed
            var data = clickDictionary[sender as UIElement];
            if (!(bool)data[0]) return;

            //Reset the check value and invoke the handlers
            data[0] = false;
            (data[1] as RoutedEventHandler).Invoke(sender, new RoutedEventArgs());
        }

        /// <summary>
        /// Called when the mouse leaves the <see cref="UIElement"/>.
        /// </summary>
        private static void OnElementMouseLeave(object sender, MouseEventArgs e)
        {
            clickDictionary[sender as UIElement][0] = false;
        }
    }
}
