// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IBinaryOperationValue : IValue
    {
        IBinaryOperationValue Overload(IValue a, IValue b);
    }
 
    public interface IBinaryOperationBoolValue : IBinaryOperationValue
    {
        IBoolValue A { get; set; }
        IBoolValue B { get; set; }
    }

    public interface IBinaryOperationIntegerValue : IBinaryOperationValue
    {
        IIntegerValue A { get; set; }
        IIntegerValue B { get; set; }
    }

    public interface IBinaryOperationSymbolValue : IBinaryOperationValue
    {
        ISymbolValue A { get; set; }
        ISymbolValue B { get; set; }
    }
}