namespace NuGetPackageManager.Controls
{
    using Catel.MVVM;
    using NuGetPackageManager.Controls.Helpers;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    public class InfiniteScrollListBox : ListBox
    {
        private ScrollViewer _scrollViewer;

        public InfiniteScrollListBox()
        {
            Loaded += ListBoxLoaded;
        }

        private void ListBoxLoaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer = WpfHelper.FindVisualChild<ScrollViewer>(this);
        }

        private ScrollViewer ScrollViewer
        {
            get { return _scrollViewer; }
            set
            {
                if (value != _scrollViewer)
                {
                    if (_scrollViewer != null)
                    {
                        _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
                    }

                    _scrollViewer = value;

                    if (_scrollViewer != null)
                    {
                        _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
                    }
                }
            }
        }

        private async void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var first = _scrollViewer.VerticalOffset;
            var last = _scrollViewer.ViewportHeight + first;
            if (_scrollViewer.ViewportHeight > 0 && last >= Items.Count)
            {
                await ExecuteLoadingItemsCommandAsync();
            }
        }

        protected async Task ExecuteLoadingItemsCommandAsync()
        {
            Command?.Execute(CommandParameter);
        }

        public TaskCommand Command
        {
            get { return (TaskCommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(TaskCommand), typeof(InfiniteScrollListBox), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(InfiniteScrollListBox), new PropertyMetadata(0));
    }
}
