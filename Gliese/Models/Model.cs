namespace Gliese.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class BloggingContext : DbContext
{
    public DbSet<Articles> Articles => Set<Articles>();

    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
    {
    }

    // protected override void OnConfiguring(DbContextOptionsBuilder options)
    //     {
    //         // string sPath = Environment.GetEnvironmentVariable("CSHARP_DSN"); 

    //         options.UseNpgsql(@"XXXXX");

    //     }
}

[Table("articles")]
[PrimaryKey(nameof(Pk))]
public class Articles
{
    [Column("pk")]
    public string Pk { get; set; } = "";
    [Column("title")]
    public string Title { get; set; } = "";
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
    [Column("mark_lang")]
    public int MarkLang { get; set; } = 0;
    [Column("status")]
    public int Status { get; set; } = 0;
    [Column("mark_text")]
    public string? MarkText { get; set; } = "";
    [Column("cover")]
    public string? Cover { get; set; } = "";
}
