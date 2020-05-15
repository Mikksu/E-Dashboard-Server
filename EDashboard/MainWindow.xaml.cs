using EDashboard.Services;
using EDashboardService.OvenMonitoring.V1;
using Grpc.Core;
using System;
using System.Diagnostics;
using System.Windows;

namespace EDashboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            string Host = "127.0.0.1";
            int Port = 5500;

            var server = new Server
            {
                Services = { OvenMonitoringService.BindService(new OvenMonitoringServiceImp()) },
                Ports = { new ServerPort(Host, Port, ServerCredentials.Insecure) }
            };

            // Start server
            server.Start();

            Debug.WriteLine("OvenMonitoringService is listening on port " + Port);

            //server.ShutdownAsync().Wait();

        }
    }
}
