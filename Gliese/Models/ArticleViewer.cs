
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

    [Table("article_viewers")]
    [PrimaryKey(nameof(User))]
    public class ArticleViewerTable
    {
        [Column("user")]
        public string User { get; set; } = "";

        [Column("article")]
        public string Article { get; set; } = "";

        [Column("netaddr")]
        public string NetAddr { get; set; } = "";

        [Column("create_time")]
        public DateTime CreateTime { get; set; } = DateTime.MinValue;

        [Column("update_time")]
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
    }
}
