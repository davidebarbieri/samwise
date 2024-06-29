// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface ICase: IContent, ICheckable
    {
        IMultiCaseNode Parent { get; }
    }
}