using CommonServiceLocator;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using EDashboard.Core.Extension;
using EDashboard.ViewModel;
using EDashboardService.OvenMonitoring.V1;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EDashboard.Services
{
    public class OvenMonitoringServiceImp : OvenMonitoringService.OvenMonitoringServiceBase
    {
        public override Task<EmptyResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            var ovenHash = request.OvenHash;
            var ovenCaption = request.Caption;

            // add new oven to the list. 
            var mainVm = ServiceLocator.Current.GetInstance<MainViewModel>();
            var oven = mainVm.OvenList.FindByHashstring(ovenHash);
            if (oven == null)
            {
                mainVm.AddNewOven(ovenHash, ovenCaption);
            }
            else
                oven.Caption = ovenCaption; // the caption might be updated.

            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> ReportRealtimeTemperature(ReportRealtimeTemperatureRequest request, ServerCallContext context)
        {
            var mainVm = ServiceLocator.Current.GetInstance<MainViewModel>();
            var oven = mainVm.OvenList.FindByHashstring(request.OvenHash);
            
            if(oven != null)
            {
                oven.AddRealtimeTemperaturePoint(request.Temperature);
                return Task.FromResult(new EmptyResponse());
            }
            else
            {
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument, 
                        $"unable to find the oven with hashstring {request.OvenHash}."));
            }
        }

        public override Task<EmptyResponse> Feed(FeedRequest request, ServerCallContext context)
        {
            return base.Feed(request, context);
        }

        public override Task<EmptyResponse> Fetch(FetchRequest request, ServerCallContext context)
        {
            return base.Fetch(request, context);
        }
    }
}
