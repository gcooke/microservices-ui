using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using Bagl.Cib.MIT.IoC;
using CronExpressionDescriptor;
using Gateway.Web.Database;
using Gateway.Web.Models.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Batches.Utils;
using Gateway.Web.Services.Schedule.Interfaces;
using Gateway.Web.Utils;
using ScheduleGroup = Gateway.Web.Database.ScheduleGroup;

namespace Gateway.Web.Services.Batches
{
    public class BatchConfigService : IBatchConfigService
    {
        private readonly IScheduleDataService _scheduleDataService;
        public string ConnectionString;

        public BatchConfigService(IScheduleDataService scheduleDataService, ISystemInformation systemInformation)
        {
            _scheduleDataService = scheduleDataService;
            ConnectionString = systemInformation.GetConnectionString("GatewayDatabase", "Database.PnRFO_Gateway");
        }

        public BatchConfigModel CreateConfiguration(BatchConfigModel batchConfigModel)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                if (!IsUniqueConfiguration(db, batchConfigModel))
                {
                    throw new Exception($"A configuration of type {batchConfigModel.Type} already exists.");
                }

                var entity = batchConfigModel.ToEntity();
                var savedEntity = db.RiskBatchConfigurations.Add(entity);
                db.SaveChanges();
                return savedEntity.ToModel();
            }
        }

        public BatchConfigModel UpdateConfiguration(BatchConfigModel batchConfigModel)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                if (!IsUniqueConfiguration(db, batchConfigModel))
                {
                    throw new Exception($"A configuration of type {batchConfigModel.Type} already exists.");
                }

                var entity = db.RiskBatchConfigurations.SingleOrDefault(x => x.ConfigurationId == batchConfigModel.ConfigurationId);

                if (entity == null)
                {
                    throw new Exception($"Unable to find configuration with ID {batchConfigModel.ConfigurationId}");
                }

                entity.UpdateEntity(batchConfigModel);
                db.SaveChanges();
                return entity.ToModel();
            }
        }

        public BatchConfigModel DeleteConfiguration(long id)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var entity = db.RiskBatchConfigurations.SingleOrDefault(x => x.ConfigurationId == id);

                if (entity == null)
                {
                    throw new Exception($"Unable to find configuration with ID {id}");
                }

                _scheduleDataService.DeleteForConfiguration(id, db, false);

                var model = entity.ToModel();
                db.RiskBatchConfigurations.Remove(entity);
                db.SaveChanges();
                return model;
            }
        }

        public BatchConfigModel GetConfiguration(long id)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var entity = db.RiskBatchConfigurations.SingleOrDefault(x => x.ConfigurationId == id);

                if (entity == null)
                {
                    throw new Exception($"Unable to find configuration with ID {id}");
                }

                return entity.ToModel();
            }
        }

        public BatchConfigList GetConfigurations(int offset, int pageSize, string searchTerm)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var totalItems = db.RiskBatchConfigurations.Count();
                var entities = db.RiskBatchConfigurations
                    .Where(GetSearchCriteria(searchTerm))
                    .OrderBy(x => x.ConfigurationId)
                    .Skip(offset)
                    .Take(pageSize)
                    .ToList()
                    .Select(x => x.ToModel())
                    .ToList();

                return new BatchConfigList
                {
                    BatchConfigModels = entities,
                    TotalItems = totalItems,
                    Offset = offset,
                    PageSize = pageSize,
                    SearchTerm = searchTerm
                };
            }
        }

        public IList<BatchConfigModel> GetConfigurationTypes()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                return db.RiskBatchConfigurations
                    .Select(x => new { x.Type, x.ConfigurationId, x.OutputTag })
                    .Select(x => new BatchConfigModel
                    {
                        ConfigurationId = x.ConfigurationId,
                        Type = x.Type,
                        OutputTag = x.OutputTag
                    })
                    .ToList();
            }
        }

        public IEnumerable<ScheduleGroup> GetScheduleGroups()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var items = db.ScheduleGroups
                    .ToList();
                return items;
            }
        }

        public BatchSettingsReport GetSettingsReport()
        {
            var report = new BatchSettingsReport();
            using (var db = new GatewayEntities(ConnectionString))
            {
                var rawData = db.spBatchOverridesReport();

                foreach (var row in rawData)
                {
                    var item = new BatchItem(row.Name);
                    if (!string.IsNullOrEmpty(row.Schedule))
                        item.Schedule = ExpressionDescriptor.GetDescription(row.Schedule, new Options() { Use24HourTimeFormat = true });

                    item.Echo = ExtractEcho(db, row.EchoId);
                    report.Batches.Add(item);
                }
            }
            return report;
        }

        private EchoCreationDto ExtractEcho(GatewayEntities db, Guid? echoId)
        {
            try
            {
                var records = db.Payloads.Where(p => p.CorrelationId == echoId).ToList();
                var record = records.FirstOrDefault(r => r.Direction == "Request");
                if (record == null) throw new InvalidOperationException("No echo found");

                var data = LegacyCompession.DecodeObject(record.Data, "String");
                var element = XElement.Parse(data);
                var echo = element.Deserialize<EchoCreationDto>();
                return echo;
            }
            catch (Exception ex)
            {
                return new EchoCreationDto()
                {
                    ApplicationName = "Could not compare parameters:" + ex.Message
                };
            }
        }

        public IList<BatchConfigModel> GetConfigurationTypes(IEnumerable<long> configurationIdList)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                return db.RiskBatchConfigurations
                    .Where(x => configurationIdList.Contains(x.ConfigurationId))
                    .Select(x => new { x.Type, x.ConfigurationId })
                    .Select(x => new BatchConfigModel
                    {
                        ConfigurationId = x.ConfigurationId,
                        Type = x.Type
                    })
                    .ToList();
            }
        }

        public BatchConfigModel GetConfiguration(string type)
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var entity = db.RiskBatchConfigurations.FirstOrDefault(x => x.Type.ToLower() == type.ToLower());

                if (entity == null)
                {
                    throw new Exception($"Unable to find configuration with type {type}");
                }

                return entity.ToModel();
            }
        }

        private Expression<Func<RiskBatchConfiguration, bool>> GetSearchCriteria(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return x => true;

            var term = s.ToLower();
            return x => x.OutputTag.ToLower().Contains(term) ||
                        x.OutputType.ToLower().Contains(term) ||
                        x.Type.ToLower().Contains(term);
        }

        private bool IsUniqueConfiguration(GatewayEntities db, BatchConfigModel batchConfigModel)
        {
            if (batchConfigModel.IsUpdating)
            {
                var existingItem = db.RiskBatchConfigurations.SingleOrDefault(x => x.ConfigurationId == batchConfigModel.ConfigurationId);
                if (existingItem != null && existingItem.Type == batchConfigModel.Type)
                {
                    return true;
                }

                if (existingItem != null && (existingItem.Type != batchConfigModel.Type))
                {
                    return db.RiskBatchConfigurations.Count(x => x.Type == batchConfigModel.Type) == 0;
                }
            }

            return db.RiskBatchConfigurations.Count(x => x.Type == batchConfigModel.Type && x.OutputTag == batchConfigModel.OutputTag) == 0;
        }
    }
}