namespace Gliese.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class BloggingContext : DbContext
{
    public DbSet<Articles> Articles { get; set; }

    public BloggingContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // string sPath = Environment.GetEnvironmentVariable("CSHARP_DSN"); 

            options.UseNpgsql(@"XXXXX");

        }
}

[Table("articles")]
[PrimaryKey(nameof(Pk))]
public class Articles
{ 
    [Column("pk")]
    public string Pk { get; set; } = "";
    [Column("title")]
    public string Title { get; set; } = "";
}
