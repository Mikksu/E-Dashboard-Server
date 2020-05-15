using System.Collections.ObjectModel;
using System.Windows;

namespace EDashboard.Core
{
    public class OvenMonitoringDataCollection : ObservableCollection<OvenMonitoringData>
    {
        readonly object locker = new object();

        protected override void InsertItem(int index, OvenMonitoringData item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.InsertItem(index, item);
            });
        }

        protected override void RemoveItem(int index)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                base.RemoveItem(index);
            });
        }
    }
}
