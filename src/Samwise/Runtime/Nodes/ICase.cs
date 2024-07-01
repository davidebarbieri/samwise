// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface ICase
    {
        IMultiCaseNode Parent { get; }

        int ContentCount { get; }
        IContent GetContent(int index);

    }
}