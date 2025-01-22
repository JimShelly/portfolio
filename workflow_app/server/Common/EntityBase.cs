using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Common
{
    public class EntityBase
    {
        [MaxLength(50), Required]
        public String CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [MaxLength(50)]
        public String UpdatedBy { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}