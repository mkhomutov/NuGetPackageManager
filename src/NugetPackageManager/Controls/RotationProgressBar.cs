using Catel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace NuGetPackageManager.Controls
{
    public class RotationProgressBar : ProgressBar
    {
        private ILog _log = LogManager.GetCurrentClassLogger();

        public RotationProgressBar()
        {
            this.ValueChanged += RotationProgressBar_ValueChanged;
        }

        private void RotationProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IsInProgress = e.NewValue > Minimum && e.NewValue < Maximum;

            if(!IsInProgress)
            {
                _log.Debug($"{nameof(IsInProgress)} property value changed to {IsInProgress}");
            }
        }

        public double Speed
        {
            get { return (double)GetValue(SpeedProperty); }
            set { SetValue(SpeedProperty, value); }
        }

        //Basic rotation speed
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.Register("Speed", typeof(double), typeof(RotationProgressBar), new PropertyMetadata(1d));


        public Path IconData
        {
            get { return (Path)GetValue(IconDataProperty); }
            set { SetValue(IconDataProperty, value); }
        }


        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register("IconData", typeof(Path), typeof(RotationProgressBar), new PropertyMetadata());


        public bool IsInProgress
        {
            get { return (bool)GetValue(IsInProgressProperty); }
            protected set { SetValue(IsInProgressPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsInProgressPropertyKey =
            DependencyProperty.RegisterReadOnly("IsInProgress", typeof(bool), typeof(RotationProgressBar), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for IsInProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInProgressProperty = IsInProgressPropertyKey.DependencyProperty;

        public bool Success
        {
            get { return (bool)GetValue(SuccessProperty); }
            protected set { SetValue(SuccessPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey SuccessPropertyKey =
           DependencyProperty.RegisterReadOnly("Success", typeof(bool), typeof(RotationProgressBar), new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for Success.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuccessProperty = SuccessPropertyKey.DependencyProperty;



    }
}
