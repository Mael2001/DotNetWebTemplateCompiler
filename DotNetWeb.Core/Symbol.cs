using DotNetWeb.Core.Expressions;

namespace DotNetWeb.Core
{
    public enum SymbolType
    {
        Variable
    }

    public class Symbol
    {
        public Symbol(SymbolType symbolType, Id id, dynamic value)
        {
            SymbolType = symbolType;
            Id = id;
            Value = value;
        }
        public SymbolType SymbolType { get; }
        public Id Id { get; }
        public dynamic Value { get; set; }
    }
}