using DevExpress.Data.Extensions;
using EDashboard.OvenMonitoring;
using Meziantou.Framework.WPF.Collections;
using System;
using System.Linq;
using System.Threading;

namespace EDashboard.Core
{
    public class Coordinator
    {
        #region Variables

        readonly object LotListLock = new object();

        #endregion

        public Coordinator()
        {
            ThreadPool.SetMinThreads(100, 100);

            this.OvenList = new ConcurrentObservableCollection<Oven>();
            this.LotList = new ConcurrentObservableCollection<LotInfo>();
        }


        #region Properties

        public ConcurrentObservableCollection<Oven> OvenList { get; }

        public ConcurrentObservableCollection<LotInfo> LotList { get; }

        #endregion


        #region Methods

        /// <summary>
        /// 增加一个新的烤箱到烤箱列表。
        /// </summary>
        /// <param name="Hashstring"></param>
        /// <param name="Caption"></param>
        public void AddNewOven(string Hashstring, string Caption)
        {
            var ovenCtx = new Oven(Hashstring);
            ovenCtx.Caption = Caption;
            ovenCtx.OnHeartbeatTimeout += (s, e) =>
            {
                OvenList.Remove(s as Oven);
            };
            OvenList.Add(ovenCtx);
        }

        public void AddLot(string OvenHash, string lotNum, int pcs, int bakingSec, string opName)
        {
            lock (LotListLock)
            {
                // find the oven by the hash string.
                var oven = OvenList.FirstOrDefault(x => x.HashString == OvenHash);
                if (oven == null)
                    throw new InvalidOperationException("当前烤箱处于离线状态。");

                // check if the lot has been existed.
                var existLot = LotList.FirstOrDefault(x => x.LotNum == lotNum);
                if (existLot != null)
                    throw new InvalidOperationException($"当前Lot已经存在于烤箱 [{existLot.Oven}] 中。");

                var lotInfo = new LotInfo(oven, lotNum, pcs, new TimeSpan(0, 0, bakingSec), opName);
                this.LotList.Add(lotInfo);
            }
        }

        public void DeleteLot(string LotNum)
        {
            lock(LotListLock)
            {
               
                var id = LotList.FindIndex(x => x.LotNum == LotNum);
                if (id > -1)
                    LotList.RemoveAt(id);
                else
                    throw new InvalidOperationException($"无法找到Lot [{LotNum}]。");
            }
        }

        #endregion
    }
}
