// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public class ExitNode : LocalGotoNode
    {
        public ExitNode(int sourceLine) : base(sourceLine, null, null) {}
        
        public override string PrintPayload()
        {
            return "-> end";
        }
    }
} 