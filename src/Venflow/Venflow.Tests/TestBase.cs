using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Venflow.Tests.Models;

namespace Venflow.Tests
{
    public abstract class TestBase : IAsyncDisposable
    {
        protected RelationDatabase Database { get; }

        protected TestBase()
        {
            Database = new RelationDatabase();
        }

        public ValueTask DisposeAsync()
        {
            return Database.DisposeAsync();
        }
    }
}
