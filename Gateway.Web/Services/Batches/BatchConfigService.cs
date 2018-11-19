using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bagl.Cib.MIT.IoC;
using Gateway.Web.Database;
using Gateway.Web.Models.Batches;
using Gateway.Web.Services.Batches.Interfaces;
using Gateway.Web.Services.Batches.Utils;
using Gateway.Web.Services.Schedule.Interfaces;
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
                    .Select(x => new { x.Type, x.ConfigurationId})
                    .Select(x => new BatchConfigModel
                    {
                        ConfigurationId = x.ConfigurationId,
                        Type = x.Type
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

        public IEnumerable<Database.Schedule> GetSchedules()
        {
            using (var db = new GatewayEntities(ConnectionString))
            {
                var items = db.Schedules
                    .Include("RiskBatchSchedule")
                    .Include("RiskBatchSchedule.RiskBatchConfiguration")
                    .Include("RequestConfiguration")
                    .Include("ExecutableConfiguration")
                    .Include("ParentSchedule")
                    .Include("Children")
                    .ToList();
                return items;
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
                var entity = db.RiskBatchConfigurations.SingleOrDefault(x => x.Type.ToLower() == type.ToLower());

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

            return db.RiskBatchConfigurations.Count(x => x.Type == batchConfigModel.Type) == 0;
        }
    }
}