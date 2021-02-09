using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Venflow.Tests.Models
{
    public class Person
    {
        public int Id { get; }

        public virtual string? Name { get; set; }

        public string Something { get; }
        public string SomethingElse { get; private set; } = "SomethingElse";

        [NotMapped]
        public string Stuff { get; set; }

        public List<Email> Emails { get; set; }
    }
}
