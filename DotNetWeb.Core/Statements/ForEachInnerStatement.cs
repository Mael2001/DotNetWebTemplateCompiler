using System;
using DotNetWeb.Core.Expressions;

namespace DotNetWeb.Core.Statements
{
    public class ForEachInnerStatement: Statement
    {
        public ForEachInnerStatement(TypedExpression expression)
        {
            Expression = expression;
        }
        
        public TypedExpression Expression { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"{Expression.Evaluate()}";
            return code;
        }

        public override void Interpret()
        {

        }

        public override void ValidateSemantic()
        {

        }
    }
}