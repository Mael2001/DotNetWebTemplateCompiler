namespace DotNetWeb.Core.Statements
{
    public class SequenceStatement: Statement
    {
        public Statement Statement1 { get; }
        public Statement Statement2 { get; }

        public SequenceStatement(Statement statement1, Statement statement2)
        {
            Statement1 = statement1;
            Statement2 = statement2;
        }
        public override void ValidateSemantic()
        {
            Statement1?.ValidateSemantic();
            Statement2?.ValidateSemantic();
        }

        public override string Generate(int tabs)
        {
            throw new System.NotImplementedException();
        }

        public override void Interpret()
        {
            Statement1?.Interpret();
            Statement2?.Interpret();
        }
    }
}