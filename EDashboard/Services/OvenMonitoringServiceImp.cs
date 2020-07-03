using CommonServiceLocator;
using EDashboard.Core.Extension;
using EDashboard.ViewModel;
using EDashboardService.OvenMonitoring.V1;
using Grpc.Core;
using System.Threading.Tasks;

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
            var oven = mainVm.MainCoordinator.OvenList.FindByHashstring(ovenHash);
            if (oven == null)
            {
                mainVm.MainCoordinator.AddNewOven(ovenHash, ovenCaption);
            }
            else
                oven.Caption = ovenCaption; // the caption might be updated.

            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> ReportRealtimeTemperature(ReportRealtimeTemperatureRequest request, ServerCallContext context)
        {
            var mainVm = ServiceLocator.Current.GetInstance<MainViewModel>();
            var oven = mainVm.MainCoordinator.OvenList.FindByHashstring(request.OvenHash);
            
            if(oven != null)
            {
                // oven.AddRealtimeTemperaturePoint(request.Temperature);

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
            var mainVm = ServiceLocator.Current.GetInstance<MainViewModel>();
            mainVm.MainCoordinator.AddLot(request.OvenHash, request.LotNum, request.Pcs, request.RoastSec, request.OpName);
            return Task.FromResult(new EmptyResponse());
        }

        public override Task<EmptyResponse> Fetch(FetchRequest request, ServerCallContext context)
        {
            var mainVm = ServiceLocator.Current.GetInstance<MainViewModel>();
            mainVm.MainCoordinator.DeleteLot(request.OvenHash, request.LotNum);
            return Task.FromResult(new EmptyResponse());
        }

        // Check are there overly roasted lots in the specified oven.
        public override Task<CheckOverlyRoastedLotResponse> CheckOverlyRoastedLot(CheckOverlyRoastedLotRequest request, ServerCallContext context)
        {
            var mainVm = ServiceLocator.Current.GetInstance<MainViewModel>();
            var hasOne = mainVm.CheckOverlyRoasted(request.OvenHash);
            return Task.FromResult(new CheckOverlyRoastedLotResponse() { HasOne = hasOne });
        }
    }
}
