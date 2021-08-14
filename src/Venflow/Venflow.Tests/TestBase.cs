using System;
using Venflow.Tests.Models;

namespace Venflow.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected RelationDatabase Database { get; }

        protected TestBase()
        {
            Database = new RelationDatabase();
        }

        public void Dispose()
        {
            Database.Dispose();
        }
    }
}
