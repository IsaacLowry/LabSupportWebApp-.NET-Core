using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace LabSupportApp.Models
{
    [BsonIgnoreExtraElements]
    public class QueueObject
    {
        [BsonElement("labName")]
        public string LabName { get; set; }
        [BsonElement("teamsLink")]
        public string TeamsLink { get; set; }
        [BsonElement("code")]
        public int Code { get; set; }
        [BsonElement("user")]
        public string User { get; set; }

        [BsonElement("studentCount")]
        public int StudentCount { get; set; }

        [BsonElement("studentAtHead")]
        public string StudentAtHead { get; set; }

        public QueueObject (string LabName, string TeamsLink, int Code, string User, int StudentCount, string StudentAtHead)
        {
            this.LabName = LabName;
            this.TeamsLink = TeamsLink;
            this.Code = Code;
            this.User = User;
            this.StudentCount = StudentCount;
            this.StudentAtHead = StudentAtHead;
        }

    }
}