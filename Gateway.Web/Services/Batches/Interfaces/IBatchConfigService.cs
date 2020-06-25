using System.Collections.Generic;
using Gateway.Web.Database;
using Gateway.Web.Models.Batches;

namespace Gateway.Web.Services.Batches.Interfaces
{
    public interface IBatchConfigService
    {
        BatchConfigModel CreateConfiguration(BatchConfigModel batchConfigModel);
        BatchConfigModel UpdateConfiguration(BatchConfigModel batchConfigModel);
        BatchConfigModel DeleteConfiguration(long id);
        BatchConfigModel GetConfiguration(long id);
        BatchConfigModel GetConfiguration(string type);
        BatchConfigList GetConfigurations(int offset, int pageSize, string searchTerm);
        IList<BatchConfigModel> GetConfigurationTypes();
        IList<BatchConfigModel> GetConfigurationTypes(IEnumerable<long> configurationIdList);
        IEnumerable<ScheduleGroup> GetScheduleGroups();
        BatchSettingsReport GetSettingsReport();
    }
}
