using Catel.IoC;
using Catel.MVVM.Views;
using NuGet.Packaging;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace NuGetPackageManager.Views
{
    /// <summary>
    /// Interaction logic for DependenciesView.xaml
    /// </summary>
    public partial class DependenciesView : Catel.Windows.Controls.UserControl
    {
        static DependenciesView()
        {
            typeof(DependenciesView).AutoDetectViewPropertiesToSubscribe();
        }

        public DependenciesView()
        {
            InitializeComponent();
        }

        [ViewToViewModel("Collection", MappingType=ViewToViewModelMappingType.TwoWayViewWins)]
        public object Collection
        {
            get { return (object)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(object), typeof(DependenciesView), new PropertyMetadata(null, OnCollectionChanged));

        private static void OnCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            
        }
    }
}
