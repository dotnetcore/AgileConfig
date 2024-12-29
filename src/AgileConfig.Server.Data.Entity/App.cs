﻿using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using AgileConfig.Server.Common;

namespace AgileConfig.Server.Data.Entity
{
    
    public interface IAppModel
    {
        string Id { get; set; }
        
        string Name { get; set; }
        
        string Group { get; set; }
        
        DateTime CreateTime { get; set; }
    }

    
    public enum AppType
    {
        PRIVATE,
        Inheritance,
    }

    [Table(Name = "agc_app")]
    [OraclePrimaryKeyName("agc_app_pk")]
    public class App: IAppModel, IEntity<string>
    {
        [Column(Name= "id" , StringLength = 36)]
        public string Id { get; set; }

        [Column(Name = "name" , StringLength = 50)]
        public string Name { get; set; }

        [Column(Name = "group" , StringLength = 50)]
        public string Group { get; set; }

        [Column(Name = "secret", StringLength = 36)]
        public string Secret { get; set; }

        [Column(Name = "create_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; }

        [Column(Name = "update_time")]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdateTime { get; set; }

        [Column(Name = "enabled")]
        public bool Enabled { get; set; }

        [Column(Name = "type")]
        public AppType Type { get; set; }

        [Column(Name = "app_admin",StringLength = 36)]
        public string AppAdmin { get; set; }
    }
}
