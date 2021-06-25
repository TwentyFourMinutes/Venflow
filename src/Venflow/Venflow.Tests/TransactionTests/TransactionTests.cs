using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Tests.Models;
using Xunit;

namespace Venflow.Tests.TransactionTests
{
    public class TransactionTests : TestBase
    {
        [Fact]
        public async Task ReuseTransactionAsync()
        {
            await using var transaction = await Database.BeginTransactionAsync();

            Assert.Equal(transaction, await Database.BeginTransactionAsync());
        }

        [Fact]
        public async Task RollbackToTransactionSavepointAsync()
        {
            await using var transaction = await Database.BeginTransactionAsync();

            await Assert.ThrowsAsync<NotImplementedException>(() => Database.People.InsertAsync(new Person { Emails = new ThrowingEmailList() }));

            var transactionType = transaction.GetType();
            var npgsqlTransactionType = typeof(NpgsqlTransaction);

            Assert.False((bool)transactionType.GetProperty("IsDisposed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transaction));
            Assert.False((bool)npgsqlTransactionType.GetProperty("IsCompleted", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transaction.GetNpgsqlTransaction()));
        }

        private class ThrowingEmailList : IList<Email>
        {
            public Email this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public int Count => throw new NotImplementedException();
            public bool IsReadOnly => throw new NotImplementedException();

            public void Add(Email item)
            {
                throw new System.NotImplementedException();
            }

            public void Clear()
            {
                throw new System.NotImplementedException();
            }

            public bool Contains(Email item)
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(Email[] array, int arrayIndex)
            {
                throw new System.NotImplementedException();
            }

            public IEnumerator<Email> GetEnumerator()
            {
                throw new System.NotImplementedException();
            }

            public int IndexOf(Email item)
            {
                throw new System.NotImplementedException();
            }

            public void Insert(int index, Email item)
            {
                throw new System.NotImplementedException();
            }

            public bool Remove(Email item)
            {
                throw new System.NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new System.NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
