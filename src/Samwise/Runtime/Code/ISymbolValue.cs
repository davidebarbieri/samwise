// (c) Copyright 2023 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface ISymbolValue : IValue
    {
        string EvaluateSymbol(IDialogueContext context);
    }
}