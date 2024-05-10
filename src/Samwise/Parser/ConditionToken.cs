// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    internal struct ConditionToken
    {
        public ConditionTokenId id;
        public int position;
        public int length;

        public string GetText(string fullText) => fullText.Substring(position, length);
    }
}