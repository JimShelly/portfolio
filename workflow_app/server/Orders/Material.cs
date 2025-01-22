using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace server.Orders
{
    public class Material
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        [MaxLength(255), Required]
        public String Description { get; set; }

        [Required]
        public Int32 Quantity { get; set; }

        public Guid OrderId { get; set; }

        public Order Order { get; set; }
    }
}