using System;

namespace EDashboard.Core
{
    public class RtTemperaturePoint
    {
        public RtTemperaturePoint(DateTime Time, double Temperature)
        {
            this.Time = Time;
            this.Temperature = Temperature;
        }

        public DateTime Time { get; }

        public double Temperature { get; }
    }
}
