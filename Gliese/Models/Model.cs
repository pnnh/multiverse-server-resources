namespace Gliese.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class BloggingContext : DbContext
{
    public DbSet<ArticleTable> Articles => Set<ArticleTable>();
    public DbSet<ArticleExtendTable> ArticleExtendTable => Set<ArticleExtendTable>();
    public DbSet<ArticleViewerTable> ArticleViewerTable => Set<ArticleViewerTable>(); 
    public DbSet<AccountTable> Accounts => Set<AccountTable>();
    public DbSet<CredentialTable> Credentials => Set<CredentialTable>();
    public DbSet<SessionTable> Sessions => Set<SessionTable>();

    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
    {
    }
}
