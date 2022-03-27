using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Reflow.Analyzer
{
    internal abstract class GeneratorSection<TPrevious, TSyntaxReceiver, TData>
        : IGeneratorSection<TPrevious, TSyntaxReceiver>
        where TPrevious : IGeneratorSection
        where TSyntaxReceiver : ISyntaxContextReceiver
        where TData : new()
    {
        public GeneratorCache Cache { get; set; } = null!;
        internal TData Data { get; private set; }
        protected internal TPrevious Previous { get; private set; }

        private GeneratorExecutionContext? _context;

        IGeneratorSection IGeneratorSection.Previous
        {
            get => Previous;
            set => Previous = (TPrevious)value;
        }

        protected GeneratorSection()
        {
            Data = new();
            Previous = default!;
        }

        protected TSection GetPrevious<TSection>() where TSection : IGeneratorSection
        {
            IGeneratorSection parent = Previous;

            while (parent is not null)
            {
                if (parent is TSection section)
                {
                    return section;
                }

                parent = parent.Previous;
            }

            throw new TypeArgumentException(
                $"The section '{typeof(TSection).Name}' is not found in the parent hierarchy."
            );
        }

        protected abstract TData Execute(
            GeneratorExecutionContext context,
            TSyntaxReceiver syntaxReceiver,
            TPrevious previous
        );

        void IGeneratorSection.Execute(
            GeneratorExecutionContext context,
            ISyntaxContextReceiver syntaxReceiver
        )
        {
            _context = context;
            Data = Execute(context, (TSyntaxReceiver)syntaxReceiver, Previous);
        }

        protected void AddSource(string name, SourceText sourceText)
        {
            if (!_context.HasValue)
                throw new InvalidOperationException();

            _context.Value.AddSource(name + ".g.cs", sourceText);
        }
    }

    internal abstract class GeneratorSection<TPrevious, TSyntaxReceiver>
        : GeneratorSection<TPrevious, TSyntaxReceiver, NoData>
        where TPrevious : IGeneratorSection
        where TSyntaxReceiver : ISyntaxContextReceiver { }

    internal abstract class GeneratorSection<TPrevious>
        : GeneratorSection<TPrevious, NoReceiver, NoData> where TPrevious : IGeneratorSection { }

    internal interface IGeneratorSection<TPrevious, TSyntaxReceiver> : IGeneratorSection
        where TPrevious : IGeneratorSection
        where TSyntaxReceiver : ISyntaxContextReceiver { }

    internal interface IGeneratorSection
    {
        GeneratorCache Cache { get; set; }
        IGeneratorSection Previous { get; set; }

        void Execute(GeneratorExecutionContext context, ISyntaxContextReceiver syntaxReceiver);
    }
}
