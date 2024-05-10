// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    // Optional external code parser
    public interface IExternalCodeParser
    {
        bool Parse(string code, out IStatement statement, System.Action<string> logError);
        bool ParseCondition(string code, out IBoolValue expression, System.Action<string> logError);
        bool ParseAsync(string code, out IAsyncCode asyncCode, System.Action<string> logError);
    }
}