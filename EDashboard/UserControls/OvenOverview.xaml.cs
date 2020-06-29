using EDashboard.Core;
using EDashboard.OvenMonitoring;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EDashboard.UserControls
{
    /// <summary>
    /// Interaction logic for OvenOverview.xaml
    /// </summary>
    public partial class OvenOverview : UserControl
    {
        public OvenOverview()
        {
            InitializeComponent();
        }

        #region Dp

        public Oven DataSource
        {
            get { return (Oven)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(Oven), typeof(OvenOverview), new PropertyMetadata(new Oven(Guid.NewGuid().ToString("N"))));

        #endregion
    }
}
