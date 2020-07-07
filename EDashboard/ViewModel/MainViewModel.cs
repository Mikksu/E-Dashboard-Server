using DevExpress.PivotGrid.OLAP.Mdx;
using DevExpress.Xpf.Grid;
using EDashboard.Core;
using EDashboard.OvenMonitoring;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Grpc.Core;
using System;
using System.Threading;
using System.Windows;

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
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            MainCoordinator = new Coordinator();

        }

        #region Properties

        public Coordinator MainCoordinator { get; set; }


        public LotInfo SelectedLot { get; set; }

        #endregion

        #region Methods


        public void StartDemo()
        {
            for (int i = 1; i <= 5; i++)
            {
                MainCoordinator.AddNewOven(Guid.NewGuid().ToString("N"), $"Oven {i}");
                Thread.Sleep(50);
            }

            var r = new Random();

            for (int i = 0; i < 20; i++)
            {
                MainCoordinator.AddLot(MainCoordinator.OvenList[r.Next(0, 4)].HashString, $"Lot {i}" ,25, 30, "testOP");
                Thread.Sleep(500);
            }
        }


        public LotInfo[] CheckOverlyRoasted(string OvenHashString)
        {
            return MainCoordinator.CheckOverRoastLot(OvenHashString);
        }

        #endregion

        #region Commands

        public RelayCommand TerminateBakingManually
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (SelectedLot != null)
                    {
                        try
                        {
                            MainCoordinator.DeleteLot(SelectedLot.Oven.HashString, SelectedLot.LotNum);
                        }
                        catch(RpcException ex)
                        {
                            MessageBox.Show(ex.Status.Detail, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                     }
                    else
                    {
                        MessageBox.Show("请先选择一个Lot再进行删除。", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }

        #endregion

    }
}