using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LabSupportApp.Models
{
    [BsonIgnoreExtraElements]
    public class StudentUser
    {
        [BsonElement("qcode")]
        public int Qcode { get; set; }
        [BsonElement("timeEntered")]
        public DateTime TimeEntered { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("elapsedTime")]
        public int ElapsedTime { get; set; }

        public StudentUser(int Qcode, DateTime TimeEntered, string Name, string Description, int ElapsedTime)
        {
            this.Qcode = Qcode;
            this.TimeEntered = TimeEntered;
            this.Name = Name;
            this.Description = Description;
            this.ElapsedTime = ElapsedTime;

        }
    }
}