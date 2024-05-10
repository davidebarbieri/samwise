// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public enum NextBlockPolicy
    {
        ParentNext, // next node is parent's next (default)
        ReturnToParent, // next node is this block's parent node (e.g. <-)
        End // there's no next node (e.g. anonymous blocks)
    }
}