
using Enferno.Public.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Enferno.Public.Test
{
    [TestClass]
    public class UtilsTest
    {
        [TestMethod]
        public void DistanceFromSkanstullToOfficeTest()
        {
            //59.30849,18.06392,59.32457,18.06776
            var distance = DistanceCalculator.GetKilometersBetween(new GeoPosition(18.06392, 59.30849), new GeoPosition(18.06776, 59.32457));

            Assert.IsTrue(distance < 2000);
        }

        [TestMethod]
        [Description("Tests an invalid address")]
        public void EmailValidationTest1()
        {
            const string email = "men.men@live,se";
            Assert.IsFalse(Validation.IsValidEmail(email));
        }
    }
}
