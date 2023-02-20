
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

namespace Gliese.Models { 

    [Table("users")]
    [PrimaryKey(nameof(Pk))]
    public class UserTable
    {
        [Column("pk")]
        public string Pk { get; set; } = "";

        [Column("username")]
        public string Username { get; set; } = "";

        [Column("access_token")]
        public string? AccessToken { get; set; } = ""; 
        
        [Column("nickname")]
        public string? Nickname { get; set; } = "";
    } 
}