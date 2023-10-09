using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Recharge_Test.Repository.Models;

#nullable disable

namespace Recharge_Test.Repository
{
    public partial class EVDbContext : DbContext
    {
        public EVDbContext()
        {
        }

        public EVDbContext(DbContextOptions<EVDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Deposit> Deposits { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=EV_Recharge;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Deposit>(entity =>
            {
                entity.Property(e => e.ForeignCurrency).IsFixedLength(true);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Invoices_Customers1");
            });

            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                entity.HasKey(e => e.DetailId)
                    .HasName("PK_Invoices_Detail");

                entity.Property(e => e.DetailId).ValueGeneratedNever();

                entity.HasOne(d => d.Invoices)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(d => d.InvoicesId)
                    .HasConstraintName("FK_InvoiceDetails_Invoices");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
