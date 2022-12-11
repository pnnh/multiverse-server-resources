namespace Gliese.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class BloggingContext : DbContext
{
    public DbSet<ArticleModel> Articles => Set<ArticleModel>();

    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
    {
    }
}
