// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public enum Symbol
    {
        Invalid,
        Fallback, // ?
        Loop, // >>
        Clamp, // >>>
        PingPong, // ><
        Score, // %
        Choice, // :
        Says, // >
        Fork, // =>
        Join, // <=
        Await, // <=>
        Cancel, // <!=
        Check, // $
        Interrupt, // !
        ResetAndInterrupt, // !!
    }

    public static class SymbolExtensions
    {
        public static string ToSymbolString(this Symbol symbol)
        {
            switch (symbol)
            {
                default:
                    return "--Invalid--";
                case Symbol.Fallback:
                    return "?";
                case Symbol.Score:
                    return "%";
                case Symbol.Loop:
                    return ">>";
                case Symbol.Clamp:
                    return ">>>";
                case Symbol.PingPong:
                    return "><";
                case Symbol.Choice:
                    return ":";
                case Symbol.Says:
                    return ">";
                case Symbol.Fork:
                    return "=>";
                case Symbol.Join:
                    return "<=";
                case Symbol.Cancel:
                    return "<!=";
                case Symbol.Await:
                    return "<=>";
                case Symbol.Check:
                    return "$";
                case Symbol.Interrupt:
                    return "!";
                case Symbol.ResetAndInterrupt:
                    return "!!";
            }
        }
    }
}