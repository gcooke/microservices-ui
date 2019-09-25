using System;
using Bagl.Cib.MIT.IoC;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Gateway.Web.Services.Batches.BatchCosts;
using Moq;
using NUnit.Framework;
using System.IO;
using Bagl.Cib.MIT.Cube;
using Bagl.Cib.MIT.Cube.Impl;

namespace Gateway.Web.Tests.Services.Batches.BatchCosts
{
    [TestFixture]
    public class BatchCostsServiceTests
    {
        private IBatchCostsService _batchCostsService;
        private Mock<IGateway> _gateway;
        private Mock<ISystemInformation> _systemInformation;
        private Mock<ILoggingService> _loggingService;
        private ICube _cube;

        [SetUp]
        public void Setup()
        {
            _gateway = new Mock<IGateway>();
            _systemInformation= new Mock<ISystemInformation>();
            _loggingService = new Mock<ILoggingService>();
            _batchCostsService = new BatchCostsService(_gateway.Object, _systemInformation.Object, _loggingService.Object);

            var builder = new CubeBuilder()
                .AddColumn("CostGroup", ColumnType.String)
                .AddColumn("BatchType", ColumnType.String)
                .AddColumn("CostType", ColumnType.String)
                .AddColumn("Month", ColumnType.String)
                .AddColumn("TotalCost", ColumnType.Decimal);

            _cube = builder.Build();

            var inputString = GetEmbeddedContent("CostGroupMonthlyCostBatches.csv");
            var rows = inputString.Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var row in rows)
            {
                var rowParts = row.Split(',');
                _cube.AddRow(new object[] { rowParts[0], rowParts[1], rowParts[2], rowParts[3], rowParts[4] });
            }
        }

        [Test]
        public void Gets_Correct_Number_Of_Batch_Monthly_Costs()
        {
            var batchMonthlyCosts = _batchCostsService.GetBatchMonthlyCosts(_cube);
            Assert.AreEqual(54, batchMonthlyCosts.Count);
        }

        private string GetEmbeddedContent(string content)
        {
            var assembly = this.GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream(this.GetType(), content))
            {
                using (var reader = new StreamReader(stream))
                    return reader.ReadToEnd();
            }
        }
    }
}
