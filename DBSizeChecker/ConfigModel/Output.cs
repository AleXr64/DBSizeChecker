namespace DBSizeChecker.ConfigModel
{
    public class GoogleSettings
    {
        /// <summary>
        ///     Path to credentials file obtainet via Google Docs API
        /// </summary>
        public string PathToCredentials { get; set; }

        /// <summary>
        ///     Which sheet we can use?
        /// </summary>
        public string SheetName { get; set; }
    }
}
