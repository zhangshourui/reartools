namespace RearTools
{
    public class AppSettings
    {
        /// <summary>
        /// 代码生成的根目录
        /// </summary>
        public string SchemaBuildDirBase { get; set; }
        public ConnectionItem[] Connections { get; set; }

        public static int DefaultQueueRetryCount
        {
            get { return 5; }
        }

        public class ConnectionItem
        {
            public string Name { get; set; }
            public string ConnectionString { get; set; }
            public string ProviderName { get; set; }
        }


    }
}
