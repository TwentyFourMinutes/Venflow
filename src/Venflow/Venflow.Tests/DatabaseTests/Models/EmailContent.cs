namespace Venflow.Tests.DatabaseTests.Models
{
    public class EmailContent
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int EmailId { get; set; }

        public Email Email { get; set; }
    }
}
