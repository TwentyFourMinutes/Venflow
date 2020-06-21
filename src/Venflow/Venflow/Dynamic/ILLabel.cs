using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;
using Venflow.Enums;

namespace Venflow.Dynamic
{

    internal struct ILLabel : IILBaseInst
    {
        private readonly OpCode _opCode;
        private readonly Label _label;

        internal ILLabel(OpCode opCode, Label label)
        {
            _opCode = opCode;
            _label = label;
        }

        public void WriteIL(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(_opCode, _label);
        }
    }
}