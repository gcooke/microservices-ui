using System.IO;
using System.Linq;
using Gateway.Web.Models.User;
using Gateway.Web.Utils;
using NUnit.Framework;

namespace Gateway.Web.Tests.Models.Security
{
    [TestFixture]
    public class UserSecurityTest
    {
        private T GetPayload<T>(string resourceName)
        {
            var assembly = GetType().Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))

            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Deserialize<T>();
            }
        }

        [Test]
        public void Can_load_portfolios_model()
        {
            Assert.DoesNotThrow(
                () =>
                {
                    var result = GetPayload<PortfoliosModel>("Gateway.Web.Tests.Resources.UserPortfolioPayload.xml");
                });
        }

        [Test]
        public void Can_load_portfolio_load_correctly()
        {
            var result = GetPayload<PortfoliosModel>("Gateway.Web.Tests.Resources.UserPortfolioPayload.xml");

            var portfolio = result.Portfolios.First();

            Assert.AreEqual(portfolio.Name, "9803AFS OUTRIGHT", "Does not load 'Name' of portfolio correctly.");

            Assert.AreEqual(portfolio.Id, 1, "Does not load 'Id' of portfolios correctly.");

            Assert.AreEqual(portfolio.Level, 12, "Does not load 'Level' of portfolios correctly.");
        }

        [Test]
        public void Test_portfolios_load_correctly()
        {
            var result = GetPayload<PortfoliosModel>("Gateway.Web.Tests.Resources.UserPortfolioPayload.xml");

            Assert.AreEqual(result.Portfolios.Count, 2, "Does not load list of portfolios correctly.");

            Assert.AreEqual(result.InheritedPortfolios.Count, 1, "Does not load list of inherited portfolios correctly.");
        }

        [Test]
        public void Can_load_users_model()
        {
            Assert.DoesNotThrow(() =>
              {
                  var result = GetPayload<UserModel>("Gateway.Web.Tests.Resources.UserPayload.xml");
              });
        }

        [Test]
        public void Can_load_user_correctly()
        {
            var user = GetPayload<UserModel>("Gateway.Web.Tests.Resources.UserPayload.xml");

            Assert.IsNotEmpty(user.Domain, "Does not load 'Domain' of User correctly.");

            Assert.AreNotEqual(user.Id, 0, "Does not load 'Id' of User correctly.");

            Assert.IsNotEmpty(user.Login, "Does not load 'Login' of User correctly.");

            Assert.IsNotEmpty(user.FullName, "Does not load 'FullName' of User correctly.");
        }

        [Test]
        public void Can_load_user_groups_correctly()
        {
            var user = GetPayload<UserModel>("Gateway.Web.Tests.Resources.UserPayload.xml");

            Assert.AreEqual(user.UserGroups.Count, 4, "Does not load list of groups correctly.");

            var group = user.UserGroups.First();

            Assert.AreNotEqual(group.Id, 0, "Does not load 'Id' of User correctly.");

            Assert.IsNotEmpty(group.Name, "Does not load 'Login' of User correctly.");
        }

        [Test]
        public void Can_load_sites_model()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = GetPayload<SitesModel>("Gateway.Web.Tests.Resources.UserSitePayload.xml");
            });
        }

        [Test]
        public void Can_load_sites_correctly()
        {
            var result = GetPayload<SitesModel>("Gateway.Web.Tests.Resources.UserSitePayload.xml");

            Assert.IsNotEmpty(result.Login, "Does not load 'Domain' of SitesModel correctly.");

            Assert.AreNotEqual(result.Id, 0, "Does not load 'Id' of SitesModel correctly.");

            Assert.AreEqual(result.Sites.Count, 1, "Does not load list of sites correctly.");

            Assert.AreEqual(result.InheritedSites.Count, 2, "Does not load list of inherited correctly.");
        }

        [Test]
        public void Can_load_site_correctly()
        {
            var result = GetPayload<SitesModel>("Gateway.Web.Tests.Resources.UserSitePayload.xml");

            var site = result.Sites.First();

            Assert.AreNotEqual(site.Id, 0, "Does not load 'Id' of SiteModel correctly.");

            Assert.IsNotEmpty(site.Name, "Does not load 'Domain' of SiteModel correctly.");
        }

        [Test]
        public void Can_load_addIns_model()
        {
            Assert.DoesNotThrow(() =>
            {
                var result = GetPayload<AddInsModel>("Gateway.Web.Tests.Resources.UserAddInVersionPayload.xml");
            });
        }

        [Test]
        public void Can_load_addIns_correctly()
        {
            var result = GetPayload<AddInsModel>("Gateway.Web.Tests.Resources.UserAddInVersionPayload.xml");

            Assert.IsNotEmpty(result.Login, "Does not load 'Domain' of SitesModel correctly.");

            Assert.AreNotEqual(result.Id, 0, "Does not load 'Id' of SitesModel correctly.");

            Assert.AreEqual(result.ExcelAddInVersions.Count, 1, "Does not load list of sites correctly.");

            Assert.AreEqual(result.GroupExcelAddInVersions.Count, 1, "Does not load list of inherited correctly.");
        }

        [Test]
        public void Can_load_addIn_correctly()
        {
            var result = GetPayload<AddInsModel>("Gateway.Web.Tests.Resources.UserAddInVersionPayload.xml");

            var addInVersion = result.ExcelAddInVersions.First();

            Assert.AreNotEqual(addInVersion.ExcelAddInVersionId, 0, "Does not load 'ExcelAddInVersionId' of UserAddInVersion correctly.");

            Assert.IsNotEmpty(addInVersion.Name, "Does not load 'Name' of UserAddInVersion correctly.");

            Assert.IsNotEmpty(addInVersion.FriendlyName, "Does not load 'FriendlyName' of UserAddInVersion correctly.");

            Assert.IsNotEmpty(addInVersion.Version, "Does not load 'Version' of UserAddInVersion correctly.");
        }
    }
}