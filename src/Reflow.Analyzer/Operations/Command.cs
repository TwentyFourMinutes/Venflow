using Microsoft.CodeAnalysis;
using Reflow.Analyzer.Models;

namespace Reflow.Analyzer.Operations
{
    internal class Command
    {
        internal OperationType OperationType { get; }
        internal CommandType Type { get; }

        internal ITypeSymbol Entity { get; }

        internal MethodLocation? Location { get; set; }

        internal Command(OperationType operationType, CommandType type, ITypeSymbol entity)
        {
            OperationType = operationType;
            Type = type;
            Entity = entity;
        }

        internal enum CommandType : byte
        {
            Insert,
            Update,
            Delete
        }
    }
}
