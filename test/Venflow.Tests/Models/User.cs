using System.Collections.Generic;

namespace Venflow.Tests.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Blog> Blogs { get; set; }
    }
}
