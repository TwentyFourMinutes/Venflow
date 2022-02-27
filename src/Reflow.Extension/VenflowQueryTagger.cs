using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Reflow.Extension
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("csharp")]
    [TagType(typeof(ClassificationTag))]
    internal class VenflowQueryTaggerProvider : IViewTaggerProvider
    {
        private readonly IClassificationTypeRegistryService _classificationRegistry;
        private readonly IBufferTagAggregatorFactoryService _tagAggregatorFactory;

        [ImportingConstructor]
        internal VenflowQueryTaggerProvider(
            IClassificationTypeRegistryService classificationRegistry,
            IBufferTagAggregatorFactoryService tagAggregatorFactory
        )
        {
            _classificationRegistry = classificationRegistry;
            _tagAggregatorFactory = tagAggregatorFactory;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var tagAggregator = _tagAggregatorFactory.CreateTagAggregator<IClassificationTag>(
                buffer
            );

            return (ITagger<T>)new VenflowQueryTagger(tagAggregator, _classificationRegistry);
        }
    }

    internal class VenflowQueryTagger : ITagger<ClassificationTag>
    {
        private readonly ITagAggregator<IClassificationTag> _tagAggregator;
        private readonly ClassificationTag _sqlString;
        private readonly ClassificationTag _sqlDefault;
        private readonly ClassificationTag _sqlKeyword;
        private readonly ClassificationTag _sqlFunction;
        private readonly ClassificationTag _sqlPunctuation;
        private readonly ClassificationTag _sqlBraces;
        private readonly ClassificationTag _sqlInterpolation;

        public VenflowQueryTagger(
            ITagAggregator<IClassificationTag> tagAggregator,
            IClassificationTypeRegistryService classificationRegistry
        )
        {
            _tagAggregator = tagAggregator;
            _sqlString = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlString)
            );
            _sqlDefault = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlDefault)
            );
            _sqlKeyword = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlKeyword)
            );
            _sqlFunction = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlFunction)
            );
            _sqlPunctuation = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlPunctuation)
            );
            _sqlBraces = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlBraces)
            );
            _sqlInterpolation = new ClassificationTag(
                classificationRegistry.GetClassificationType(Constants.SqlInterpolation)
            );
        }

#pragma warning disable CS0414, CS0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged = null!;
#pragma warning restore CS0414, CS0067

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(
            NormalizedSnapshotSpanCollection spans
        )
        {
            var classifications = new List<ITagSpan<ClassificationTag>>();

            var lastValidatedIndex = 0;

            foreach (var spanTag in _tagAggregator.GetTags(spans))
            {
                var tagType = spanTag.Tag.ClassificationType.Classification;

                if (tagType is "string" or "string - verbatim")
                {
                    var snapshotSpan = spanTag.Span.GetSnapshotSpan();

                    if (snapshotSpan.Start.Position < lastValidatedIndex)
                        continue;

                    var snapshot = snapshotSpan.Snapshot;
                    var spanIterator = new SqlSpanIterator(_tagAggregator, snapshotSpan);

                    var validator = new SqlTagValidator(SqlTagValidator.WalkDirection.Backward);

                    validator.Validate(spanTag, spanTag.Span.GetSnapshotSpan());

                    IMappingTagSpan<IClassificationTag> iterationTag;

                    var containsInvalidTag = false;

                    while ((iterationTag = spanIterator.GetNextPreviousTag()) is not null)
                    {
                        if (!validator.Validate(iterationTag, iterationTag.Span.GetSnapshotSpan()))
                        {
                            containsInvalidTag = true;
                            break;
                        }

                        if (validator.HasFinished)
                            break;
                    }

                    if (!validator.HasFinished)
                    {
                        return Enumerable.Empty<ITagSpan<ClassificationTag>>();
                    }

                    if (validator.TextSpan.HasValue)
                        lastValidatedIndex = validator.TextSpan.Value.End;

                    if (containsInvalidTag)
                        continue;

                    validator.Direction = SqlTagValidator.WalkDirection.Forward;

                    while ((iterationTag = spanIterator.GetNextSubsequentTag()) is not null)
                    {
                        if (!validator.Validate(iterationTag, iterationTag.Span.GetSnapshotSpan()))
                        {
                            containsInvalidTag = true;
                            break;
                        }

                        if (validator.HasFinished)
                            break;
                    }

                    if (!validator.HasFinished)
                    {
                        return Enumerable.Empty<ITagSpan<ClassificationTag>>();
                    }

                    if (validator.TextSpan.HasValue)
                        lastValidatedIndex = validator.TextSpan.Value.End + 1;

                    if (containsInvalidTag)
                        continue;

                    for (
                        var interpolationSpanIndex = 0;
                        interpolationSpanIndex < validator.TextSpans.Count;
                        interpolationSpanIndex++
                    )
                    {
                        var textSpan = validator.TextSpans[interpolationSpanIndex];
                        var text = snapshot.GetText(textSpan);

                        GetSqlClassifications(snapshot, classifications, text, textSpan.Start);

                        for (
                            var punctuationIndex = 0;
                            (
                                punctuationIndex = text.IndexOfAny(
                                    SqlConstants.Operators,
                                    punctuationIndex
                                )
                            ) != -1;
                            punctuationIndex++
                        )
                        {
                            classifications.Add(
                                new TagSpan<ClassificationTag>(
                                    new SnapshotSpan(
                                        snapshot,
                                        textSpan.Start + punctuationIndex,
                                        1
                                    ),
                                    _sqlPunctuation
                                )
                            );
                        }

                        for (
                            var punctuationIndex = 0;
                            (
                                punctuationIndex = text.IndexOfAny(
                                    SqlConstants.Braces,
                                    punctuationIndex
                                )
                            ) != -1;
                            punctuationIndex++
                        )
                        {
                            classifications.Add(
                                new TagSpan<ClassificationTag>(
                                    new SnapshotSpan(
                                        snapshot,
                                        textSpan.Start + punctuationIndex,
                                        1
                                    ),
                                    _sqlBraces
                                )
                            );
                        }
                    }

                    classifications.Add(
                        new TagSpan<ClassificationTag>(
                            new SnapshotSpan(
                                snapshot,
                                validator.TextSpan!.Value.Start,
                                validator.TextSpan!.Value.Length
                            ),
                            _sqlString
                        )
                    );

                    for (
                        var interpolationSpanIndex = 0;
                        interpolationSpanIndex < validator.InterpolationSpans.Count;
                        interpolationSpanIndex++
                    )
                    {
                        var interpoaltionSpan = validator.InterpolationSpans[
                            interpolationSpanIndex
                        ];
                        classifications.Add(
                            new TagSpan<ClassificationTag>(
                                new SnapshotSpan(
                                    snapshot,
                                    interpoaltionSpan.Start,
                                    interpoaltionSpan.Length
                                ),
                                _sqlInterpolation
                            )
                        );
                    }
                }
            }

            var strippedClassifications = classifications
                .GroupBy(x => x.Span.Span)
                .Select(x => x.First());

            return strippedClassifications;
        }

        private void GetSqlClassifications(
            ITextSnapshot snapshot,
            List<ITagSpan<ClassificationTag>> classifications,
            string text,
            int offset
        )
        {
            var matchIndex = 0;

            Match match;
            while (
                (match = SqlConstants.Splitter.Match(text, matchIndex)).Success
                && matchIndex < text.Length
            )
            {
                if (match.Index != 0 && match.Index - matchIndex > 0)
                {
                    var substring = text.Substring(matchIndex, match.Index - matchIndex)
                        .ToUpperInvariant();

                    if (SqlConstants.Keywords.Contains(substring))
                    {
                        var span = new SnapshotSpan(snapshot, offset, substring.Length);

                        classifications.Add(new TagSpan<ClassificationTag>(span, _sqlKeyword));
                    }
                    else if (match.Index + 1 < text.Length && text[match.Index] == '(')
                    {
                        if (SqlConstants.Functions.Contains(substring))
                        {
                            classifications.Add(
                                new TagSpan<ClassificationTag>(
                                    new SnapshotSpan(snapshot, offset, substring.Length),
                                    _sqlFunction
                                )
                            );
                        }
                    }
                    else
                    {
                        var span = new SnapshotSpan(snapshot, offset, substring.Length);

                        classifications.Add(new TagSpan<ClassificationTag>(span, _sqlDefault));
                    }

                    offset += substring.Length;
                }

                matchIndex = match.Index + match.Length;
                offset += match.Length;
            }
        }
    }

    internal class SqlSpanIterator
    {
        private int _subsequentTagIndex;
        private int _previousTagIndex;
        private int _subsequentLineIndex;
        private int _previousLineIndex;

        private readonly ITagAggregator<IClassificationTag> _tagAggregator;
        private readonly List<IMappingTagSpan<IClassificationTag>> _subsequentTags;
        private readonly List<IMappingTagSpan<IClassificationTag>> _previousTags;
        private readonly ITextSnapshot _snapshot;

        internal SqlSpanIterator(
            ITagAggregator<IClassificationTag> tagAggregator,
            SnapshotSpan rootSpan
        )
        {
            _tagAggregator = tagAggregator;
            _snapshot = rootSpan.Snapshot;

            _previousLineIndex = _subsequentLineIndex = rootSpan.Start.GetContainingLineNumber();

            _subsequentTags = new List<IMappingTagSpan<IClassificationTag>>();
            _previousTags = new List<IMappingTagSpan<IClassificationTag>>();

            var tagLine = _snapshot.GetLineFromLineNumber(_previousLineIndex);

            foreach (
                var tag in _tagAggregator.GetTags(new SnapshotSpan(tagLine.Start, tagLine.End))
            )
            {
                var tagSpan = tag.Span.GetSnapshotSpan();

                if (tagSpan.Span == rootSpan.Span)
                    continue;

                if (tagSpan.End.Position > rootSpan.End.Position)
                    _subsequentTags.Add(tag);
                else if (tagSpan.Start.Position < rootSpan.Start.Position)
                    _previousTags.Add(tag);
                else
                    continue;
            }

            _previousTags.Reverse();
        }

        internal IMappingTagSpan<IClassificationTag> GetNextPreviousTag()
        {
            while (_previousTagIndex == _previousTags.Count)
            {
                if (_previousTags.Count > 0)
                {
                    _previousTags.Clear();
                    _previousTagIndex = 0;
                }

                _previousLineIndex--;

                if (_previousLineIndex == 0)
                {
                    return null!;
                }

                var line = _snapshot.GetLineFromLineNumber(_previousLineIndex);
                var lineSpan = new SnapshotSpan(line.Start, line.End);

                foreach (var tag in _tagAggregator.GetTags(lineSpan).Reverse())
                {
                    _previousTags.Add(tag);
                }
            }

            return _previousTags[_previousTagIndex++];
        }

        internal IMappingTagSpan<IClassificationTag> GetNextSubsequentTag()
        {
            while (_subsequentTagIndex == _subsequentTags.Count)
            {
                if (_subsequentTags.Count > 0)
                {
                    _subsequentTags.Clear();
                    _subsequentTagIndex = 0;
                }

                _subsequentLineIndex++;

                if (_subsequentLineIndex == _snapshot.LineCount)
                {
                    return null!;
                }

                var line = _snapshot.GetLineFromLineNumber(_subsequentLineIndex);
                var lineSpan = new SnapshotSpan(line.Start, line.End);

                foreach (var tag in _tagAggregator.GetTags(lineSpan))
                {
                    _subsequentTags.Add(tag);
                }
            }

            return _subsequentTags[_subsequentTagIndex++];
        }
    }

    internal class SqlTagValidator
    {
        private WalkDirection _direction;
        internal WalkDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                _state = State.String;
                HasFinished = false;
            }
        }

        internal bool HasFinished { get; private set; }
        internal Span? TextSpan { get; private set; }
        internal List<Span> TextSpans { get; private set; }
        internal List<Span> InterpolationSpans { get; }

        private State _state = State.None;
        private int _interpolationDeepness;
        private static readonly Regex _stringOffsetMatcher = new Regex(
            @"^\$?\""",
            RegexOptions.Compiled
        );
        private static readonly Regex _verbatimStringOffsetMatcher = new Regex(
            @"^[@\$]{1,2}\""",
            RegexOptions.Compiled
        );

        internal SqlTagValidator(WalkDirection direction)
        {
            Direction = direction;
            TextSpan = null;
            TextSpans = new List<Span>();
            InterpolationSpans = new List<Span>();
        }

        internal bool Validate(IMappingTagSpan<IClassificationTag> tag, SnapshotSpan span)
        {
            var type = tag.Tag.ClassificationType.Classification;
            var text = span.GetText();

            if (text == string.Empty || type == "comment")
                return true;

            if (
                (type == "string" || type == "string - verbatim")
                && (
                    _state == State.None
                    || _state == State.String
                    || (_state == State.Interpolation && _interpolationDeepness == 0)
                )
            )
            {
                _state = State.String;

                if (text == "\"")
                {
                    HasFinished = true;

                    return true;
                }

                Match match;

                if (type == "string")
                {
                    match = _stringOffsetMatcher.Match(text);
                }
                else
                {
                    match = _verbatimStringOffsetMatcher.Match(text);
                }

                var offset = match.Success ? match.Length : 0;
                var stringStart = span.Start.Position + offset;
                var stringSpan = new Span(stringStart, span.Length - offset);

                if (match.Length != text.Length && stringSpan.Length > 0)
                {
                    TextSpans.Add(stringSpan);
                }

                if (TextSpan.HasValue)
                {
                    AdjustTextSpan(stringSpan);
                }
                else
                {
                    TextSpan = stringSpan;
                }

                return true;
            }
            else if (
                type == "punctuation"
                && _state == State.String
                && (
                    (Direction == WalkDirection.Forward && text == "{")
                    || (Direction == WalkDirection.Backward && text == "}")
                )
            )
            {
                _state = State.Interpolation;
                _interpolationDeepness++;

                InterpolationSpans.Add(new Span(span.Start.Position, 1));

                return true;
            }
            else if (_state == State.Interpolation)
            {
                if (type == "punctuation")
                {
                    if (Direction == WalkDirection.Forward)
                    {
                        if (text == "{")
                        {
                            _interpolationDeepness++;
                            InterpolationSpans.Add(new Span(span.Start.Position, 1));
                            AdjustTextSpan(span);
                        }
                        else if (text == "}")
                        {
                            _interpolationDeepness--;
                            InterpolationSpans.Add(new Span(span.Start.Position, 1));
                            AdjustTextSpan(span);
                        }
                    }
                    else if (Direction == WalkDirection.Backward)
                    {
                        if (text == "{")
                        {
                            _interpolationDeepness--;
                            InterpolationSpans.Add(new Span(span.Start.Position, 1));
                            AdjustTextSpan(span);
                        }
                        else if (text == "}")
                        {
                            _interpolationDeepness++;
                            InterpolationSpans.Add(new Span(span.Start.Position, 1));
                            AdjustTextSpan(span);
                        }
                    }
                }

                return true;
            }
            else if (Direction == WalkDirection.Backward)
            {
                if (type == "operator" && text == "=>" && _state == State.String)
                {
                    _state = State.LambdaOperator;
                    return true;
                }
                else if (
                    (type == "punctuation" || type == "parameter name" || type == "identifier")
                    && (_state == State.LambdaOperator || _state == State.LambdaArgumentsStarted)
                )
                {
                    if (text == "(")
                    {
                        _state = State.LambdaArgumentsFinished;
                    }
                    else
                    {
                        _state = State.LambdaArgumentsStarted;
                    }

                    return true;
                }
                else if (
                    type == "punctuation" && _state == State.LambdaArgumentsFinished && text == "("
                )
                {
                    _state = State.MethodInvocation;

                    return true;
                }
                else if (
                    (type == "punctuation" || type == "identifier")
                    && (
                        _state == State.MethodInvocation
                        || _state == State.TypeArgumentsStarted
                        || _state == State.LambdaArgumentsFinished
                    )
                )
                {
                    if (text == "<")
                    {
                        _state = State.TypeArgumentsFinished;
                    }
                    else
                    {
                        _state = State.TypeArgumentsStarted;
                    }

                    HasFinished = true;

                    return true;
                }
                else if (
                    type == "identifier"
                    && (
                        _state == State.MethodInvocation
                        || _state == State.TypeArgumentsFinished
                        || _state == State.LambdaArgumentsFinished
                    )
                    && (text == "Query" || text == "QueryRaw")
                )
                {
                    HasFinished = true;

                    return true;
                }
            }
            else if (Direction == WalkDirection.Forward)
            {
                if (type == "punctuation" && text == ")" && _state == State.String)
                {
                    if (HasFinished)
                        return true;

                    HasFinished = true;

                    var textSpans = TextSpans.Distinct().OrderBy(x => x.End).ToList();

                    if (TextSpans.Count == 0)
                        return false;

                    var lastTextSpan = textSpans[textSpans.Count - 1];

                    if (lastTextSpan.Length == 1)
                    {
                        textSpans.RemoveAt(textSpans.Count - 1);
                    }
                    else
                    {
                        textSpans[textSpans.Count - 1] = new Span(
                            lastTextSpan.Start,
                            lastTextSpan.Length - 1
                        );
                    }

                    TextSpans = textSpans;

                    TextSpan = new Span(TextSpan!.Value.Start, TextSpan!.Value.Length - 1);

                    return true;
                }
            }

            return false;
        }

        private void AdjustTextSpan(Span span)
        {
            if (TextSpan!.Value.Start > span.Start)
            {
                TextSpan = new Span(span.Start, TextSpan!.Value.End - span.Start);
            }

            if (TextSpan!.Value.End < span.End)
            {
                TextSpan = new Span(TextSpan!.Value.Start, span.End - TextSpan!.Value.Start);
            }
        }

        private enum State : byte
        {
            None,
            String,
            Interpolation,
            LambdaOperator,
            LambdaArgumentsStarted,
            LambdaArgumentsFinished,
            TypeArgumentsStarted,
            TypeArgumentsFinished,
            MethodInvocation
        }

        internal enum WalkDirection : byte
        {
            Forward,
            Backward
        }
    }
}
