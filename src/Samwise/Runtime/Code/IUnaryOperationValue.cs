// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IUnaryOperationBoolValue : IBoolValue
    {
        IBoolValue A { get; set; }
    }

    public interface IUnaryOperationIntegerValue : IIntegerValue
    {
        IIntegerValue A { get; set; }
    }
}