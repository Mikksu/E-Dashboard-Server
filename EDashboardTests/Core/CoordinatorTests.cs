using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDashboard.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace EDashboard.Core.Tests
{
    [TestClass()]
    public class SqliteDB
    {
        [TestMethod()]
        public void InsertNewRecordTest()
        {
            //using (EDashboardDataContext db = new EDashboardDataContext())
            //{
            //    db.HistoryTable.Add(new History()
            //    {
            //        Uuid = Guid.NewGuid().ToString("N"),
            //        LotNo = "Lot0001",
            //        OvenCaption = "TestOven",
            //        OvenHashString = "Abcd",
            //        Pcs = 25,
            //        FeedingTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.sss"),
            //        RoastingDurationSec = 6000,
            //        OperatorWorkID = "SZH123"
            //    });
            //    db.SaveChanges();

            //}

            string cs = "Data Source=d:\\eDashboard.db";
            string stm = "SELECT SQLITE_VERSION()";
            stm = "SELECT * from RoastingHistory";

            var conn = new SQLiteConnection(cs);
            conn.Open();

            var cmd = new SQLiteCommand("INSERT INTO [RoastingHistory] (Uuid, LotNo, OvenCaption, OvenHashString, pcs, FeedingTime, RoastingDurationSec, OperatorWorkID) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?)", conn);

            cmd.Parameters.Add(new SQLiteParameter() { Value = Guid.NewGuid().ToString("N") });
            cmd.Parameters.Add(new SQLiteParameter() { Value = "Lot0001" });
            cmd.Parameters.Add(new SQLiteParameter() { Value = "TestOven" });
            cmd.Parameters.Add(new SQLiteParameter() { Value = "86B4A19F6C9A4A6981A7E5D072D87292" });
            cmd.Parameters.Add(new SQLiteParameter() { Value = 25 });
            cmd.Parameters.Add(new SQLiteParameter() { Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.sss") });
            cmd.Parameters.Add(new SQLiteParameter() { Value = 6000 });
            cmd.Parameters.Add(new SQLiteParameter() { Value = "SZH123" });

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}