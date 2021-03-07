using System;
using System.Linq;
using FileSorter.FileSystem;
using Microsoft.Extensions.Configuration;

namespace FileSorter.Configuration
{
    public class Configuration
    {
        private readonly IConfigurationRoot _config;
        
        private static Configuration _instance;
        public static Configuration Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Configuration();

                return _instance;
            }
        }
        
        public Configuration()
        {
            this._config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false, true)
                .Build();
        }

        public TempFolderCollection TempFolders
        {
            get
            {
                var sorting = _config.GetSection("Sorting").Get<SortingSettings>();
                return new TempFolderCollection(sorting.TempFolders.Select(t => new TempFolder(t)));
            }
        }
    }
}