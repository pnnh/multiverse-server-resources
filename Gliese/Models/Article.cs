
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

    [Table("articles")]
    [PrimaryKey(nameof(Pk))]
    public class ArticleTable
    {
        [Column("pk")]
        public string Pk { get; set; } = "";

        [Column("title")]
        public string Title { get; set; } = "";

        [Column("header")]
        public string Header { get; set; } = "";

        [Column("body")]
        public string Body { get; set; } = "";

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.MinValue;

        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;

        [Column("creator")]
        public string Creator { get; set; } = "";

        [Column("keywords")]
        public string? Keywords { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; } = ""; 

        [Column("status")]
        public int Status { get; set; } = 0; 
        
        [Column("cover")]
        public string? Cover { get; set; } = "";
    }
}
