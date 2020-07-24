using System.Collections.Generic;

namespace Venflow.Tests.Models
{
    public class Person
    {
        public int Id { get; set; }

        public virtual string Name { get; set; }

        public List<Email> Emails { get; set; }
    }
}
