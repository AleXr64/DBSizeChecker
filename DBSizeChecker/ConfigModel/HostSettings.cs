namespace DBSizeChecker.ConfigModel
{
    /// <summary>
    ///     Host configuration
    /// </summary>
    public class HostSettings
    {
        /// <summary>
        ///     Server identificator
        /// </summary>
        public string ServerID { get; set; }

        /// <summary>
        ///     Settings for connection to DB
        /// </summary>
        public string Connection { get; set; }

        /// <summary>
        ///     Total disk space on server
        /// </summary>
        public double DiskSpace { get; set; }
    }
}
