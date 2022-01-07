using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Reflow.Extension
{
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("SQL token completion provider")]
    [ContentType("csharp")]
    internal class SqlTokenCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        private readonly Dictionary<ITextView, IAsyncCompletionSource> _cache;

        private readonly SqlTokenCatalog _catalog;
        private readonly ITextStructureNavigatorSelectorService _structureNavigatorSelector;
        private readonly IViewTagAggregatorFactoryService _viewTagAggregator;

        [ImportingConstructor]
        public SqlTokenCompletionSourceProvider(
            SqlTokenCatalog catalog,
            ITextStructureNavigatorSelectorService structureNavigatorSelector,
            IViewTagAggregatorFactoryService viewTagAggregator
        )
        {
            _catalog = catalog;
            _structureNavigatorSelector = structureNavigatorSelector;
            _viewTagAggregator = viewTagAggregator;

            _cache = new Dictionary<ITextView, IAsyncCompletionSource>();
        }

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            if (_cache.TryGetValue(textView, out var itemSource))
                return itemSource;

            var source = new SqlTokenCompletionSource(
                _catalog,
                _structureNavigatorSelector,
                _viewTagAggregator.CreateTagAggregator<IClassificationTag>(textView)
            );

            textView.Closed += (o, e) => _cache.Remove(textView);

            _cache.Add(textView, source);

            return source;
        }
    }
}
