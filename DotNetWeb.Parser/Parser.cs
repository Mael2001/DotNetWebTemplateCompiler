using DotNetWeb.Core;
using DotNetWeb.Core.Interfaces;
using System;
using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Statements;
using Constant = DotNetWeb.Core.Expressions.Constant;
using Expression = DotNetWeb.Core.Expressions.Expression;
using Type = DotNetWeb.Core.Type;

namespace DotNetWeb.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner _scanner;
        private Token _lookAhead;
        public Parser(IScanner scanner)
        {
            this._scanner = scanner;
            this.Move();
        }
        public void Parse()
        {
            EnvironmentManager.PushContext();
            var program = Program();
            program.ValidateSemantic();
            program.Interpret();
            var code = "<html>";
            code += "<header>";
            code += "<!-- CSS only -->" +
                    "<link href = \"https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/css/bootstrap.min.css\" rel = \"stylesheet\" integrity = \"sha384-F3w7mX95PdgyTmZZMECAngseQB83DfGTowi0iMjiWaeVhAn4FJkqJByhZMI3AhiU\" crossorigin = \"anonymous\"> ";
            code += "<!-- JavaScript Bundle with Popper -->" +
                    "<script src = \"https://cdn.jsdelivr.net/npm/bootstrap@5.1.1/dist/js/bootstrap.bundle.min.js\" integrity = \"sha384-/bQdsTh/da6pkI1MST/rWKFNjaCP5gBSY4sEBT38Q/9RBh9AH40zEOg7Hlq2THRZ\" crossorigin = \"anonymous\" ></script> ";
            code += "</header>";
            code += "<body class=\"container\">";
            code += "<nav class=\"navbar navbar-expand - lg navbar - light bg - light\">" +
                    "<a class=\"navbar-brand\" href=\"#\">Navbar</a>" +
                    "<button class=\"navbar-toggler\" type=\"button\" data-toggle=\"collapse\" data-target=\"#navbarSupportedContent\" aria-controls=\"navbarSupportedContent\" aria-expanded=\"false\" aria-label=\"Toggle navigation\">" +
                    "<span class=\"navbar-toggler-icon\"></span></button>" +
                    "<div class=\"collapse navbar-collapse\" id=\"navbarSupportedContent\"><ul class=\"navbar-nav mr-auto\">" +
                    "<li class=\"nav-item active\">" +
                    "<a class=\"nav-link\" href=\"#\">Home<span class=\"sr-only\">(current)</span></a></li>" +
                    "<li class=\"nav-item\"><a class=\"nav-link\" href=\"#\">Link</a></li><li class=\"nav-item dropdown\">" +
                    "<a class=\"nav-link dropdown-toggle\" href=\"#\" id=\"navbarDropdown\" role=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">Dropdown</a>" +
                    "<div class=\"dropdown-menu\" aria-labelledby=\"navbarDropdown\">" +
                    "<a class=\"dropdown-item\" href=\"#\">Action</a>" +
                    "<a class=\"dropdown-item\" href=\"#\">Another action</a>" +
                    "<div class=\"dropdown-divider\"></div>" +
                    "<a class=\"dropdown-item\" href=\"#\">Something else here</a></div></li>" +
                    "<li class=\"nav-item\"><a class=\"nav-link disabled\" href=\"#\">Disabled</a></li></ul>" +
                    "<form class=\"form-inline my-2 my-lg-0\">" +
                    "<input class=\"form-control mr-sm-2\" type=\"search\" placeholder=\"Search\" aria-label=\"Search\"></nav>";
            code += "<div =\"container\">";
            code += program.Generate(1);
            code += "</div>";
            code += "</body>";
            code += "</html>";
            Console.WriteLine(program.Generate(1));
            System.IO.File.WriteAllText(@"C:\Users\Public\code.html", code);
            EnvironmentManager.PopContext();

        }

        private Statement Program()
        {
            return new SequenceStatement(Init(), Template());
        }

        private Statement Template()
        {
            return new SequenceStatement(Tag(), InnerTemplate());
        }
        
        private Statement InnerTemplate()
        {
            if (this._lookAhead.TokenType == TokenType.LessThan)
            {
                return Template();
            }

            return null;
        }
        private Statement Tag()
        {
            Match(TokenType.LessThan);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            var statement = Stmts();
            Match(TokenType.LessThan);
            Match(TokenType.Slash);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            return statement;
        }

        private Statement Stmts()
        {
            if (this._lookAhead.TokenType == TokenType.OpenBrace)
            {
                return new SequenceStatement(Stmt(), Stmts());
            }
            return null;
        }

        private Statement Stmt()
        {
            Expression expression;
            Match(TokenType.OpenBrace);
            switch (this._lookAhead.TokenType)
            {
                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    expression = Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    return new ForEachInnerStatement( expression as TypedExpression);
                case TokenType.Percentage:
                    return IfStmt();
                case TokenType.Hyphen:
                    return ForeachStatement();
                default:
                    throw new ApplicationException("Unrecognized statement");
            }
        }

        private Statement ForeachStatement()
        {
            Match(TokenType.Hyphen);
            Match(TokenType.Percentage);
            Match(TokenType.ForEeachKeyword);
            
            var token1 = _lookAhead;
            Match(TokenType.Identifier);
            var id1 = new Id(token1, Type.FloatList);
            EnvironmentManager.AddVariable(token1.Lexeme, id1);

            Match(TokenType.InKeyword);
            var token2 = _lookAhead;
            Match(TokenType.Identifier);
            if (EnvironmentManager.GetSymbolForEvaluation(token2.Lexeme)==null)
            {
                throw new ApplicationException($"Variable {token2.Lexeme} Doesn't Exist");
            }

            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            var statement = Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndForEachKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return new ForeachStatement(token1,token2,statement);
        }

        private Statement IfStmt()
        {
            Match(TokenType.Percentage);
            Match(TokenType.IfKeyword);
            var expression = Eq();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            var statement1 = Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndIfKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return new IfStatement(expression as TypedExpression, statement1);
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this._lookAhead.TokenType == TokenType.Equal || this._lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = _lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }

            return expression;
        }

        private Expression Rel()
        {
            var expression = Expr();
            if (this._lookAhead.TokenType == TokenType.LessThan
                || this._lookAhead.TokenType == TokenType.GreaterThan)
            {
                var token = _lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }

            return expression;
        }

        private Expression Expr()
        {
            var factors = Term();
            while (this._lookAhead.TokenType == TokenType.Plus || this._lookAhead.TokenType == TokenType.Hyphen)
            {
                var token = _lookAhead;
                Move();
                factors = new ArithmeticOperator(token, factors as TypedExpression, Factor() as TypedExpression);
            }

            return factors;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this._lookAhead.TokenType == TokenType.Asterisk || this._lookAhead.TokenType == TokenType.Slash)
            {
                var token = _lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this._lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Eq();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(_lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(_lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(_lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.OpenBracket:
                    Match(TokenType.OpenBracket);
                    var compression = ExprList();
                    Match(TokenType.CloseBracket);
                    return compression;
                default:
                    var symbol = EnvironmentManager.GetSymbol(this._lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
            }
            
        }

        private Expression ExprList()
        {
            var expression = Eq();
            if (this._lookAhead.TokenType != TokenType.Comma)
            {
                return expression;
            }
            Match(TokenType.Comma);
            return new SequenceExpression(expression, ExprList());
        }

        private Statement Init()
        {
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.InitKeyword);
            var statements = Code();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return statements;
        }

        private Statement Code()
        {
            Decls();
            return Assignations();
        }

        private Statement Assignations()
        {
            if (this._lookAhead.TokenType == TokenType.Identifier)
            {
                var symbol = EnvironmentManager.GetSymbol(this._lookAhead.Lexeme);
                return new SequenceStatement(Assignation(symbol.Id), Assignations());
            }

            return null;
        }

        private Statement Assignation(Id id)
        {
            Match(TokenType.Identifier);
            Match(TokenType.Assignation);
            var expression = Eq();
            Match(TokenType.SemiColon);
            if (expression is SequenceExpression)
            {
                //return ListAssign(expression as SequenceExpression);
            }
            return new AssignationStatement(id, expression as TypedExpression);
        }

       /* private Statement ListAssign(SequenceExpression expression)
        {
            return new SequenceStatement(
                new AssignationStatement(new Id(expression.Token, expression.Type),
                    expression.Expression1 as TypedExpression),
                new AssignationStatement(new Id(expression.Token, expression.Type),
                    expression.Expression2 as TypedExpression));
        }*/
        private void Decls()
        {
            Decl();
            InnerDecls();
        }

        private void InnerDecls()
        {
            if (this.LookAheadIsType())
            {
                Decls();
            }
        }

        private void Decl()
        {
            switch (this._lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = _lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = _lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntKeyword:
                    Match(TokenType.IntKeyword);
                    token = _lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.FloatListKeyword:
                    Match(TokenType.FloatListKeyword);
                    token = _lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.FloatList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntListKeyword:
                    Match(TokenType.IntListKeyword);
                    token = _lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.IntList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringListKeyword:
                    Match(TokenType.StringListKeyword);
                    token = _lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.StringList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                default:
                    throw new ApplicationException($"Unsupported type {this._lookAhead.Lexeme}");
            }
        }

        private void Move()
        {
            this._lookAhead = this._scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this._lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this._lookAhead.TokenType}. Line: {this._lookAhead.Line}, Column: {this._lookAhead.Column}");
            }
            this.Move();
        }

        private bool LookAheadIsType()
        {
            return this._lookAhead.TokenType == TokenType.IntKeyword ||
                this._lookAhead.TokenType == TokenType.StringKeyword ||
                this._lookAhead.TokenType == TokenType.FloatKeyword ||
                this._lookAhead.TokenType == TokenType.IntListKeyword ||
                this._lookAhead.TokenType == TokenType.FloatListKeyword ||
                this._lookAhead.TokenType == TokenType.StringListKeyword;

        }
    }
}
