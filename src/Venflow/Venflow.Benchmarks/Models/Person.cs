using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Venflow.Benchmarks.Models
{
    [Table("People")]
    public class Person
    {
        public int Id { get; set; }

        public virtual string Name { get; set; }

        public List<Email> Emails { get; set; }
    }

    public class PersonConfiguration : EntityConfiguration<Person>
    {
        protected override void Configure(IEntityBuilder<Person> entityBuilder)
        {
            entityBuilder.HasMany(x => x.Emails)
                         .WithOne(x => x.Person)
                         .UsingForeignKey(x => x.PersonId);
        }
    }
}
