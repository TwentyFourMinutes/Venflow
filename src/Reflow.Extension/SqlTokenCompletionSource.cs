using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;

namespace Reflow.Extension
{
    internal class SqlTokenCompletionSource : IAsyncCompletionSource
    {
        private readonly static ImageElement KeywordIcon = new ImageElement(
            new ImageId(
                new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"),
                KnownImageIds.IntellisenseKeyword
            ),
            "Keyword"
        );
        private readonly static ImageElement FunctionIcon = new ImageElement(
            new ImageId(
                new Guid("ae27a6b0-e345-4288-96df-5eaf394ee369"),
                KnownImageIds.MethodPublic
            ),
            "Function"
        );

        private readonly static CompletionFilter KeywordFilter = new CompletionFilter(
            "Keywords",
            "K",
            KeywordIcon
        );
        private readonly static CompletionFilter FunctionFilter = new CompletionFilter(
            "Functions",
            "F",
            FunctionIcon
        );

        private readonly static ImmutableArray<CompletionFilter> KeywordFilters =
            ImmutableArray.Create(KeywordFilter);
        private readonly static ImmutableArray<CompletionFilter> FunctionFilters =
            ImmutableArray.Create(FunctionFilter);

        private readonly SqlTokenCatalog _catalog;
        private readonly ITextStructureNavigatorSelectorService _structureNavigatorSelector;
        private readonly ITagAggregator<IClassificationTag> _tagAggregator;

        internal SqlTokenCompletionSource(
            SqlTokenCatalog catalog,
            ITextStructureNavigatorSelectorService structureNavigatorSelector,
            ITagAggregator<IClassificationTag> tagAggregator
        )
        {
            _catalog = catalog;
            _structureNavigatorSelector = structureNavigatorSelector;
            _tagAggregator = tagAggregator;
        }

        public CompletionStartData InitializeCompletion(
            CompletionTrigger trigger,
            SnapshotPoint triggerLocation,
            CancellationToken token
        )
        {
            if (
                char.IsWhiteSpace(trigger.Character)
                || char.IsNumber(trigger.Character)
                || char.IsPunctuation(trigger.Character)
                || trigger.Character == '\n'
                || trigger.Reason == CompletionTriggerReason.Backspace
                || trigger.Reason == CompletionTriggerReason.Deletion
            )
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            var tags = _tagAggregator.GetTags(new SnapshotSpan(triggerLocation, 1));

            if (!tags.Any(x => x.Tag.ClassificationType.Classification.StartsWith("sql")))
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            return new CompletionStartData(CompletionParticipation.ProvidesItems);
        }

        public Task<CompletionContext> GetCompletionContextAsync(
            IAsyncCompletionSession session,
            CompletionTrigger trigger,
            SnapshotPoint triggerLocation,
            SnapshotSpan applicableToSpan,
            CancellationToken token
        )
        {
            return Task.FromResult(GetContextForKey());
        }

        private CompletionContext GetContextForKey()
        {
            return new CompletionContext(
                _catalog.Tokens.Select(n => MakeItemFromElement(n)).ToImmutableArray()
            );
        }

        private CompletionItem MakeItemFromElement(SqlTokenCatalog.SqlToken token)
        {
            ImageElement icon;
            ImmutableArray<CompletionFilter> filters;

            switch (token.Category)
            {
                case SqlTokenCatalog.SqlToken.Categories.Keyword:
                    icon = KeywordIcon;
                    filters = KeywordFilters;
                    break;
                case SqlTokenCatalog.SqlToken.Categories.Function:
                    icon = FunctionIcon;
                    filters = FunctionFilters;
                    break;
                default:
                    throw new ArgumentException(nameof(token));
            }

            var item = new CompletionItem(
                displayText: token.Name,
                source: this,
                icon: icon,
                filters: filters,
                suffix: string.Empty,
                insertText: token.Name,
                sortText: token.Name,
                filterText: token.Name,
                attributeIcons: ImmutableArray<ImageElement>.Empty
            );

            item.Properties.AddProperty(nameof(SqlTokenCatalog.SqlToken), token);

            return item;
        }

        public Task<object> GetDescriptionAsync(
            IAsyncCompletionSession session,
            CompletionItem item,
            CancellationToken token
        )
        {
            if (
                item.Properties.TryGetProperty<SqlTokenCatalog.SqlToken>(
                    nameof(SqlTokenCatalog.SqlToken),
                    out var matchingElement
                )
            )
            {
                return Task.FromResult<object>(
                    $"{matchingElement.Name} is a {matchingElement.Category.ToString().ToLower()}"
                );
            }

            return Task.FromResult(new object());
        }
    }
}
