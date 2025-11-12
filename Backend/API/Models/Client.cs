using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string? Email { get; set; }
        public string? Company { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
    }
}
