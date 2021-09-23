using System;
using DotNetWeb.Core.Expressions;

namespace DotNetWeb.Core.Statements
{
    public class ForeachStatement:Statement
    {
        public ForeachStatement(Token token1, Token token2, Statement statement)
        {
            Token1 = token1;
            Token2 = token2;
            Statement = statement;
        }
        public Statement Statement { get; }
        public Token Token1 { get; }
        public Token Token2 { get; }
        public override void Interpret()
        {
            Statement?.Interpret();
        }

        public override void ValidateSemantic()
        {
            Statement?.ValidateSemantic();
        }

        public override string Generate(int tabs)
        {
            var code = Statement.Generate(tabs);
            code += Statement.Generate(tabs + 1);
            
            return code;
        }
    }
}