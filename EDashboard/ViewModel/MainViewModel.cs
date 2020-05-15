using EDashboard.Core;
using GalaSoft.MvvmLight;
using System;

namespace EDashboard.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        readonly object lockOvenList = new object();


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            OvenList = new OvenMonitoringDataCollection();

            // Add test ovens.

            for (int i = 1; i <= 5; i++)
            {
                AddNewOven(Guid.NewGuid().ToString("N"), $"Oven {i}");
            }
        }

        #region Properties

        public OvenMonitoringDataCollection OvenList { get; }

        #endregion

        #region Methods

        private void OnOvenHeartbeatTimeout(object sender, EventArgs e)
        {
            var ovenCtx = sender as OvenMonitoringData;
            OvenList.Remove(ovenCtx);
        }

        /// <summary>
        /// 增加一个新的烤箱到烤箱列表。
        /// </summary>
        /// <param name="Hashstring"></param>
        /// <param name="Caption"></param>
        public void AddNewOven(string Hashstring, string Caption)
        {
            lock (lockOvenList)
            {
                var ovenCtx = new OvenMonitoringData(Hashstring);
                ovenCtx.Caption = Caption;
                ovenCtx.OnHeartbeatTimeout += OnOvenHeartbeatTimeout;
                OvenList.Add(ovenCtx);
            }
        }

        #endregion

    }
}