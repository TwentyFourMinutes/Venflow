using System.Data.Common;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Reflow.Analyzer.CodeGenerator;
using Reflow.Analyzer.Operations;
using static Reflow.Analyzer.CodeGenerator.CSharpCodeGenerator;

namespace Reflow.Analyzer.Emitters
{
    internal static class CommandLinksEmitter
    {
        internal static SourceText Emit(IList<Command> commands, Command.CommandType commandType)
        {
            _ = commandType;
            var links = new InitializerExpressionSyntax[commands.Count];

            for (var commandIndex = 0; commandIndex < commands.Count; commandIndex++)
            {
                var command = commands[commandIndex];

                var operationType = command.OperationType.HasFlag(OperationType.Single)
                  ? Type(command.Entity)
                  : GenericType(typeof(IList<>), Type(command.Entity));

                links[commandIndex] = DictionaryEntry(
                    TypeOf(operationType),
                    Cast(
                        GenericType(
                            typeof(Func<,,>),
                            Type<DbCommand>(),
                            operationType,
                            Type<Task>()
                        ),
                        AccessMember(
                            Type(command.Location!.FullTypeName),
                            command.Location!.MethodName
                        )
                    )
                );
            }

            return File("Reflow")
                .WithMembers(
                    Class("CommandLinks", CSharpModifiers.Public | CSharpModifiers.Static)
                        .WithMembers(
                            Field(
                                    "Links",
                                    DictionaryType(),
                                    CSharpModifiers.Public | CSharpModifiers.Static
                                )
                                .WithInitializer(
                                    Instance(DictionaryType())
                                        .WithArguments(Constant(commands.Count))
                                        .WithInitializer(DictionaryInitializer(links))
                                )
                        )
                )
                .GetText();

            static TypeSyntax DictionaryType() =>
                GenericType(typeof(Dictionary<,>), Type<Type>(), Type<Delegate>());
        }
    }
}
