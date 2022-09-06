﻿using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Windows.Navigation;
using MessengerClient.Controls;
using System.Windows.Documents;
using MessengerClient.Helpers;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.ComponentModel;
using MessengerClient.Users;
using MessengerClient.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows;
using System.Linq;
using System;

namespace MessengerClient
{
    public partial class MainWindow : Window
    {
        #region Fields

        LoginSession session = new LoginSession();
        List<RoundTextBox> loginBoxes;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            PrepareWindow();
        }

        /// <summary>
        /// Only called when the program is started. Prepares any elements before they are actually shown.
        /// </summary>
        private void PrepareWindow()
        {
            Top = SystemParameters.PrimaryScreenHeight / 6;
            Left = SystemParameters.PrimaryScreenWidth / 6;
            resizeThumb.IsHitTestVisible = false;
            resizeThumb.Opacity = 0;
            passBox.Opacity = 0;
            userBox.Opacity = 0;
            exitBtn.Opacity = 0;
            minBtn.Opacity = 0;
            Opacity = 0;

            //Set the text boxes
            loginBoxes = winGrid.Children.Cast<UIElement>().Where(x => Regex.IsMatch((x as RoundTextBox)?.Name ?? "", "(user|email|pass|confirm)Box")).Select(x => { (x as RoundTextBox).TextFinished += OnLoginBoxTextFinished; return x as RoundTextBox; }).ToList();
        }

        /// <summary>
        /// Plays a splash screen based on an image. If the image is faded out, this method returns <see langword="null"/>, otherwise, returns the image with the animated values.
        /// </summary>
        private async Task<Image> PlaySplash(int fadeIn, int? fadeOut, int totalDuration, Uri imageUri)
        {
            //Add the image to the window
            var img = new Image() { Source = new BitmapImage(imageUri), 
                                    Opacity = 0, 
                                    Width = 400, 
                                    Height = 400, 
                                    RenderTransform = new ScaleTransform(), 
                                    RenderTransformOrigin = new Point(0.5, 0.5) };
            winGrid.Children.Add(img);

            img.AnimateOpacity(0, 1, fadeIn, typeof(SineEase), EasingMode.EaseIn);
            (img.RenderTransform as ScaleTransform).AnimateWidth(1, 1.1, totalDuration, typeof(QuinticEase), EasingMode.EaseInOut);
            if (!fadeOut.HasValue)
            {
                await (img.RenderTransform as ScaleTransform).AnimateHeightAsync(1, 1.1, totalDuration, typeof(QuinticEase), EasingMode.EaseInOut);
                return img;
            }
            (img.RenderTransform as ScaleTransform).AnimateHeight(1, 1.1, totalDuration, typeof(QuinticEase), EasingMode.EaseInOut);

            //Await until fading out
            await Task.Delay(totalDuration - fadeOut.Value);

            //Fade out
            img.AnimateOpacity(1, 0, fadeOut.Value, typeof(SineEase), EasingMode.EaseOut);

            //Remove the image
            await Task.Delay(fadeOut.Value);
            winGrid.Children.Remove(img);
            return null;
        }

        /// <summary>
        /// Fades the window in or out, scaling it from the center accordingly. If <paramref name="minimize"/> is <see langword="true"/>, the window is scaled from the bottom.
        /// </summary>
        private async Task FadeWindow(int duration, bool fadeOut, bool minimize)
        {
            //Set the origin
            var ogOrigin = RenderTransformOrigin;
            RenderTransformOrigin = new Point(0.5, minimize ? 1 : 0.5);

            //Begin animations
            this.AnimateOpacity(Opacity, Convert.ToInt32(!fadeOut), duration, typeof(QuinticEase), fadeOut ? EasingMode.EaseIn : EasingMode.EaseOut);
            winScale.AnimateWidth(fadeOut ? winScale.ScaleX : 0.9, fadeOut ? 0.9 : 1, duration, typeof(QuinticEase), fadeOut ? EasingMode.EaseIn : EasingMode.EaseOut);
            winScale.AnimateHeight(fadeOut ? winScale.ScaleY : 0.9, fadeOut ? 0.9 : 1, duration, typeof(QuinticEase), fadeOut ? EasingMode.EaseIn : EasingMode.EaseOut);

            await Task.Delay(duration);

            //Set all properties
            RenderTransformOrigin = ogOrigin;
        }

        /// <summary>
        /// Called when the window is first initialized. Plays all initial animations.
        /// </summary>
        private async void OnWindowInitialized(object sender, EventArgs e)
        {
            await Task.Delay(150);
            await FadeWindow(250, false, false);
            await Task.Delay(250);

            //Animate the splash
            var splash = await PlaySplash(750, null, 2000, new Uri("Resources/logo_main.png", UriKind.RelativeOrAbsolute));
            (splash.RenderTransform as ScaleTransform).AnimateWidth(1.1, 0.1, 750, typeof(QuinticEase), EasingMode.EaseOut);
            (splash.RenderTransform as ScaleTransform).AnimateHeight(1.1, 0.1, 750, typeof(QuinticEase), EasingMode.EaseOut);
            await splash.AnimateMarginAsync(splash.Margin, new Thickness(-(winGrid.Width - 60), -splash.Height / 1.16, 0, winGrid.Height - splash.Height), 750, typeof(QuinticEase), EasingMode.EaseOut);

            //Bind the splash
            var splashBind = new MultiBinding() { Converter = FindResource("margin") as IMultiValueConverter, ConverterParameter = "f-({0}-60),-{2}/1.16,0,{1}-{2}" };
            splashBind.Bindings.Add(new Binding() { ElementName = "winGrid", Path = new PropertyPath("Width") });
            splashBind.Bindings.Add(new Binding() { ElementName = "winGrid", Path = new PropertyPath("Height") });
            splashBind.Bindings.Add(new Binding() { RelativeSource = RelativeSource.Self, Path = new PropertyPath("Height") });
            splash.SetBinding(MarginProperty, splashBind);

            //Set the splash's quality
            RenderOptions.SetBitmapScalingMode(splash, BitmapScalingMode.Fant);

            //Bring back the exit button
            exitBtn.AnimateOpacity(0, 1, 250, typeof(SineEase), EasingMode.EaseOut);
            (exitBtn.RenderTransform as ScaleTransform).AnimateWidth(0.7, 0.8, 250, typeof(QuinticEase), EasingMode.EaseOut);
            (exitBtn.RenderTransform as ScaleTransform).AnimateHeight(0.7, 0.8, 250, typeof(QuinticEase), EasingMode.EaseOut);
            await Task.Delay(125);

            //Bring back the minimize button
            minBtn.AnimateOpacity(0, 1, 250, typeof(SineEase), EasingMode.EaseOut);
            (minBtn.RenderTransform as ScaleTransform).AnimateWidth(0.7, 0.8, 250, typeof(QuinticEase), EasingMode.EaseOut);
            (minBtn.RenderTransform as ScaleTransform).AnimateHeight(0.7, 0.8, 250, typeof(QuinticEase), EasingMode.EaseOut);

            //Bring back the resizer
            await resizeThumb.AnimateOpacityAsync(0, 1, 250, typeof(SineEase), EasingMode.EaseOut);
            ResizeMode = ResizeMode.CanResizeWithGrip;
            resizeThumb.IsHitTestVisible = true;

            //Show welcome text
            welcomeTxt.Text = "(b,f1.1)[WELCOME TO]\n(w1,c#FBD6FB-#BDD7F9,b)[Messenger]";
            await welcomeTxt.AwaitQueue();
            await Task.Delay(1000);

            //Shrink and move welcome text to the top
            var introScale = new ScaleTransform();
            welcomeTxt.RenderTransform = introScale;
            introScale.AnimateWidth(1, 0.7, 750, typeof(QuinticEase), EasingMode.EaseOut);
            introScale.AnimateHeight(1, 0.7, 750, typeof(QuinticEase), EasingMode.EaseOut);
            await welcomeTxt.AnimateMarginAsync(new Thickness(0, 0, 0, 0), new Thickness(0, 0, 0, winGrid.Height - 150), 750, typeof(QuinticEase), EasingMode.EaseOut);

            //Set the binding
            var textBinding = new MultiBinding() { Converter = FindResource("margin") as IMultiValueConverter, ConverterParameter = "f0,0,0,{0} - 150" };
            textBinding.Bindings.Add(new Binding() { ElementName = "winGrid", Path = new PropertyPath("Height") });
            welcomeTxt.SetBinding(MarginProperty, textBinding);

            await Task.Delay(150);

            //Show the text boxes
            userBox.AnimateOpacity(0, 1, 500, typeof(SineEase), EasingMode.EaseOut);
            await Task.Delay(150);
            await passBox.AnimateOpacityAsync(0, 1, 500, typeof(SineEase), EasingMode.EaseOut);
            await Task.Delay(250);

            //Show the log in tip
            tipperText.Text = "LOG IN OR (lb,c#00bbff-#0873ff)[SIGN UP] TO CONTINUE";
            tipperText.LinkHandlerAdded += OnTipperHandlerAdded;
            tipperText.LinkHandlersCleared += OnTipperHandlersCleared;

            keepSignText.Text = "KEEP ME SIGNED IN";
        }

        /// <summary>
        /// Called when Alt+F4 is hit. Ensures the closing animation will still play.
        /// </summary>
        private async void OnWindowClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            OnExitClick(sender, new RoutedEventArgs());
            await Task.Delay(150);
        }

        /// <summary>
        /// Animates the closing of the window.
        /// </summary>
        private async void OnExitClick(object sender, RoutedEventArgs e)
        {
            await FadeWindow(250, true, false);
            await Task.Delay(150);
            Environment.Exit(0);
        }

        /// <summary>
        /// Animates the minimization of the window.
        /// </summary>
        private async void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            await FadeWindow(250, true, true);
            await Task.Delay(150);
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Animates the normalization of the window.
        /// </summary>
        private async void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (!WindowState.Equals(WindowState.Normal)) return;
            await FadeWindow(250, false, true);
        }

        /// <summary>
        /// Drags the window.
        /// </summary>
        private void OnDraggerDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        /// <summary>
        /// Called when a link handler is added to the tipper text. This will only be called once per text change. Only registers the click event of the handler.
        /// </summary>
        private void OnTipperHandlerAdded(object sender, LinkHandlerAddedEventArgs e)
        {
            e.Handler.Handler += OnTipperHandlerClicked;
        }

        /// <summary>
        /// Called when all link handlers are removed from the tipper text. Unregisters all click events. 
        /// </summary>
        private void OnTipperHandlersCleared(object sender, LinkHandlersClearedEventArgs e)
        {
            for (int i = 0; i < e.Handlers.Count; i++) e.Handlers[i].Handler = null;
        }

        /// <summary>
        /// Called when the only handler of the tipper text is clicked. Animates the text boxes and changes the tipper text accordingly.
        /// </summary>
        private void OnTipperHandlerClicked()
        {
            //Ensure if signing in is required and change the tipper text accordingly
            session.SignInRequired ^= true;
            tipperText.Text = session.SignInRequired ? "<i>(lb,c#00bbff-#0873ff)[LOG IN] OR SIGN UP TO CONTINUE" : "<i>LOG IN OR (lb,c#00bbff-#0873ff)[SIGN UP] TO CONTINUE";

            //Change the user box's preview text accordingly
            userBox.PreviewText = session.SignInRequired ? "Username" : "Username or Email";

            //Animate the heights
            userBox.AnimateHeight(session.SignInRequired ? 100 : 75,
                                  session.SignInRequired ? 75 : 100,
                                  250,
                                  typeof(QuinticEase),
                                  EasingMode.EaseOut);

            passBox.AnimateHeight(session.SignInRequired ? 100 : 75,
                                  session.SignInRequired ? 75 : 100,
                                  250,
                                  typeof(QuinticEase),
                                  EasingMode.EaseOut);

            //Animate the margin
            passBox.AnimateMargin(new Thickness(0, 0, 0, session.SignInRequired ? -150 : -200), 
                                  new Thickness(0, 0, 0, session.SignInRequired ? -200 : -150),
                                  250, 
                                  typeof(QuinticEase), 
                                  EasingMode.EaseOut);

            //Animate the opacities
            emailBox.AnimateOpacity(Convert.ToInt32(!session.SignInRequired), Convert.ToInt32(session.SignInRequired), 250, typeof(SineEase), EasingMode.EaseOut);
            confirmBox.AnimateOpacity(Convert.ToInt32(!session.SignInRequired), Convert.ToInt32(session.SignInRequired), 250, typeof(SineEase), EasingMode.EaseOut);
        }

        /// <summary>
        /// Called when the keep sign in text is clicked.
        /// </summary>
        private void OnKeepTextClick(object sender, RoutedEventArgs e)
        {
            session.ShouldPersist ^= true;
            keepSignText.Text = session.ShouldPersist ? "<i>(c#00FF00,b)[YOU WILL BE KEPT SIGNED IN]" : "<i>KEEP ME SIGNED IN";
        }

        private void OnLoginBoxTextFinished(object sender, RoutedEventArgs e)
        {
            //Only perform the login if the last text box, depending on the requirement of signing in, is unfocused
            if ((session.SignInRequired && sender == loginBoxes.Last()) || (!session.SignInRequired && sender == loginBoxes[2]))
            {
                PerformLogin();
                return;
            }

            loginBoxes[loginBoxes.IndexOf(sender as RoundTextBox) + (session.SignInRequired ? 1 : 2)].Focus();
        }

        private void PerformLogin()
        {
            Console.WriteLine("logging in");
        }
    }
}
