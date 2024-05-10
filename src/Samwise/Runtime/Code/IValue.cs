// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IValue
    {
        void Traverse(System.Action<IValue> action);
    }
}