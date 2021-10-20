namespace Venflow.Tests.Models
{
    public class ReverseEmailContent
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int EmailId { get; set; }

        public ReverseEmail Email { get; set; }
    }
}
