using System;
using System.CodeDom.Compiler;
using Microsoft.Build.Framework;

namespace ServiceImport.MSBuild.Factory
{
    internal class CodeGeneratorOptionsFactory
    {
        public CodeGeneratorOptions Create(ITaskItem taskItem)
        {
            if (taskItem == null)
                throw new ArgumentNullException("taskItem");

            var codeGeneratorOptions = new CodeGeneratorOptions
                {
                    BracingStyle = taskItem.ItemSpec
                };

            var blankLinesBetweenMembers = taskItem.GetMetadata("BlankLinesBetweenMembers");
            if (blankLinesBetweenMembers != null)
                codeGeneratorOptions.BlankLinesBetweenMembers = bool.Parse(blankLinesBetweenMembers);

            var elseOnClosing = taskItem.GetMetadata("ElseOnClosing");
            if (elseOnClosing != null)
                codeGeneratorOptions.ElseOnClosing = bool.Parse(elseOnClosing);

            return codeGeneratorOptions;
        }
    }
}
