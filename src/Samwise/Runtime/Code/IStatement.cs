// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IStatement
    {
        void Execute(IDialogueContext context);
    }
}