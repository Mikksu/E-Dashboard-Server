using DevExpress.Data;
using DevExpress.Xpf.Bars;
using EDashboard.Core;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EDashboard.OvenMonitoring
{
    public class LotInfo : NotifyPropertyChangedBase, IDisposable
    {
        /// <summary>
        /// limit the max size of the temperature sampling points to 60.
        /// </summary>
        const int MAX_TEMP_SAMPLING_SIZE = 60;

        CancellationTokenSource cts;
        private Oven _oven;
        private int _pcs;
        private DateTime _feedingTime;
        private TimeSpan _bakingTimeRequest;
        private DateTime _bakingEndTime;
        private TimeSpan _overdue;
        private string _operatorName;
        private string _lotNum;
        private double _progress;
        private TimeSpan _remain;
        private Guid _uuid;

        public LotInfo(
            Oven oven, string lotNum, int pcs, TimeSpan bakingDurationRequest, string opName, 
            string ovenHashstring = "", Guid? Uuid = null, DateTime? FeedingTime = null)
        {
            if (Uuid != null)
                this.Uuid = Uuid.Value;
            else
                this.Uuid = Guid.NewGuid();

            if (FeedingTime != null)
                this.FeedingTime = FeedingTime.Value;
            else
                this.FeedingTime = DateTime.Now;

            if (oven == null && OvenHashstring != "")
                this.OvenHashstring = ovenHashstring;

            this.Oven = oven;
            this.LotNum = lotNum;
            this.Pcs = pcs;
            
            this.BakingDurationRequest = bakingDurationRequest;
            this.Operator = opName;
            this.BakingEndTime = this.FeedingTime + bakingDurationRequest;

            TemperatureHistory = new ObservableCollection<double>();

            IProgress<int> progress = new Progress<int>(prog =>
            {
                if (prog == 0)
                {
                    if (DateTime.Now > BakingEndTime)
                    {
                        this.Overdue = DateTime.Now - this.BakingEndTime;
                        this.BakingTimeRemain = TimeSpan.Zero;
                    }
                    else
                    {
                        this.Overdue = TimeSpan.Zero;
                        this.BakingTimeRemain = this.BakingEndTime - DateTime.Now;
                    }

                    this.Progress = (DateTime.Now - this.FeedingTime).TotalSeconds / this.BakingDurationRequest.TotalSeconds * 100;
                }
            });

            Debug.WriteLine($"{LotNum} added!");

            StartBgTask(progress);
        }

        #region Properties

        public Guid Uuid
        {
            get
            {
                return _uuid;
            }
            private set
            {
                _uuid = value;
            }
        }

        /// <summary>
        /// 所属烤箱
        /// </summary>
        public Oven Oven
        {
            get
            {
                return _oven;
            }
            set
            {
                UpdateProperty(ref _oven, value);

                if (this._oven != null)
                {
                    this.OvenHashstring = _oven.HashString;
                    _oven.OnTemperatureUpdated += Oven_OnTemperatureUpdated;
                }
            }
        }

        public string OvenHashstring { get; private set; }
        
        public string LotNum
        {
            get
            {
                return _lotNum;
            }
            private set
            {
                UpdateProperty(ref _lotNum, value);
            }
        }


        /// <summary>
        /// 本Lot内有的产品数量。
        /// </summary>
        public int Pcs
        {
            get
            {
                return _pcs;
            }
            private set
            {
                UpdateProperty(ref _pcs, value);
            }
        }

        /// <summary>
        /// 本Lot放入烤箱的时间。
        /// </summary>
        public DateTime FeedingTime
        {
            get
            {
                return _feedingTime;
            }
            private set
            {
                UpdateProperty(ref _feedingTime, value);
            }
        }


        /// <summary>
        /// 本Lot需要烘烤的时间.
        /// </summary>
        public TimeSpan BakingDurationRequest
        {
            get
            {
                return _bakingTimeRequest;
            }
            private set
            {
                UpdateProperty(ref _bakingTimeRequest, value);
            }
        }



        /// <summary>
        /// 本Lot的烘烤结束时间.
        /// </summary>
        public DateTime BakingEndTime
        {
            get
            {
                return _bakingEndTime;
            }
            private set
            {
                UpdateProperty(ref _bakingEndTime, value);
            }
        }

        /// <summary>
        /// 剩余烘烤时间
        /// </summary>
        public TimeSpan BakingTimeRemain
        {
            get
            {
                return _remain;
            }
            set
            {
                UpdateProperty(ref _remain, value);
            }
        }
   

        /// <summary>
        /// 本Lot烘烤超时的时长.
        /// </summary>
        public TimeSpan Overdue
        {
            get
            {
                return _overdue;
            }
            private set
            {
                UpdateProperty(ref _overdue, value);
            }
        }


        /// <summary>
        /// 操作员名称
        /// </summary>
        public string Operator
        {
            get
            {
                return _operatorName;
            }
            private set
            {
                UpdateProperty(ref _operatorName, value);
            }
        }

        /// <summary>
        /// 获取完成进度。
        /// </summary>
        public double Progress
        {
            get
            {
                return _progress;
            }
            private set
            {
                UpdateProperty(ref _progress, value);
            }
        }

        /// <summary>
        /// 温度历史记录。
        /// </summary>
        public ObservableCollection<double> TemperatureHistory
        {
            get;
        }

        #endregion

        #region Methods

        private void StartBgTask(IProgress<int> progress)
        {
            cts = new CancellationTokenSource();

            Debug.WriteLine($"{LotNum}: background task is running...");

            Task.Run(() =>
            {
                // long duration process

                while (true)
                {
                    progress.Report(0);

                    Thread.Sleep(1000);
                }
            });
        }

        public void Dispose()
        {
            cts?.Cancel();
            Thread.Sleep(100);
        }

        public override string ToString()
        {
            return $"{Overdue}";
        }
        #endregion

        #region Events

        private readonly object tempListLock = new object();

        private void Oven_OnTemperatureUpdated(object sender, double e)
        {
            lock (tempListLock)
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (TemperatureHistory.Count > MAX_TEMP_SAMPLING_SIZE)
                            this.TemperatureHistory.RemoveAt(0);

                        this.TemperatureHistory.Add(e);
                    });
                }
            }
        }

        #endregion
    }
}
