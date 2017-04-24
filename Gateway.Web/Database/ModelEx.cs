using System.Linq;
using Gateway.Web.Models;
using Microsoft.Ajax.Utilities;

namespace Gateway.Web.Database
{
    public static class ModelEx
    {
        public static ControllerModel ToModel(this Controller controller, ResponseStats stats)
        {
            var result = new ControllerModel();

            result.Name = controller.Name;
            result.TotalCalls = stats.GetTotalCalls(controller.Name);
            result.TotalErrors = stats.GetTotalErrors(controller.Name);
            result.AverageResponse = stats.GetAverageResponse(controller.Name);
            foreach (var group in controller.Versions.GroupBy(v => v.Status.Name))
            {
                result.VersionSummary.Add(new InfoItem(group.Key, group.Count().ToString()));
            }
            return result;
        }


        public static Models.Version ToModel(this Version version)
        {
            var result = new Models.Version(version.Id, version.Version1, version.Alias, version.Status.Name);
            return result;
        }
    }

}
