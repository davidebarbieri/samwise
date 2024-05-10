// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IIntegerValue : IValue
    {
        long EvaluateInteger(IDialogueContext context);
    }
}