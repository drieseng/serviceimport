using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using BRail.Nis.ServiceImport.Framework.CodeDom;
using BRail.Nis.ServiceImport.Framework.Extension;
using NUnit.Framework;

namespace BRail.Nis.ServiceImport.Framework.Tests.Extension
{
    [TestFixture]
    public class TypeRenameExtensionTests
    {
        [Test]
        public void Apply_ShouldThrowArgumentNullExceptionWhenCodeCompileUnitsIsNull()
        {
            var typeRenames = new Dictionary<string,string>();
            IDataContractGenerationExtension typeRenameExtension = new TypeRenameExtension(typeRenames);
            const CodeCompileUnit codeCompileUnit = null;

            var ane = Assert.Throws<ArgumentNullException>(() => typeRenameExtension.GenerateContract(codeCompileUnit));
            Assert.IsNull(ane.InnerException);
            Assert.AreEqual("codeCompileUnit", ane.ParamName);
        }

        [Test]
        public void Apply_ShouldThrowArgumentNullExceptionWhenTypeRenamesIsNull()
        {
            const Dictionary<string, string> typeRenames = null;
            IDataContractGenerationExtension typeRenameExtension = new TypeRenameExtension(typeRenames);
            var codeCompileUnit = new CodeCompileUnit();

            var ane = Assert.Throws<ArgumentNullException>(() => typeRenameExtension.GenerateContract(codeCompileUnit));
            Assert.IsNull(ane.InnerException);
            Assert.AreEqual("typeRenames", ane.ParamName);
        }

        [Test]
        public void Apply_ShouldMoveTypeToOtherNamespaceWhileRetainingNameWhenOnlyNamespaceOfNewTypeIsDifferentAndNewNamespaceExistsButTypeDoesNotExist_OriginalNamespaceHasTypesAfterMove()
        {
            #region Arrange

            var typeRenames = new Dictionary<string, string>
                {
                    {"System.Net.Byte", "System.Byte"}
                };
            IDataContractGenerationExtension typeRenameExtension = new TypeRenameExtension(typeRenames);
            var codeCompileUnit = new CodeCompileUnit();

            var systemNamespace = new CodeNamespace("System");
            systemNamespace.Types.Add(new CodeTypeDeclaration("DateTime"));
            codeCompileUnit.Namespaces.Add(systemNamespace);

            var systemNetNamespace = new CodeNamespace("System.Net");
            systemNetNamespace.Types.Add(new CodeTypeDeclaration("Byte"));
            systemNetNamespace.Types.Add(new CodeTypeDeclaration("IPAddress"));
            codeCompileUnit.Namespaces.Add(systemNetNamespace);

            #endregion Arrange

            #region Act

            typeRenameExtension.GenerateContract(codeCompileUnit);

            #endregion Act

            #region Assert

            Assert.That(codeCompileUnit.Namespaces.Contains(systemNamespace));
            Assert.That(systemNamespace.Types.Count, Is.EqualTo(2));
            Assert.That(systemNamespace.Types().Any(c => c.Name == "Byte"));
            Assert.That(systemNamespace.Types().Any(c => c.Name == "DateTime"));

            Assert.That(codeCompileUnit.Namespaces.Contains(systemNetNamespace));
            Assert.That(systemNetNamespace.Types.Count, Is.EqualTo(1));
            Assert.That(systemNetNamespace.Types().Any(c => c.Name == "IPAddress"));

            #endregion Assert
        }

        [Test]
        public void Apply_ShouldMoveTypeToOtherNamespaceWhileRetainingNameWhenOnlyNamespaceOfNewTypeIsDifferentAndNewNamespaceExistsButTypeDoesNotExist_OriginalNamespaceHasNoTypesAfterMove()
        {
            #region Arrange

            var typeRenames = new Dictionary<string, string>
                {
                    {"System.Net.Byte", "System.Byte"}
                };
            IDataContractGenerationExtension typeRenameExtension = new TypeRenameExtension(typeRenames);
            var codeCompileUnit = new CodeCompileUnit();

            var systemNamespace = new CodeNamespace("System");
            systemNamespace.Types.Add(new CodeTypeDeclaration("DateTime"));
            codeCompileUnit.Namespaces.Add(systemNamespace);

            var systemNetNamespace = new CodeNamespace("System.Net");
            systemNetNamespace.Types.Add(new CodeTypeDeclaration("Byte"));
            codeCompileUnit.Namespaces.Add(systemNetNamespace);

            #endregion Arrange

            #region Act

            typeRenameExtension.GenerateContract(codeCompileUnit);

            #endregion Act

            #region Assert

            Assert.That(codeCompileUnit.Namespaces.Contains(systemNamespace));
            Assert.That(systemNamespace.Types.Count, Is.EqualTo(2));
            Assert.That(systemNamespace.Types().Any(c => c.Name == "Byte"));
            Assert.That(systemNamespace.Types().Any(c => c.Name == "DateTime"));

            Assert.IsFalse(codeCompileUnit.Namespaces.Contains(systemNetNamespace));
            Assert.That(systemNetNamespace.Types.Count, Is.EqualTo(0));

            #endregion Assert
        }


        [Test]
        public void Apply_ShouldRenameTypeWhileRetainingNamespaceWhenOnlyTypeNameOfNewTypeIsDifferent()
        {
            #region Arrange

            var typeRenames = new Dictionary<string, string>
                {
                    {"System.Int32", "System.Byte"}
                };
            IDataContractGenerationExtension typeRenameExtension = new TypeRenameExtension(typeRenames);
            var codeCompileUnit = new CodeCompileUnit();

            var systemNamespace = new CodeNamespace("System");
            systemNamespace.Types.Add(new CodeTypeDeclaration("DateTime"));
            systemNamespace.Types.Add(new CodeTypeDeclaration("Int32"));
            codeCompileUnit.Namespaces.Add(systemNamespace);

            #endregion Arrange

            #region Act

            typeRenameExtension.GenerateContract(codeCompileUnit);

            #endregion Act

            #region Assert

            Assert.That(codeCompileUnit.Namespaces.Contains(systemNamespace));
            Assert.That(systemNamespace.Types.Count, Is.EqualTo(2));
            Assert.That(systemNamespace.Types().Any(c => c.Name == "Byte"));
            Assert.That(systemNamespace.Types().Any(c => c.Name == "DateTime"));

            #endregion Assert
        }
    }
}
