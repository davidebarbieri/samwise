// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IExternalContextSaveResolver
    {
        string GetUIDFromObject(object context);
    }

    public interface IExternalContextLoadResolver
    {
        object GetObjectFromUID(string uid);
    }
}