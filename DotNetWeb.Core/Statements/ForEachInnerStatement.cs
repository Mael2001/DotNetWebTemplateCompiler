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
            code += $"<div class=\"container\">{Expression.Evaluate()}</div>";
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