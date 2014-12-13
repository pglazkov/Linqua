using System;
using System.Composition;

namespace Framework.MefExtensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class DefaultExportAttribute : ExportAttribute
    {
        public DefaultExportAttribute(Type contractType)
            : base(Constants.DefaultContractNamePrefix, contractType) { }

        public DefaultExportAttribute(string contractName, Type contractType)
            : base(Constants.DefaultContractNamePrefix + contractName, contractType) { }
    }
}
