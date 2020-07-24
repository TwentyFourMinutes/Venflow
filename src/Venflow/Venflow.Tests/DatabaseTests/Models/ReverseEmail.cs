using System.Collections.Generic;

namespace Venflow.Tests.DatabaseTests.Models
{
    public class ReverseEmail
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public int PersonId { get; set; }

        public ReversePerson Person { get; set; }

        public List<ReverseEmailContent> Contents { get; set; }
    }
}
