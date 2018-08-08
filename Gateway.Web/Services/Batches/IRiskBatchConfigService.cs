using System.Collections.Generic;
using Gateway.Web.Models.Batches;

namespace Gateway.Web.Services.Batches
{
    public interface IRiskBatchConfigService
    {
        BatchConfigModel CreateConfiguration(BatchConfigModel batchConfigModel);
        BatchConfigModel UpdateConfiguration(BatchConfigModel batchConfigModel);
        BatchConfigModel DeleteConfiguration(long id);
        BatchConfigModel GetConfiguration(long id);
        BatchConfigModel GetConfiguration(string type);
        BatchConfigList GetConfigurations(int offset, int pageSize, string searchTerm);
        IList<string> GetConfigurationTypes();
    }
}
