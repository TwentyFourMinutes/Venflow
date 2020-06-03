using System.Collections.Generic;

namespace Venflow.Benchmarks.Benchmarks.Models
{
    public class Person2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Email2> Emails { get; set; }
    }

    public class Email2
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public int PersonId { get; set; }
    }
}
