// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    internal enum ConditionTokenId
    {
        RoundOpen,
        RoundClose,
        Or,
        And,
        Not,
        Add,
        Sub,
        Mul,
        Div,
        Equal,
        Different,
        LEqual,
        Less,
        GEqual,
        Greater,
        IntegerVar,
        BoolVar,
        SymbolVar,
        IntegerConst,
        SymbolConst,
        True,
        False,
        Once,
        External
    }
}