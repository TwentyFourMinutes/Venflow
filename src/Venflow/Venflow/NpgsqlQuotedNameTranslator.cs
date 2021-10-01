using Npgsql;
using Npgsql.NameTranslation;

namespace Venflow
{
    /// <summary>
    /// A name translator which preserves CLR names (e.g. SomeClass) when mapping names to the database.
    /// </summary>
    public class NpgsqlQuotedNameTranslator : NpgsqlNullNameTranslator, INpgsqlNameTranslator
    {
        /// <summary>
        /// Given a CLR type name (e.g class, struct, enum), translates its name to a database type name.
        /// </summary>
        public string TranslateTypeName(string clrName) => $"\"{base.TranslateTypeName(clrName)}\"";

        /// <summary>
        /// Given a CLR member name (property or field), translates its name to a database type name.
        /// </summary>
        public string TranslateMemberName(string clrName) => $"\"{base.TranslateMemberName(clrName)}\"";
    }
}