using System;
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

            var person = new Person { Name = "RollbackPerson" };

            await Database.People.InsertAsync(person);

            var throwingPerson = new Person { Emails = new Email[] { new ThrowingEmail() } };

            await Assert.ThrowsAsync<NotImplementedException>(() => Database.People.InsertAsync(throwingPerson));

            Assert.Null(await Database.People.QueryInterpolatedSingle(@$"SELECT * FROM ""People"" WHERE ""Id"" = {throwingPerson.Id}").QueryAsync());

            var transactionType = transaction.GetType();
            var npgsqlTransactionType = typeof(NpgsqlTransaction);

            Assert.False((bool)transactionType.GetProperty("IsDisposed", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transaction));
            Assert.False((bool)npgsqlTransactionType.GetProperty("IsCompleted", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(transaction.GetNpgsqlTransaction()));

            await transaction.CommitAsync();

            Assert.Null(await Database.People.QueryInterpolatedSingle(@$"SELECT * FROM ""People"" WHERE ""Id"" = {throwingPerson.Id}").QueryAsync());
            Assert.NotNull(await Database.People.QueryInterpolatedSingle(@$"SELECT * FROM ""People"" WHERE ""Id"" = {person.Id}").QueryAsync());
        }

        private class ThrowingEmail : Email
        {
            public override string Address { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
