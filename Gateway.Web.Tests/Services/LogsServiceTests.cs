using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bagl.Cib.MIT.IO;
using Bagl.Cib.MIT.IO.Mock;
using Bagl.Cib.MIT.Logging.Mock;
using Bagl.Cib.MIT.Redis;
using Gateway.Web.Database;
using Gateway.Web.Services;
using Moq;
using NUnit.Framework;

namespace Gateway.Web.Tests.Services
{
    [TestFixture]
    public class LogsServiceTests
    {
        private Mock<IGatewayDatabaseService> _database;
        private Mock<IRedisConnectionProvider> _redisConnectionProvider;
        private IFileService _fileService;

        [SetUp]
        public void Setup()
        {
            _database = new Mock<IGatewayDatabaseService>();
            _redisConnectionProvider = new Mock<IRedisConnectionProvider>();

            _fileService = new MockFileService();
            _fileService.WriteAllText(@"\\server\data\LogFiles\controller\2019-03-22T01_26_34_riskbatch_1.2.946_38768.log21-Mar-2019.log", "");
            _fileService.WriteAllText(@"\\server\data\LogFiles\controller\2019-03-22T01_26_34_riskbatch_1.2.946_38768.log.2", "");
            _fileService.WriteAllText(@"\\server\data\LogFiles\controller\2019-03-22T01_26_34_riskbatch_1.2.946_38768.log", "");
            _fileService.WriteAllText(@"\\server\data\LogFiles\controller\2019-03-22T01_26_34_riskbatch_1.2.946_38768.log.1", "");
            _fileService.WriteAllText(@"\\server\data\LogFiles\controller\2019-03-22T12_43_44_riskbatch_1.2.946_22824.log", "");
        }

        [Test]
        public void Can_construct_service()
        {
            Assert.DoesNotThrow(() => GetSUT());
        }

        //[Test]
        //public void Can_retrieve_log_filenames()
        //{
        //    var sut = GetSUT();
        //    var location = new LogsService.LogLocation("server", "controller", "38768");

        //    var result = sut.GetRelevantControllerLogFileNames(location).ToArray();

        //    Assert.AreEqual(4, result.Length);
        //}
        
        private ILogsService GetSUT()
        {
            return new LogsService(new MockLoggingService(), _database.Object, _redisConnectionProvider.Object, _fileService);
        }
    }
}
