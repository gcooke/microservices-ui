using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Bagl.Cib.MIT.Cube;
using Gateway.Web.Database;
using Gateway.Web.Models.Security;

namespace Gateway.Web.Services.Batches.Interrogation
{
    public class PfeInterrogation : BaseInterrogation, IInterrogation
    {
        private readonly IGatewayDatabaseService _gatewayDatabase;
        private readonly IPnRFoDatabaseService _pnRFoDatabase;

        public PfeInterrogation(IGatewayDatabaseService gatewayDatabase, IPnRFoDatabaseService pnRFoDatabase)
        {
            _gatewayDatabase = gatewayDatabase;
            _pnRFoDatabase = pnRFoDatabase;
        }

        public override ReportsModel Run(DateTime valuationDate)
        {
            var report = base.Run(valuationDate);

            //if (!cube.Successfull)
            //{
            //    result.Report.Add(ErrorCube(cube.Message));
            //}
            //else
            //{
            //    var summary = GetSummary(cube.Body);
            //    result.Report.Add(summary);

            //    var missing = GetMissingPfeRows(cube.Body);
            //    result.Report.Add(missing);
            //}
            return report;
        }

        private ICube GetSummary(ICube cube)
        {
            var result = new CubeBuilder()
                .AddColumn("Total Counterparties")
                .AddColumn("Sigma MTM")
                .AddColumn("Total PFE")
                .Build();

            result.SetAttribute("Title", "Summary");
            long totalCounterparties = 0;
            decimal sigmaMtm = 0;
            decimal totalPfe = 0;
            foreach (var row in cube.GetRows())
            {
                var sumMtm = row.GetValue<decimal>("SumMTM");
                var peak = row.GetValue<decimal>("PfePeak");
                if (sumMtm.HasValue && sumMtm.Value != 0)
                {
                    totalCounterparties++;
                    sigmaMtm += sumMtm.Value;
                    if (peak.HasValue)
                        totalPfe += Math.Abs(peak.Value);
                }
            }

            result.AddRow(new object[] { totalCounterparties, sigmaMtm, totalPfe });
            return result;
        }

        private ICube GetMissingPfeRows(ICube cube)
        {
            var result = new CubeBuilder()
                .AddColumn("SDS ID")
                .AddColumn("Counterparty Name")
                .AddColumn("Sigma MTM")
                .Build();

            result.SetAttribute("Title", "Counterparties missing PFE values");
            foreach (var row in cube.GetRows())
            {
                var sdsId = row.GetStringValue("LegalEntityId");
                var name = row.GetStringValue("LegalEntityName");
                var sumMtm = row.GetValue<decimal>("SumMTM");
                var peak = row.GetValue<decimal>("PfePeak");
                if ((sumMtm.HasValue && sumMtm.Value != 0) && !peak.HasValue)
                {
                    result.AddRow(new object[] { sdsId, name, sumMtm.Value });
                }
            }
            return result;
        }

        private ICube ErrorCube(string message)
        {
            var result = new CubeBuilder()
                .AddColumn("Error")
                .Build();

            result.SetAttribute("Title", "Error");
            result.AddRow(new[] { message });
            return result;
        }
    }
}