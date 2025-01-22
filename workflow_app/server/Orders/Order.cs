using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace server.Orders
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        [MaxLength(20), Required]
        public String InvoiceNumber { get; set; }

        [MaxLength(20), Required]
        public string QuoteNumber { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime LunchOut { get; set; }

        public DateTime LunchIn { get; set; }

        public DateTime EndTime { get; set; }

        public Guid ServiceId { get; set; }
        public ICollection<Service> Services { get; set; }

        public Guid MaterialId { get; set; }
        public ICollection<Material> Materials { get; set; }
    }
}