using NUnit.Framework;

namespace BRail.Nis.ServiceImport.Framework.Tests.Extension
{
    [TestFixture]
    public class PascalCaseFieldNamesExtensionTests
    {
        [Test]
        public void PrivateInstanceFieldsThatDoNotHavePascalCaseFieldNamesAreRenamed()
        {
        }

        [Test]
        public void ReferencesToPrivateInstanceFieldsThatDoNotHavePascalCaseFieldNamesAreUpdated()
        {
        }

        [Test]
        public void PublicInstanceFieldsThatDoNotHavePascalCaseFieldNamesAreNotRenamed()
        {
        }
    }
}
