using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace API.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; }
        public DateTime DateIssued { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        // [ForeignKey("Client")]
        public int ClientId { get; set; }
        public Client? Client { get; set; }

        // [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<InvoiceItem>? Items { get; set; }
    }

    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.Id);
            builder.Property(i => i.InvoiceNumber).IsRequired();
            builder.Property(i => i.DateIssued).IsRequired();
            builder.Property(i => i.DueDate).IsRequired();
            builder.Property(i => i.Status).IsRequired();
            builder.Property(i => i.Subtotal).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(i => i.Tax).IsRequired().HasColumnType("decimal(18,2)");
            builder.Property(i => i.Total).IsRequired().HasColumnType("decimal(18,2)");
        }
    }
}
