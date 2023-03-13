
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

    [Table("resources")]
    [PrimaryKey(nameof(Pk))]
    public class ResourceTable
    {
        [Column("pk")]
        public string Pk { get; set; } = "";

        [Column("createat")]
        public DateTime CreateAt { get; set; } = DateTime.MinValue;

        [Column("updateat")]
        public DateTime UpdateAt { get; set; } = DateTime.MinValue;

        [Column("title")]
        public string Title { get; set; } = "";

        [Column("header")]
        public string Header { get; set; } = "";

        [Column("body")]
        public string Body { get; set; } = "";

        [Column("creator")]
        public string Creator { get; set; } = "";

        [Column("tags")]
        public string? Tags { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; } = "";

        [Column("status")]
        public int Status { get; set; } = 0;

        [Column("version")]
        public int Version { get; set; } = 0;

        [Column("cover")]
        public string? Cover { get; set; } = "";
    }
}
