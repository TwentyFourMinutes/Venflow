﻿using System.Reflection;
using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Properties;

namespace Reflow.Analyzer
{
    [Generator(LanguageNames.CSharp)]
    public class SourceGenerator : ISourceGenerator, IGeneratorSection
    {
        private readonly RootData _rootData;

        IGeneratorSection IGeneratorSection.Previous
        {
            get => null!;
            set { }
        }

        public SourceGenerator()
        {
#if DEBUG
            // This is a temp fix to allow NuGet packages in DEBUG
            Assembly.LoadFile(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "../Reflow.Analyzer/bin/Debug/netstandard2.0/Microsoft.Bcl.HashCode.dll"
                )
            );
#endif

            var rootGenerator = typeof(SourceGenerator);
            var generatorSectionType = typeof(IGeneratorSection);

            var dict = new Dictionary<Type, Data>();
            var types = generatorSectionType.Assembly.GetTypes();

            for (var typeIndex = 0; typeIndex < types.Length; typeIndex++)
            {
                var sectionType = types[typeIndex];

                if (
                    !sectionType.IsClass
                    || sectionType.IsAbstract
                    || !generatorSectionType.IsAssignableFrom(sectionType)
                    || sectionType == rootGenerator
                )
                    continue;

                var typeArguments = sectionType
                    .GetInterface("IGeneratorSection`2")
                    .GetGenericArguments();

                dict.Add(sectionType, new Data(sectionType, typeArguments[1], typeArguments[0]));
            }

            _rootData = new RootData(this, dict.Values.ToArray());

            foreach (var data in dict.Values)
            {
                if (data.PreviousType == rootGenerator)
                {
                    data.Section.Previous = this;
                    _rootData.Subsequents.Add(data);
                }
                else
                {
                    var previous = dict[data.PreviousType];
                    data.Section.Previous = previous.Section;
                    previous.Subsequents.Add(data);
                }
            }
        }

        void ISourceGenerator.Initialize(GeneratorInitializationContext context)
        {
            var receiver = new SyntaxContextReceiver(_rootData);

            context.RegisterForSyntaxNotifications(
                () =>
                {
                    _rootData.Initialize();
                    return receiver;
                }
            );
        }

        void ISourceGenerator.Execute(GeneratorExecutionContext context)
        {
            context.Compilation.EnsureReference("Reflow", AssemblyInfo.PublicKey);

            _rootData.Execute(context);
        }

        void IGeneratorSection.Execute(
            GeneratorExecutionContext context,
            ISyntaxContextReceiver syntaxReceiver
        ) => throw new InvalidOperationException();

        private class SyntaxContextReceiver : ISyntaxContextReceiver
        {
            private readonly RootData _rootData;

            internal SyntaxContextReceiver(RootData rootData)
            {
                _rootData = rootData;
            }

            void ISyntaxContextReceiver.OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                _rootData.OnVisitSyntaxNode(context);
            }
        }
        private class RootData
        {
            public List<Data> Subsequents { get; }

            private readonly SourceGenerator _root;
            private readonly ISyntaxContextReceiver[] _receivers;
            private readonly IList<Data> _allSubsequets;

            internal RootData(SourceGenerator root, IList<Data> allSubsequets)
            {
                Subsequents = new();
                _root = root;
                _receivers = new ISyntaxContextReceiver[allSubsequets.Count];
                _allSubsequets = allSubsequets;
            }

            internal void Initialize()
            {
                for (var receiverIndex = 0; receiverIndex < _receivers.Length; receiverIndex++)
                {
                    _receivers[receiverIndex] = _allSubsequets[receiverIndex].Initialize();
                }
            }

            internal void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                for (var receiverIndex = 0; receiverIndex < _receivers.Length; receiverIndex++)
                {
                    _receivers[receiverIndex].OnVisitSyntaxNode(context);
                }
            }

            internal void Execute(GeneratorExecutionContext context)
            {
                for (
                    var subesquentIndex = 0;
                    subesquentIndex < Subsequents.Count;
                    subesquentIndex++
                )
                {
                    Subsequents[subesquentIndex].Execute(context);
                }
            }
        }

        private class Data
        {
            internal List<Data> Subsequents { get; }
            internal Type PreviousType { get; }
            internal IGeneratorSection Section { get; }

            private ISyntaxContextReceiver? _receiver;
            private readonly Type _receiverType;

            internal Data(Type sectionType, Type receiverType, Type previousType)
            {
                Subsequents = new();
                Section = (IGeneratorSection)Activator.CreateInstance(sectionType, true);
                _receiverType = receiverType;
                PreviousType = previousType;
            }

            internal ISyntaxContextReceiver Initialize()
            {
                return _receiver = (ISyntaxContextReceiver)Activator.CreateInstance(
                    _receiverType,
                    true
                );
            }

            internal void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                _receiver!.OnVisitSyntaxNode(context);
            }

            internal void Execute(GeneratorExecutionContext context)
            {
                Section.Execute(context, _receiver!);

                for (
                    var subesquentIndex = 0;
                    subesquentIndex < Subsequents.Count;
                    subesquentIndex++
                )
                {
                    Subsequents[subesquentIndex].Execute(context);
                }
            }
        }
    }
}
