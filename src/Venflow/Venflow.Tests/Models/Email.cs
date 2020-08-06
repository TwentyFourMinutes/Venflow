using System.Collections.Generic;

namespace Venflow.Tests.Models
{
    public class Email
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public int PersonId { get; set; }

        public Person Person { get; set; }

        public List<EmailContent> Contents { get; set; }
    }
}
