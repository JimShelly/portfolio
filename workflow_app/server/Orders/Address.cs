using System;
using server.Orders.CustomTypes;
using server.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Orders
{
    public class Address : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }

        [MaxLength(100), Required]
        public String Street1 { get; set; }

        [MaxLength(100)]
        public String Street2 { get; set; }

        [MaxLength(100), Required]
        public String City { get; set; }

        [MaxLength(2), Required]
        public String State { get; set; }

        [MaxLength(20), Required]
        public String ZipCode { get; set; }
        public AddressType AddressType { get; set; }

        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}