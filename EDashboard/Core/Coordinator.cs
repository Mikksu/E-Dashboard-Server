using DevExpress.Data.Extensions;
using DevExpress.DataProcessing;
using EDashboard.OvenMonitoring;
using Grpc.Core;
using Meziantou.Framework.WPF.Collections;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

            IProgress<LotInfo[]> prog = new Progress<LotInfo[]>(x =>
            {
                LotList.AddRange(x);
                
            });

            Task.Run(() =>
            {
                loadUnfinishedLots(prog);
            });
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

            // binding the oven to the lots....
            var lots = LotList.Where(x => x.OvenHashstring == ovenCtx.HashString);
            foreach (var lot in lots)
                lot.Oven = ovenCtx;


            ovenCtx.LotAmount = lots.Count();
        }

        public void AddLot(string OvenHash, string lotNum, int pcs, int bakingSec, string opName)
        {
            lock (LotListLock)
            {
                // find the oven by the hash string.
                var oven = OvenList.FirstOrDefault(x => x.HashString == OvenHash);
                if (oven == null)
                    throw new RpcException(new Status(StatusCode.Unknown, "当前烤箱处于离线状态。"));

                // check if the lot has been existed.
                var existLot = LotList.FirstOrDefault(x => x.LotNum == lotNum);
                if (existLot != null)
                    throw new RpcException(new Status(StatusCode.Unknown, $"当前Lot已经存在于烤箱 [{existLot.Oven}] 中。"));

                var lotInfo = new LotInfo(oven, lotNum, pcs, new TimeSpan(0, 0, bakingSec), opName);
                this.LotList.Add(lotInfo);

                // calculate the amount of the lot no. in the oven.
                oven.LotAmount = _calculate_lotNum_in_oven(OvenHash);

                try
                {
                    var db = new SqliteDB();
                    db.InsertNewLot(lotInfo);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }


        public void DeleteLot(string OvenHashString, string LotNum)
        {
            lock(LotListLock)
            {
                var lot = LotList.FirstOrDefault(x => x.LotNum == LotNum && x.Oven.HashString == OvenHashString);
                if(lot == null)
                    throw new RpcException(new Status(StatusCode.Unknown, $"无法在烤箱 [{OvenHashString}] 中找到Lot [{LotNum}]。"));

                // update the overdue field of the database...
                var db = new SqliteDB();
                db.RoastingFinished(lot);

                // remove the lot from the list.
                var id = LotList.FindIndex(x => x.Oven.HashString == OvenHashString && x.LotNum == LotNum);
                if (id > -1)
                    LotList.RemoveAt(id);
                else
                    throw new RpcException(new Status(StatusCode.Unknown, $"无法在烤箱 [{OvenHashString}] 中找到Lot [{LotNum}]。"));

                // calculate lot number in the oven.
                var oven = OvenList.FirstOrDefault(x => x.HashString == OvenHashString);
                if(oven != null)
                {
                    oven.LotAmount = _calculate_lotNum_in_oven(OvenHashString);
                }
            }
        }

        /// <summary>
        /// Check are there overly roasted lot in the specified oven.
        /// </summary>
        /// <param name="OvenHashString"></param>
        /// <param name="MaxReturned">The max number of overdued lot the method returned.</param>
        /// <returns></returns>
        public LotInfo[] CheckOverRoastLot(string OvenHashString, int MaxReturned = 5)
        {
            lock (LotListLock)
            {
                try
                {
                    var lots = LotList
                        .OrderByDescending(a => a.Overdue)
                        .Where(x => x.Oven.HashString == OvenHashString && x.Overdue.TotalSeconds > 0)
                        .Take(5);

                    return lots.ToArray();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        #endregion

        #region Private Methods

        private void loadUnfinishedLots(IProgress<LotInfo[]> progress)
        {
            var db = new SqliteDB();

            try
            {
                var lots = db.FindUnfinishedLot();
                if (lots.Length > 0)
                    progress.Report(lots);
            }
            catch(AggregateException ae)
            {

            }
        }


        /// <summary>
        /// Calculate the lot amount in the specified oven.
        /// </summary>
        /// <param name="ovenHashstring"></param>
        /// <returns></returns>
        private int _calculate_lotNum_in_oven(string ovenHashstring)
        {
            // calculate the amount of the lot no. in the oven.
            return LotList.Count(x => x.Oven.HashString == ovenHashstring);
        }
        #endregion
    }
}
