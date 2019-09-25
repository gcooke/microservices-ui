using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace Gateway.Web.Tests
{
    [SetUpFixture]
    public class CultureSet
    {
        [SetUp]
        public void Init()
        {
            SetCulture("en-GB");
        }

        private static void SetCulture(string cultureCode)
        {
            CultureInfo culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

    }
}