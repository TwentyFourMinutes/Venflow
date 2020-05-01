using System.ComponentModel.DataAnnotations.Schema;
using Venflow.Modeling;

namespace Venflow.Benchmarks.Benchmarks.Models
{
    public class MyDbConfiguration : DbConfiguration
    {
        public MyDbConfiguration() : base(SecretsHandler.GetConnectionString())
        {

        }

        protected override void Configure(DbConfigurator dbConfigurator)
        {
            dbConfigurator.AddEntity<PersonConfiguration, Person>();
        }
    }

    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class PersonConfiguration : EntityConfiguration<Person>
    {
        protected override void Configure(EntityBuilder<Person> entityBuilder)
        {
            entityBuilder.MapId(x => x.Id, DatabaseGeneratedOption.Computed);
        }
    }
}
