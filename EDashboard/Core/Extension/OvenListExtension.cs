using System.Collections.ObjectModel;
using System.Linq;

namespace EDashboard.Core.Extension
{
    public static class OvenListExtension 
    {
        public static OvenMonitoringData FindByHashstring(this ObservableCollection<OvenMonitoringData> List, string Hashstring)
        {
            var oven = List.FirstOrDefault(o => o.HashString == Hashstring);
            return oven;
        }
    }
}
