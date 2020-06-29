using EDashboard.OvenMonitoring;
using Meziantou.Framework.WPF.Collections;
using System.Linq;

namespace EDashboard.Core.Extension
{
    public static class OvenListExtension 
    {
        public static Oven FindByHashstring(this ConcurrentObservableCollection<Oven> List, string Hashstring)
        {
            var oven = List.FirstOrDefault(o => o.HashString == Hashstring);
            return oven;
        }
    }
}
