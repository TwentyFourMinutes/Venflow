using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Reflow.Extension
{
    [Export]
    internal class SqlTokenCatalog
    {
        internal List<SqlToken> Tokens { get; }

        internal SqlTokenCatalog()
        {
            Tokens = new List<SqlToken>();

            foreach (var keyword in SqlConstants.Keywords)
            {
                Tokens.Add(new SqlToken(keyword, SqlToken.Categories.Keyword));
            }

            foreach (var function in SqlConstants.Functions)
            {
                Tokens.Add(new SqlToken(function, SqlToken.Categories.Function));
            }
        }

        internal class SqlToken
        {
            internal string Name { get; }
            internal Categories Category { get; }

            internal enum Categories
            {
                Uncategorized,
                Keyword,
                Function
            }

            internal SqlToken(string name, Categories category)
            {
                Name = name;
                Category = category;
            }
        }
    }
}
