using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Orders
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        [MaxLength(50), Required]
        public String FirstName { get; set; }

        [MaxLength(50), Required]
        public String LastName { get; set; }

        public Guid OrderId { get; set; }
        public Order Order { get; set; }

    }
}