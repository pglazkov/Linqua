using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Linqua.Service
{
    public class LinquaDbConfiguration : DbConfiguration
    {
        public LinquaDbConfiguration()
        {
            // See https://msdn.microsoft.com/en-us/data/dn456835.aspx?f=255&MSPPError=-2147217396
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}