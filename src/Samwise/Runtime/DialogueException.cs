// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    [System.Serializable]
    public class DialogueException : System.Exception
    {
        public IDialogueContext Context;
        public DialogueException(IDialogueContext context, string message) : base(message) { Context = context; }
    }
}