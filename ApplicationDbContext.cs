using System;
using System.Collections.Generic;
using ECare_Revamp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECare_Revamp.Data;

public partial class ApplicationDbContext : IdentityDbContext<ECare_User>
{
    private readonly IConfiguration _configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }

    public virtual DbSet<ECare_User> People { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("Ecare");
            optionsBuilder.UseOracle(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName) && tableName.StartsWith("AspNet"))
            {
                entity.SetTableName("ECARE_REVAMP_" + tableName);
            }
        }
        modelBuilder.Entity<ECare_User>(entity =>
        {
            entity.ToTable("ECare_User");
            entity.Property(e => e.Id).HasMaxLength(450);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.SecurityStamp).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.ConcurrencyStamp).IsConcurrencyToken().HasMaxLength(256);
            entity.Property(e => e.AccessFailedCount).HasDefaultValue(0);

            modelBuilder.Entity<ECare_User>().Property(p => p.IsActive).HasConversion<int>();
            modelBuilder.Entity<ECare_User>().Property(p => p.EmailConfirmed).HasConversion<int>();
            modelBuilder.Entity<ECare_User>().Property(p => p.TwoFactorEnabled).HasConversion<int>();
            modelBuilder.Entity<ECare_User>().Property(p => p.LockoutEnabled).HasConversion<int>();
            modelBuilder.Entity<ECare_User>().Property(p => p.PhoneNumberConfirmed).HasConversion<int>();
        });
        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
