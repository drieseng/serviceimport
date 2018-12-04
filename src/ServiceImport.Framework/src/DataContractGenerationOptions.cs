using System;

namespace ServiceImport.Framework
{
    [Flags]
    public enum DataContractGenerationOptions
    {
        /// <summary>
        /// Default data contract generation.
        /// </summary>
        Default = 0,
        /// <summary>
        /// Generate an optional element as having a nullable type.
        /// </summary>
        /// <remarks>
        /// This only applies to structs and enums.
        /// </remarks>
        OptionalElementAsNullable = 1
    }
}
