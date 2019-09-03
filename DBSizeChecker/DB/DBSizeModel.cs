using System;

namespace DBSizeChecker.DB
{

    /// <summary>
    /// Reprsent Database information
    /// </summary>
    public class DBSizeModel
    {
        public DBSizeModel(long sizeOnDisk, string dbName)
        {
            SizeOnDisk = sizeOnDisk;
            DBName = dbName;
        }

        public string DBName { get; }
        public long SizeOnDisk { get; }
        public double SizeOnDiskInGB => Math.Round(Math.Abs(SizeOnDisk / 1024.0 / 1024.0 / 1024.0), 3);
    }
}
