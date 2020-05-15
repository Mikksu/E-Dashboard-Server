using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDashboard.Core
{
    public class ProductInfo
    {
        public string OvenHash { get; set; }

        public int Pcs { get; set; }

        public DateTime FeedingTime { get; set; }

        public TimeSpan BakingDuration { get; set; }

        public DateTime EndTime { get; }

        public string Operator { get; set; }
    }
}
