using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using System.Diagnostics;
using System.Reflection;

namespace API
{
  public class ApplicationContext : DbContext
  {
    public DbSet<User> Users { get; set; }
    public DbSet<Story> Stories { get; set; }
    public DbSet<FileModel> Files { get; set; }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
      Debug.WriteLine("[Db context] Init start");
      Database.EnsureCreated();
      Debug.WriteLine("[Db context] Init end");
    }
  }
}