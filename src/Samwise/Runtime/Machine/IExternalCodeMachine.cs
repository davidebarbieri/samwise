// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IExternalCodeMachine
    {
        void Dispatch(IExternalCodeContext context, IAsyncCode asyncCode);
    }
}