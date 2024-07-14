// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public interface IMultiCaseNode
    {
        int CasesCount { get; }
        ICase GetCase(int i);
    }
}