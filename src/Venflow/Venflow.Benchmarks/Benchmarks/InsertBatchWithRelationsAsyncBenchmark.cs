using BenchmarkDotNet.Attributes;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Venflow.Benchmarks.Benchmarks.Models;
using Venflow.Commands;

namespace Venflow.Benchmarks.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertBatchWithRelationsAsyncBenchmark : BenchmarkBase
    {
        private IInsertCommand<Email> _command;

        [GlobalSetup]
        public override async Task Setup()
        {
            await base.Setup();

            PersonDbContext.Emails.AddRange(GetDummyEmails());

            await PersonDbContext.SaveChangesAsync();

            _command = VenflowDbConnection.Insert<Email>(false).Todo();
            await VenflowDbConnection.InsertBatchAsync(_command, GetDummyEmails());
        }

        private List<Email> GetDummyEmails()
        {
            var emails = new List<Email>();

            for (int i = 0; i < 20; i++)
            {
                var person = new Person { Name = "Test" + i.ToString(), Emails = new List<Email>() };

                for (int k = 0; k < 2; k++)
                {
                    var email = new Email { Address = person.Name + k.ToString(), Person = person, Contents = new List<EmailContent>() };

                    person.Emails.Add(email);

                    emails.Add(email);

                    for (int z = 0; z < 2; z++)
                    {
                        email.Contents.Add(new EmailContent { Content = email.Address + z.ToString(), Email = email });
                    }
                }
            }

            return emails;
        }

        [Benchmark]
        public Task InsertEFCoreAsync()
        {
            PersonDbContext.Emails.AddRange(GetDummyEmails());

            return PersonDbContext.SaveChangesAsync();
        }

        [Benchmark]
        public Task InsertVenflowAsync()
        {
            return VenflowDbConnection.InsertBatchAsync(_command, GetDummyEmails());
        }

        [Benchmark]
        public async Task InsertHandAsync()
        {
            var emails = GetDummyEmails();

            if (emails is null || emails.Count == 0)
                return;

            var people = new List<Person>();
            var visitedPeople = new ObjectIDGenerator();
            var emailContents = new List<EmailContent>();

            for (int i = 0; i < emails.Count; i++)
            {
                var email = emails[i];

                if (email.Contents is { } && email.Contents.Count > 0)
                {
                    emailContents.AddRange(email.Contents);
                }

                if (email.Person is null)
                    continue;

                visitedPeople.GetId(email.Person, out var duplicate);

                if (duplicate)
                {
                    continue;
                }

                people.Add(email.Person);
            }

            var builder = new StringBuilder();
            var command = new NpgsqlCommand();

            command.Connection = VenflowDbConnection.Connection;

            if (people.Count > 0)
            {
                builder.Append("INSERT INTO \"People\" (\"Name\") VALUES ");

                for (int i = 0; i < people.Count; i++)
                {
                    var person = people[i];

                    builder.Append($"(@Name{i}), ");

                    command.Parameters.Add(new NpgsqlParameter<string>($"@Name{i}", person.Name));
                }

                builder.Length -= 2;

                builder.Append(" RETURNING \"Id\"");

                command.CommandText = builder.ToString();

                var reader = await command.ExecuteReaderAsync(people.Count == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default);

                for (int i = 0; i < people.Count; i++)
                {
                    await reader.ReadAsync();

                    var key = reader.GetFieldValue<int>(0);

                    var person = people[i];

                    person.Id = key;

                    for (int k = 0; k < person.Emails.Count; k++)
                    {
                        person.Emails[k].PersonId = key;
                    }
                }

                await reader.DisposeAsync();
            }

            if (emails.Count > 0)
            {
                builder.Clear();
                command.Parameters.Clear();

                builder.Append("INSERT INTO \"Emails\" (\"Address\", \"PersonId\") VALUES ");

                for (int i = 0; i < emails.Count; i++)
                {
                    var email = emails[i];

                    builder.Append($"(@Address{i}, @PersonId{i}), ");

                    command.Parameters.Add(new NpgsqlParameter<string>($"@Address{i}", email.Address));
                    command.Parameters.Add(new NpgsqlParameter<int>($"@PersonId{i}", email.PersonId));
                }

                builder.Length -= 2;

                builder.Append(" RETURNING \"Id\"");

                command.CommandText = builder.ToString();

                var reader = await command.ExecuteReaderAsync(emails.Count == 1 ? CommandBehavior.SingleRow : CommandBehavior.Default).ConfigureAwait(false);

                for (int i = 0; i < emails.Count; i++)
                {
                    await reader.ReadAsync();

                    var key = reader.GetFieldValue<int>(0);

                    var email = emails[i];

                    email.Id = key;

                    for (int k = 0; k < email.Contents.Count; k++)
                    {
                        email.Contents[k].EmailId = key;
                    }
                }

                await reader.DisposeAsync();
            }

            if (emailContents.Count > 0)
            {
                builder.Clear();
                command.Parameters.Clear();

                builder.Append("INSERT INTO \"EmailContents\" (\"Content\", \"EmailId\") VALUES ");

                for (int i = 0; i < emailContents.Count; i++)
                {
                    var emailContent = emailContents[i];

                    builder.Append($"(@Content{i}, @EmailId{i}), ");

                    command.Parameters.Add(new NpgsqlParameter<string>($"@Content{i}", emailContent.Content));
                    command.Parameters.Add(new NpgsqlParameter<int>($"@EmailId{i}", emailContent.EmailId));
                }

                builder.Length -= 2;

                builder.Append(" RETURNING \"Id\"");

                command.CommandText = builder.ToString();

                var reader = await command.ExecuteReaderAsync(emailContents.Count == 0 ? CommandBehavior.SingleRow : CommandBehavior.Default);

                for (int i = 0; i < emailContents.Count; i++)
                {
                    await reader.ReadAsync();

                    var key = reader.GetFieldValue<int>(0);

                    var emailContent = emailContents[i];

                    emailContent.Id = key;
                }

                await reader.DisposeAsync();
            }
        }

        [GlobalCleanup]
        public override async Task Cleanup()
        {
            await base.Cleanup();
        }
    }
}