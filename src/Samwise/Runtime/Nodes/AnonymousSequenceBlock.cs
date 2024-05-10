// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
        public class AnonymousSequenceBlock : SequenceBlock
        {
            public override NextBlockPolicy NextBlockPolicy => NextBlockPolicy.End;

            public AnonymousSequenceBlock(IBlockContainerNode parent) : base(parent) {}
        }
}