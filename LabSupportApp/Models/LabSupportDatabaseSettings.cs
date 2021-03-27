using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LabSupportApp.Models
{
    public class LabSupportDatabaseSettings : ILabSupportDatabaseSettings
    {
        public string UsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ILabSupportDatabaseSettings
    {

    }
}