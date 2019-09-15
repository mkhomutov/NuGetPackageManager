﻿namespace NuGetPackageManager.Behaviors
{
    using Catel.IoC;
    using Catel.Windows;
    using Catel.Windows.Interactivity;
    using NuGetPackageManager.Windows.Service;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    public class AnimatedOverlayBehavior : BehaviorBase<DataWindow>
    {
        private Grid topInternalGrid;

        private SizeChangedEventHandler sizeHandler;

        private Storyboard overlayStoryboard;

        private IAnimationService AnimationService { get; set; }

        public Grid OverlayGrid
        {
            get { return (Grid)GetValue(OverlayGridProperty); }
            set { SetValue(OverlayGridProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OverlayGrid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OverlayGridProperty =
            DependencyProperty.Register("OverlayGrid", typeof(Grid), typeof(AnimatedOverlayBehavior), new PropertyMetadata(null, (s, e) => OnOverlayGridChanged(s, e)));

        private static void OnOverlayGridChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var behavior = s as AnimatedOverlayBehavior;

            if (behavior.IsAssociatedObjectLoaded)
            {
                behavior.DetachOverlay(e.OldValue);
                behavior.AttachOverlay(e.NewValue);
            }
        }

        public Grid ActiveContentContainer
        {
            get { return (Grid)GetValue(ActiveContentContainerProperty); }
            set { SetValue(ActiveContentContainerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveDialogContainer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveContentContainerProperty =
            DependencyProperty.Register("ActiveContentContainer", typeof(Grid), typeof(AnimatedOverlayBehavior), new PropertyMetadata(null, (s, e) => OnActiveContentContainerChanged(s, e)));

        private static void OnActiveContentContainerChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var behavior = s as AnimatedOverlayBehavior;

            if (behavior.IsAssociatedObjectLoaded)
            {
                behavior.AttachActiveContainer(e.OldValue);
                behavior.DetachActiveContainer(e.NewValue);
            }
        }

        public UIElement OverlayContent
        {
            get { return (UIElement)GetValue(OverlayContentProperty); }
            set { SetValue(OverlayContentProperty, value); }
        }

        public static readonly DependencyProperty OverlayContentProperty =
            DependencyProperty.Register("OverlayContent", typeof(UIElement), typeof(AnimatedOverlayBehavior), new PropertyMetadata(null));

        [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0005:Name of PropertyChangedCallback should match registered name.", Justification = "<Ожидание>")]
        private void AttachOverlay(object overlay)
        {
            if (overlay != null && overlay is UIElement)
            {
                topInternalGrid.Children.Add(overlay as UIElement);

                //manually hide overlay
                HideOverlay();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0005:Name of PropertyChangedCallback should match registered name.", Justification = "<Ожидание>")]
        private void DetachOverlay(object overlay)
        {
            if (overlay != null && overlay is UIElement)
            {
                topInternalGrid.Children.Remove(overlay as UIElement);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0005:Name of PropertyChangedCallback should match registered name.", Justification = "<Ожидание>")]
        public void AttachActiveContainer(object contentContainer)
        {
            if (contentContainer != null && contentContainer is UIElement)
            {
                topInternalGrid.Children.Add(contentContainer as UIElement);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0005:Name of PropertyChangedCallback should match registered name.", Justification = "<Ожидание>")]
        public void DetachActiveContainer(object contentContainer)
        {
            if (contentContainer != null && contentContainer is UIElement)
            {
                topInternalGrid.Children.Add(contentContainer as UIElement);
            }
        }

        protected override void OnAssociatedObjectLoaded()
        {
            base.OnAssociatedObjectLoaded();

            GetInternalGrid();
            AttachOverlay(OverlayGrid);
            AttachActiveContainer(ActiveContentContainer);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AnimationService = this.GetServiceLocator().ResolveType<IAnimationService>();
        }

        private void GetInternalGrid()
        {
            topInternalGrid = AssociatedObject.FindVisualDescendantByName("_InternalGridName") as Grid;
        }

        protected override void OnIsEnabledChanged()
        {
            base.OnIsEnabledChanged();

            if (IsAssociatedObjectLoaded)
            {
                if (IsEnabled)
                {
                    sizeHandler = SetupAndShowOverlay(OverlayContent);
                }
                else
                {
                    HideOverlayAsync();
                    HideActiveContainer();
                }
            }
        }

        private SizeChangedEventHandler SetupAndShowOverlay(UIElement overlayContent)
        {
            overlayContent.SetCurrentValue(Panel.ZIndexProperty, (int)OverlayGrid.GetValue(Panel.ZIndexProperty) + 1);

            overlayContent.SetCurrentValue(FrameworkElement.MinHeightProperty, AssociatedObject.ActualHeight / 4.0);
            overlayContent.SetCurrentValue(FrameworkElement.MaxHeightProperty, AssociatedObject.ActualHeight);

            SizeChangedEventHandler sizeHandler = (sender, args) =>
            {
                overlayContent.SetCurrentValue(FrameworkElement.MinHeightProperty, AssociatedObject.ActualHeight / 4.0);
                overlayContent.SetCurrentValue(FrameworkElement.MaxHeightProperty, AssociatedObject.ActualHeight);
            };

            AssociatedObject.SizeChanged += sizeHandler;

            AddContentToOverlay(overlayContent);
            ShowOverlayAsync();

            return sizeHandler;
        }

        private void AddContentToOverlay(UIElement overlayContent)
        {
            //window.StoreFocus();

            var activeContent = ActiveContentContainer.Children.Cast<UIElement>().SingleOrDefault();

            if (activeContent != null)
            {
                ActiveContentContainer.Children.Remove(activeContent);
            }

            ActiveContentContainer.Children.Add(overlayContent);
        }

        public Task ShowOverlayAsync()
        {
            if (OverlayGrid == null) throw new InvalidOperationException("Cannot find overlay in Associated object");

            if (OverlayGrid.Visibility == Visibility.Visible && overlayStoryboard == null)
            {
                return Task.CompletedTask;
            }

            if (ActiveContentContainer.Visibility == Visibility.Hidden)
            {
                ActiveContentContainer.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Visible);
            }

            Dispatcher.VerifyAccess();

            var storyboard = AnimationService.GetFadeInAnimation(OverlayGrid);

            overlayStoryboard = storyboard;

            DoubleAnimation animation;

            if (CanUseOverlayFadingStoryboard(storyboard, out animation))
            {
                OverlayGrid.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Visible);

                animation.SetCurrentValue(DoubleAnimation.ToProperty, (double?)0.7);

                EventHandler completionHandler = null;
                completionHandler = (sender, args) =>
                {
                    storyboard.Completed -= completionHandler;
                    if (overlayStoryboard == storyboard)
                    {
                        overlayStoryboard = null;
                    }
                };

                storyboard.Completed += completionHandler;
                OverlayGrid.BeginStoryboard(storyboard);
            }
            else
            {
                ShowOverlay();
            }

            return Task.CompletedTask;
        }

        public System.Threading.Tasks.Task HideOverlayAsync()
        {
            if (OverlayGrid == null) throw new InvalidOperationException("Cannot find overlay in Associated object");

            if (OverlayGrid.Visibility == Visibility.Visible && OverlayGrid.Opacity <= 0.0)
            {
                OverlayGrid.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Hidden);

                return Task.CompletedTask;
            }

            Dispatcher.VerifyAccess();

            var storyboard = AnimationService.GetFadeOutAnimation(OverlayGrid);

            overlayStoryboard = storyboard;

            DoubleAnimation animation;

            if (CanUseOverlayFadingStoryboard(storyboard, out animation))
            {
                animation.SetCurrentValue(DoubleAnimation.ToProperty, 0d);

                EventHandler completionHandler = null;
                completionHandler = (sender, args) =>
                {
                    storyboard.Completed -= completionHandler;
                    if (overlayStoryboard == storyboard)
                    {
                        OverlayGrid.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Hidden);
                        overlayStoryboard = null;
                    }
                };

                storyboard.Completed += completionHandler;
                OverlayGrid.BeginStoryboard(storyboard);
            }
            else
            {
                HideOverlay();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public void HideActiveContainer()
        {
            ActiveContentContainer.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Hidden);
        }

        private bool CanUseOverlayFadingStoryboard(Storyboard sb, out DoubleAnimation animation)
        {
            animation = null;
            if (null == sb)
            {
                return false;
            }

            sb.Dispatcher.VerifyAccess();

            animation = sb.Children.OfType<DoubleAnimation>().FirstOrDefault();
            if (null == animation)
            {
                return false;
            }

            return (sb.Duration.HasTimeSpan && sb.Duration.TimeSpan.Ticks > 0)
                   || (sb.AccelerationRatio > 0)
                   || (sb.DecelerationRatio > 0)
                   || (animation.Duration.HasTimeSpan && animation.Duration.TimeSpan.Ticks > 0)
                   || animation.AccelerationRatio > 0
                   || animation.DecelerationRatio > 0;
        }

        public void ShowOverlay()
        {
            OverlayGrid.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Visible);
            OverlayGrid.SetCurrentValue(Grid.OpacityProperty, 0.7);
        }

        public void HideOverlay()
        {
            OverlayGrid.SetCurrentValue(Grid.OpacityProperty, 0d);
            OverlayGrid.SetCurrentValue(UIElement.VisibilityProperty, Visibility.Hidden);
        }
    }
}
