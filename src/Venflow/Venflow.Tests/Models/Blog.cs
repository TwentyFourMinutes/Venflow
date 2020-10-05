namespace Venflow.Tests.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Topic { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
