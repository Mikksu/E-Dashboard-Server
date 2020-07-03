using EDashboard.OvenMonitoring;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace EDashboard
{
    public class SqliteDB
    {

        SQLiteConnection conn;

        public SqliteDB()
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "eDashboard.db");

            if (File.Exists(dbPath) == false)
                throw new FileNotFoundException("无法找到数据库文件。");

            conn = new SQLiteConnection($"Data Source={dbPath}");

            conn.Open();
        }

        public void InsertNewLot(LotInfo lot)
        {
            var cmd = new SQLiteCommand(
                "INSERT INTO [RoastingHistory] " +
                "(uuid, lotNo, OvenCaption, OvenHashString, pcs, FeedingTime, " +
                "RoastingDurationSec, OperatorID) " +
                "VALUES (?, ?, ?, ?, ?, ?, ?, ?)", conn);

            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.Uuid.ToString() });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.LotNum });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.Oven.Caption });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.Oven.HashString });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.Pcs });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.FeedingTime.ToString("yyyy-MM-dd HH:mm:ss.sss") });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.BakingDurationRequest.TotalSeconds });
            cmd.Parameters.Add(new SQLiteParameter() { Value = lot.Operator });

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateRoastingRemained(LotInfo lot)
        {
            var cmd = new SQLiteCommand(
                $"UPDATE RoastingHistory " +
                $"SET " +
                $"RoastingRemainedSec = {lot.BakingTimeRemain.TotalSeconds} " +
                $"WHERE " +
                $"uuid = '{lot.Uuid}'");

            cmd.ExecuteNonQuery();
        }

        public void RoastingFinished(LotInfo lot)
        {
            var cmd = new SQLiteCommand(
                $"UPDATE RoastingHistory " +
                $"SET " +
                $"RoastingRemainedSec = {lot.BakingTimeRemain.TotalSeconds}, " +
                $"OverdueSec = {lot.Overdue.TotalSeconds} " +
                $"WHERE " +
                $"uuid = '{lot.Uuid}'", conn);

            cmd.ExecuteNonQuery();
        }

        public LotInfo[] FindUnfinishedLot()
        {

            List<Exception> lstExp = new List<Exception>();
            List<LotInfo> lst = new List<LotInfo>();

            var cmdtxt =
                "SELECT " +
                "Uuid, LotNo, OvenCaption, OvenHashString, Pcs, FeedingTime, " +
                "RoastingDurationSec, OperatorID " +
                "from RoastingHistory " +
                "where OverdueSec is NULL " +
                "ORDER BY FeedingTime DESC " +
                "LIMIT 50";

            var cmd = new SQLiteCommand(cmdtxt, conn);
            var dr = cmd.ExecuteReader();

            if(dr.HasRows)
            {
                while (dr.Read())
                {
                    try
                    {
                        var uuid = Guid.Parse(dr["uuid"].ToString());
                        var ovenHashstring = dr["OvenHashString"].ToString();
                        var pcs = Convert.ToInt32(dr["Pcs"].ToString());
                        var roastingDuration = TimeSpan.FromSeconds(Convert.ToInt32(dr["RoastingDurationSec"].ToString()));
                        var feedingTime = Convert.ToDateTime(dr["FeedingTime"].ToString());

                        lst.Add(new LotInfo(null, dr["LotNo"].ToString(), pcs, roastingDuration, dr["OperatorID"].ToString(), ovenHashstring, uuid, feedingTime));
                    }
                    catch(Exception ex)
                    {
                        lstExp.Add(ex);
                    }
                }
            }

            if (lstExp.Count > 0)
            {
                throw new AggregateException(lstExp.ToArray());
            }
            else
            {
                return lst.ToArray();
            }
        }
    }
}