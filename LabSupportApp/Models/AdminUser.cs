using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LabSupportApp.Models
{
    [BsonIgnoreExtraElements]
    public class AdminUser
    {
        //public ObjectId Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        public AdminUser(string Email)
        {
            this.Email = Email;
        }
    }
}