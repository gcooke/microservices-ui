using System.Threading.Tasks;
using Gateway.Web.Models.Batches;

namespace Gateway.Web.Services.Batches.Interfaces
{
    public interface IBatchParametersService
    {
        Task<BatchSettingsReport> PopulateDifferences(BatchSettingsReport report);
    }
}