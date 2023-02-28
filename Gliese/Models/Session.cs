
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Markdig;
using Gliese.Utils;

namespace Gliese.Models
{

    [Table("sessions")]
    [PrimaryKey(nameof(Pk))]
    public class SessionTable
    {
        [Column("pk")]
        public string Pk { get; set; } = "";

        [Column("content")]
        public string Content { get; set; } = "";

        [Column("user")]
        public string User { get; set; } = ""; 
        
        [Column("type")]
        public string Type { get; set; } = ""; 

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.MinValue;

        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue; 
    }
}
