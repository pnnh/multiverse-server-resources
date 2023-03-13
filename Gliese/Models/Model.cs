namespace Gliese.Models;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

public class DatabaseContext : DbContext
{
    public DbSet<AccountTable> Accounts => Set<AccountTable>();
    public DbSet<ResourceTable> Resources => Set<ResourceTable>();

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
}
