using ServiceImport.Framework.CodeDom;

namespace ServiceImport.MSBuild.Model
{
    internal class TypeAccessModifierMapping
    {
        public string TypeName { get; set; }
        public TypeAccessModifier AccessModifier { get; set; }
    }
}
