// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IBoolValue: IValue
    {
        bool EvaluateBool(IDialogueContext context);
        void OnVisited(IDialogueContext context);
    }
}