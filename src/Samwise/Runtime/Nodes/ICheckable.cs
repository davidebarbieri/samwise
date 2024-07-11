// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface ICheckable
    {
        bool HasCheck(out bool isPreCheck, out string checkName);
    }
}