// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface IDialogueContext
    {
        DialogueStatus Status { get; }
        long Uid { get; }

        IDialogueNode Current { get; }
        IDialogueContext Parent { get; }

        bool IsEnded { get; }

        bool Advance();
        bool Choose(Option choice);
        void CompleteChallenge(bool passed);
        void Stop();
        bool TryResolveMissingDialogues();
        
        IDataRoot DataRoot { get; }
        IDataContext DataContext { get; }
        object ExternalContext { get; }
    }

    public static class IDialogueContextMethods
    {
        public static IDialogueContext Root(this IDialogueContext context)
        {
            var c = context;

            while (c.Parent != null)
                c = c.Parent;

            return c;
        }
        public static IDataContext LookupDataContext(this IDialogueContext context, string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                return context.DataContext; // local
            return context.DataRoot.LookupDataContext(contextName); // global
        }

        public static IDataContext LookupOrCreateDataContext(this IDialogueContext context, string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
                return context.DataContext; // local
            return context.DataRoot.LookupOrCreateDataContext(contextName); // global
        }

        public static int GetRandom(this IDialogueContext context)
        {
            return context.DataRoot.GetRandom();
        }
    }
}