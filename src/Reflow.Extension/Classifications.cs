using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Reflow.Extension
{
    internal static class SqlClassificationDefinitions
    {
#pragma warning disable CS0649, CS8618

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlString)]
        internal static ClassificationTypeDefinition String;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlDefault)]
        internal static ClassificationTypeDefinition Default;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlKeyword)]
        internal static ClassificationTypeDefinition Keyword;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlFunction)]
        internal static ClassificationTypeDefinition Function;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlPunctuation)]
        internal static ClassificationTypeDefinition Punctuation;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlBraces)]
        internal static ClassificationTypeDefinition Braces;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.SqlInterpolation)]
        internal static ClassificationTypeDefinition Interpolation;

#pragma warning restore CS0649, CS8618
    }

    static class Constants
    {
        public const string SqlString = "sql - string";
        public const string SqlKeyword = "sql - keyword";
        public const string SqlFunction = "sql - function";
        public const string SqlPunctuation = "sql - punctuation";
        public const string SqlBraces = "sql - braces";
        public const string SqlDefault = "sql - default";
        public const string SqlInterpolation = "sql - interpolation";

        public static readonly Color KeywordColor = Colors.MediumPurple;
        public static readonly Color FunctionColor = Colors.Teal;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlString)]
    [Name(Constants.SqlString)]
    [UserVisible(true)]
    [Order(After = Priority.Low)]
    internal sealed class SqlStringFormat : ClassificationFormatDefinition
    {
        public SqlStringFormat()
        {
            this.DisplayName = Constants.SqlString;
            this.BackgroundColor = Colors.Gray;
            this.BackgroundOpacity = 0.1;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlDefault)]
    [Name(Constants.SqlDefault)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SqlDefaultFormat : ClassificationFormatDefinition
    {
        public SqlDefaultFormat()
        {
            this.DisplayName = Constants.SqlDefault;
            this.ForegroundColor = Color.FromRgb(249, 168, 212);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlKeyword)]
    [Name(Constants.SqlKeyword)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SqlKeywordFormat : ClassificationFormatDefinition
    {
        public SqlKeywordFormat()
        {
            this.DisplayName = Constants.SqlKeyword;
            this.ForegroundColor = Color.FromRgb(99, 102, 241);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlFunction)]
    [Name(Constants.SqlFunction)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SqlFunctionFormat : ClassificationFormatDefinition
    {
        public SqlFunctionFormat()
        {
            this.DisplayName = Constants.SqlFunction;
            this.ForegroundColor = Color.FromRgb(45, 212, 191);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlPunctuation)]
    [Name(Constants.SqlPunctuation)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SqlPunctuationFormat : ClassificationFormatDefinition
    {
        public SqlPunctuationFormat()
        {
            this.DisplayName = Constants.SqlPunctuation;
            this.ForegroundColor = Color.FromRgb(224, 242, 254);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlBraces)]
    [Name(Constants.SqlBraces)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SqlBracesFormat : ClassificationFormatDefinition
    {
        public SqlBracesFormat()
        {
            this.DisplayName = Constants.SqlPunctuation;
            this.ForegroundColor = Color.FromRgb(13, 148, 136);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Constants.SqlInterpolation)]
    [Name(Constants.SqlInterpolation)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class SqlInterpolationFormat : ClassificationFormatDefinition
    {
        public SqlInterpolationFormat()
        {
            this.DisplayName = Constants.SqlInterpolation;
            this.ForegroundColor = Color.FromRgb(167, 139, 250);
        }
    }
}
