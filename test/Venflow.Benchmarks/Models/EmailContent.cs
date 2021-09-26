using System.ComponentModel.DataAnnotations.Schema;

namespace Venflow.Benchmarks.Models
{
    [Table("EmailContents")]
    public class EmailContent
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int EmailId { get; set; }

        public Email Email { get; set; }
    }
}
