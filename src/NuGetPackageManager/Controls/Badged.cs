﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NuGetPackageManager.Controls
{
    [TemplatePart(Name = BadgeContentPartName, Type = typeof(FrameworkElement))]
    public class Badged : ContentControl
    {
        const string BadgeContentPartName = "PART_BadgeContent";

        public object Badge
        {
            get { return (object)GetValue(BadgeProperty); }
            set { SetValue(BadgeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BadgeProperty =
            DependencyProperty.Register("Badge", typeof(object), typeof(Badged), new PropertyMetadata(null));

        public Brush BadgeForeground
        {
            get { return (Brush)GetValue(BadgeForegroundProperty); }
            set { SetValue(BadgeForegroundProperty, value); }
        }

        public static readonly DependencyProperty BadgeForegroundProperty =
            DependencyProperty.Register("BadgeForeground", typeof(Brush), typeof(Badged), new PropertyMetadata(null));

        //public Brush BadgeBackground
        //{
        //    get { return (Brush)GetValue(BadgeBackgroundProperty); }
        //    set { SetValue(BadgeBackgroundProperty, value); }
        //}

        //public static readonly DependencyProperty BadgeBackgroundProperty =
        //    DependencyProperty.Register("BadgeBackground", typeof(Brush), typeof(Badged), new PropertyMetadata(null));

        public bool IsShowed
        {
            get { return (bool)GetValue(IsShowedProperty); }
            set { SetValue(IsShowedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsShowed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsShowedProperty =
            DependencyProperty.Register("IsShowed", typeof(bool), typeof(Badged), new PropertyMetadata(true, (s,e) => OnIsShowedChanged(s,e)));

        private static void OnIsShowedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var badged = sender as Badged;

            if(badged == null)
            {
                return;
            }
            
            var templateBadge = badged.GetTemplateChild(BadgeContentPartName);

            if(templateBadge != null)
            {
                templateBadge.SetCurrentValue(VisibilityProperty, (bool)e.NewValue ? Visibility.Visible : Visibility.Hidden);
            }   
        }
    }
}
