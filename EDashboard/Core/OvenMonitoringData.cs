using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EDashboard.Core
{
    public class OvenMonitoringData : NotifyPropertyChangedBase
    {
        #region Events

        public event EventHandler OnHeartbeatTimeout;

        #endregion

        private double _minTemp;
        private double _maxTemp;
        private double _diffTemp;
        private double _realtimeTemp;
        private DateTime _lastTempReported;
        private string _caption;

        private CancellationTokenSource cts;
        private string _proximate;

        public OvenMonitoringData(string HashString)
        {
            this.RegisteredTime = DateTime.Now;
            this.LastHeartbeatReportedTime = RegisteredTime;
            this.HashString = HashString;
            this.HashStringShort = HashString.Substring(HashString.Length - 7); // get the last 6 chars for the short format.
            this.RealtimeTemperature = 99.9;
            this.IsRemoveMeRequested = false;
            this.TemperatureHistory = new List<RtTemperaturePoint>();
            this.ProductList = new List<ProductInfo>();

            // start the heartbeat task.
            cts = new CancellationTokenSource();
            IProgress<HeartbeatReport> progress = new Progress<HeartbeatReport>(report =>
            {
                switch (report.Report)
                {
                    case HeartbeatReport.ReportEnum.RemoveMe:
                        this.IsRemoveMeRequested = true;
                        OnHeartbeatTimeout?.Invoke(this, new EventArgs()); // remove me from the oven list.
                        break;

                    case HeartbeatReport.ReportEnum.RealtimeTemperature:

                        break;

                    case HeartbeatReport.ReportEnum.Proximate:
                        this.Proximate = report.StringValue;
                        break;

                    default:
                        Trace.WriteLine("Undefined report enum.");
                        break;
                }

            });

            Heartbeat(cts.Token, progress).Start();
        }

        #region Properties

        public string HashString { get; }

        public string HashStringShort { get; }

        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                UpdateProperty(ref _caption, value);
            }
        }

        /// <summary>
        /// Record the very first time when the oven is registered.
        /// </summary>
        public DateTime RegisteredTime { get; }

        /// <summary>
        /// Get the time when the last temperature is reported.
        /// <para>The value is also used as the heartbeat of the communication.</para>
        /// </summary>
        public DateTime LastHeartbeatReportedTime
        {
            get
            {
                return _lastTempReported;
            }
            private set
            {
                UpdateProperty(ref _lastTempReported, value);
            }
        }

        /// <summary>
        /// Get the real time temperature.
        /// </summary>
        public double RealtimeTemperature
        {
            get
            {
                return _realtimeTemp;
            }
            private set
            {
                UpdateProperty(ref _realtimeTemp, value);
            }
        }

        /// <summary>
        ///  Get the minimum temperature.
        /// </summary>
        public double MinTemperature
        {
            get
            {
                return _minTemp;
            }
            private set
            {
                UpdateProperty(ref _minTemp, value);
            }
        }

        /// <summary>
        /// Get the maximum temperature.
        /// </summary>
        public double MaxTemperature
        {
            get
            {
                return _maxTemp;
            }
            private set
            {
                UpdateProperty(ref _maxTemp, value);
            }
        }

        /// <summary>
        /// Get the temperature difference.
        /// </summary>
        public double DiffTemperature
        {
            get
            {
                return _diffTemp;
            }
            private set
            {
                UpdateProperty(ref _diffTemp, value);
            }
        }

        /// <summary>
        /// 心跳信号超时，从列表中移除我。
        /// </summary>
        public bool IsRemoveMeRequested { get; private set; }

        /// <summary>
        /// 最近的一个随工单还剩多久到达烘烤时间。
        /// </summary>
        public string Proximate
        {
            get
            {
                return _proximate;
            }
            private set
            {
                UpdateProperty(ref _proximate, value);
            }
        }

        public List<RtTemperaturePoint> TemperatureHistory { get; }

        public List<ProductInfo> ProductList { get; } 

        #endregion

        #region Methods

        public void AddRealtimeTemperaturePoint(double Temperature, DateTime? Time = null)
        {
            // 获取当前时间
            DateTime time = DateTime.Now;
            if (Time.HasValue)
                time = Time.Value;

            // 设置实时温度
            RealtimeTemperature = Temperature;
            
            // 添加实时温度到历史列表，并删除指定时间之前的温度采样点。
            TemperatureHistory.Add(new RtTemperaturePoint(time, Temperature));
            var lst = TemperatureHistory.RemoveAll(p => p.Time < DateTime.Now.AddMinutes(-10));

            // 刷新心跳包超时时间
            LastHeartbeatReportedTime = DateTime.Now;

            // 更新历史温度最大值、最小值和差值
            MinTemperature = TemperatureHistory.Min(p => p.Temperature);
            MaxTemperature = TemperatureHistory.Max(p => p.Temperature);
            DiffTemperature = MaxTemperature - MinTemperature;
        }

        /// <summary>
        /// The background task to check the client connection.
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="Progress"></param>
        /// <returns></returns>
        public Task Heartbeat(CancellationToken ct, IProgress<HeartbeatReport> Progress)
        {
            return new Task(() =>
            {
                while (true)
                {
                    // if the heartbeat is not arrived in 5s, remove me.
                    var ts = DateTime.Now - LastHeartbeatReportedTime;
                    if (ts.TotalSeconds >= 5)
                    {
                        Progress.Report(new HeartbeatReport(HeartbeatReport.ReportEnum.RemoveMe, this));
                        break;
                    }

                    // Calculate proximate.
                    string proximate = "";
                    if (ProductList.Count <= 0)
                        proximate =  "--:--:--";
                    else
                    {
                        var dt = ProductList.Max(p => p.EndTime);
                        var remain = dt - DateTime.Now;
                        proximate = remain.ToString("HH:mm:ss");
                    }
                    Progress.Report(new HeartbeatReport(HeartbeatReport.ReportEnum.Proximate, proximate));

                    Thread.Sleep(1000);
                }
            });
        }


        public override string ToString()
        {
            return $"{Caption} - {HashString}";
        }

        #endregion


    }


    public class HeartbeatReport
    {
        public enum ReportEnum
        {
            Proximate,
            RealtimeTemperature,
            RemoveMe
        }

        public HeartbeatReport(ReportEnum Report)
        {
            this.Report = Report;
        }

        public HeartbeatReport(ReportEnum Report, OvenMonitoringData OvenContext) : this(Report)
        {
            this.OvenContext = OvenContext;
        }

        public HeartbeatReport(ReportEnum Report, double Temperature) : this(Report)
        {
            this.Temperature = Temperature;
        }

        public HeartbeatReport(ReportEnum Report, string StringValue) : this(Report)
        {
            this.StringValue = StringValue;
        }

        #region Properties

        public ReportEnum Report { get; }

        public OvenMonitoringData OvenContext { get; }

        public double Temperature { get; }

        public string StringValue { get; }

        #endregion
    }
}
