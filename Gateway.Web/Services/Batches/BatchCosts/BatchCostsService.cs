﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Models.Batches;
using Gateway.Web.Utils;

namespace Gateway.Web.Services.Batches.BatchCosts
{
    public class BatchCostsService : IBatchCostsService
    {
        private readonly IGateway _gateway;
        private readonly ISystemInformation _systemInformation;
        private readonly ILogger _logger;
        private readonly string _maintenanceControllerName = "Maintenance";

        public string[] MonthNames { get; } = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public BatchCostsService(
                                IGateway gateway,
                                ISystemInformation information,
                                ILoggingService loggingService)
        {
            _gateway = gateway;
            _systemInformation = information;
            _logger = loggingService.GetLogger(this);

            //_gateway.SetGatewayUrlForService(_maintenanceControllerName, "http://localhost:7000");
        }

        public ICube GetBatchCosts()
        {
            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var query = $"BatchReporting/Batch/Costs/{currentDate}";
            _logger.Info($"Date passed as '{currentDate}'.");

            _logger.Info($"Starting request to {query}.");
            var response = _gateway.Get<ICube>(_maintenanceControllerName, query).GetAwaiter().GetResult();
            _logger.Info($"{(response.Successfull ? "(Successful)" : "(Unsuccessful)")} Response received from '{query}'.");

            if (!response.Successfull)
                throw new Exception($"Error retrieving batch costs:{response.Message}");

            return response.Body;
        }

        public List<CostGroupMonthlyBatchCost> GetBatchMonthlyCosts(ICube costsCube)
        {
            var monthlyCostGroups = new List<CostGroupMonthlyBatchCost>();

            foreach (var costItem in costsCube.GetRows())
            {
                var monthlyCostGroup = monthlyCostGroups.FirstOrDefault(g =>
                                                                        g.CostGroup.Equals(costItem.GetStringValue("CostGroup")) &&
                                                                        g.BatchType.Equals(costItem.GetStringValue("BatchType")) &&
                                                                        g.CostType.Equals(costItem.GetStringValue("CostType")));

                if (monthlyCostGroup == null)
                {
                    monthlyCostGroup = new CostGroupMonthlyBatchCost
                    {
                        CostGroup = costItem.GetStringValue("CostGroup"),
                        BatchType = costItem.GetStringValue("BatchType"),
                        CostType = costItem.GetStringValue("CostType")
                    };

                    monthlyCostGroups.Add(monthlyCostGroup);
                }

                monthlyCostGroup.GetType()
                    .GetProperty(costItem.GetStringValue("Month"))
                    .SetValue(monthlyCostGroup, costItem.GetStringValue("TotalCost").ToCultureAwareDecimal());
            }

            SetAnnualTotalEstimates(monthlyCostGroups);

            return monthlyCostGroups;
        }

        private void SetAnnualTotalEstimates(List<CostGroupMonthlyBatchCost> monthlyCostGroups)
        {
            foreach (var item in monthlyCostGroups)
            {
                decimal runningTotal = 0;

                var monthlyValueProperties = item.GetType().GetProperties().Where(p => MonthNames.Any(m => m.Equals(p.Name))).ToList();

                foreach (var property in monthlyValueProperties)
                {
                    var value = property.GetValue(item);

                    runningTotal += (value==null) ? 0 : Convert.ToDecimal(value);
                }

                item.EstimatedAnnualTotal = Math.Round(runningTotal / DateTime.Now.Month * 12, 2, MidpointRounding.AwayFromZero);
            }
        }
    }
}