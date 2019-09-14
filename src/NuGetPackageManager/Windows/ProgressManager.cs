namespace NuGetPackageManager.Windows
{
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Windows;
    using Catel.Windows.Interactivity;
    using NuGetPackageManager.Behaviors;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Interactivity;

    public class ProgressManager : IProgressManager
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private Dictionary<IViewModel, DataWindow> _storedManagedWindows = new Dictionary<IViewModel, DataWindow>();

        public void ShowBar(IViewModel vm)
        {
            var window = GetCurrentActiveDataWindow();

            foreach (var behavior in GetOverlayBehaviors(window))
            {
                var progressBehavior = behavior as AnimatedOverlayBehavior;
                progressBehavior.SetCurrentValue(BehaviorBase<DataWindow>.IsEnabledProperty, true);

                _storedManagedWindows.Add(vm, window);
            }
        }

        public void HideBar(IViewModel vm)
        {
            DataWindow window = null;

            if (_storedManagedWindows.TryGetValue(vm, out window))
            {
                Log.Info($"Current window is { (window == null ? "null" : window.ToString()) }");

                Log.Info($"List of current windows {Application.Current.Windows}");

                foreach (var behavior in GetOverlayBehaviors(window))
                {
                    behavior.SetCurrentValue(BehaviorBase<DataWindow>.IsEnabledProperty, false);
                    _storedManagedWindows.Remove(vm);
                }
            }
        }

        private DataWindow GetCurrentActiveDataWindow()
        {
            return Application.Current.Windows.OfType<DataWindow>().FirstOrDefault(x => x.IsActive);
        }

        private IEnumerable<AnimatedOverlayBehavior> GetOverlayBehaviors(DataWindow window)
        {
            var behaviors = Interaction.GetBehaviors(window);

            return behaviors.OfType<AnimatedOverlayBehavior>();
        }
    }
}
