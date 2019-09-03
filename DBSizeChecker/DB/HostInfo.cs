using System.Collections.Generic;

namespace DBSizeChecker.DB
{
    /// <summary>
    /// Represent host information
    /// </summary>
    public class HostInfo
    {
        public HostInfo(string hostId, List<DBSizeModel> dataBases, double totalSpace)
        {
            HostID = hostId;
            DataBases = dataBases;
            TotalSpace = totalSpace;
        }

        public double TotalSpace { get; }

        public List<DBSizeModel> DataBases { get; }
        public string HostID { get; }
    }
}
