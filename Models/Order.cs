using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Login_process_test.Models
{
    public class Order
    {
        public Order()
        {
            OrderLineItems = new HashSet<OrderLineItem>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        [Column(TypeName = "money")]
        public decimal OrderAmount { get; set; }
        [Required]
        [StringLength(128)]
        public string UserId { get; set; }
        public virtual ICollection<OrderLineItem> OrderLineItems { get; set; }
    }
}