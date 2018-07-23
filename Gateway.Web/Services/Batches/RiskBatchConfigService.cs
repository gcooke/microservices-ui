using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gateway.Web.Database;
using Gateway.Web.Models.Batches;

namespace Gateway.Web.Services.Batches
{
    public class RiskBatchConfigService : IRiskBatchConfigService
    {
        public BatchConfigModel CreateConfiguration(BatchConfigModel batchConfigModel)
        {
            using (var db = new GatewayEntities())
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
            using (var db = new GatewayEntities())
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
            using (var db = new GatewayEntities())
            {
                var entity = db.RiskBatchConfigurations.SingleOrDefault(x => x.ConfigurationId == id);

                if (entity == null)
                {
                    throw new Exception($"Unable to find configuration with ID {id}");
                }

                db.RiskBatchConfigurations.Remove(entity);
                db.SaveChanges();
                return entity.ToModel();
            }
        }

        public BatchConfigModel GetConfiguration(long id)
        {
            using (var db = new GatewayEntities())
            {
                var entity = db.RiskBatchConfigurations.SingleOrDefault(x => x.ConfigurationId == id);

                if (entity == null)
                {
                    throw new Exception($"Unable to find configuration with ID {id}");
                }

                db.RiskBatchConfigurations.Remove(entity);
                return entity.ToModel();
            }
        }

        public BatchConfigList GetConfigurations(int offset, int pageSize, string searchTerm)
        {
            using (var db = new GatewayEntities())
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

        public IList<string> GetConfigurationTypes()
        {
            using (var db = new GatewayEntities())
            {
                return db.RiskBatchConfigurations
                    .Select(x => x.Type)
                    .ToList();
            }
        }

        public BatchConfigModel GetConfiguration(string type)
        {
            using (var db = new GatewayEntities())
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
            return x => x.MarketDataMapName.ToLower().Contains(term) ||
                        x.OutputTag.ToLower().Contains(term) ||
                        x.OutputType.ToLower().Contains(term) ||
                        x.TradeSourceType.ToLower().Contains(term) ||
                        x.Type.ToLower().Contains(term);
        }

        private bool IsUniqueConfiguration(GatewayEntities db, BatchConfigModel batchConfigModel)
        {
            return db.RiskBatchConfigurations.Count(x => x.Type == batchConfigModel.Type) == 0;
        }
    }
}