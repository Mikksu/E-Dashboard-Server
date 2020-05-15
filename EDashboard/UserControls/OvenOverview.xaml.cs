using EDashboard.Core;
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

        public OvenMonitoringData DataSource
        {
            get { return (OvenMonitoringData)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(OvenMonitoringData), typeof(OvenOverview), new PropertyMetadata(new OvenMonitoringData(Guid.NewGuid().ToString("N"))));

        #endregion
    }
}
