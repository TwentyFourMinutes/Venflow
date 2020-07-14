using System.ComponentModel.DataAnnotations.Schema;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Benchmarks.Benchmarks.Models
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
