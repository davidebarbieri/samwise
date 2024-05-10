// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    internal class LocalDataContext : DataContext
    {
        public event System.Action<IDialogueContext, string, bool, bool> onLocalBoolDataChanged;
        public event System.Action<IDialogueContext, string, long, long> onLocalIntDataChanged;
        public event System.Action<IDialogueContext, string, string, string> onLocalSymbolDataChanged;
        public event System.Action<IDialogueContext, string> onLocalDataClear;
        public event System.Action<IDialogueContext> onLocalClear;

        internal IDialogueContext DialogueContext;

        internal LocalDataContext()
        {
            onBoolDataChanged += OnBoolDataChanged;
            onIntDataChanged += OnIntDataChanged;
            onSymbolDataChanged += OnSymbolDataChanged;
            onDataClear += OnDataClear;
            onClear += OnClear;
        }

        void OnClear()
        {
            // Fire only if the dialogue is running
            //if (!DialogueContext.IsEnded)
            onLocalClear?.Invoke(DialogueContext);
        }

        private void OnSymbolDataChanged(string name, string prevValue, string newValue)
        {
            // Fire only if the dialogue is running
            if (!DialogueContext.IsEnded)
                onLocalSymbolDataChanged?.Invoke(DialogueContext, name, prevValue, newValue);
        }

        private void OnIntDataChanged(string name, long prevValue, long newValue)
        {
            // Fire only if the dialogue is running
            if (!DialogueContext.IsEnded)
                onLocalIntDataChanged?.Invoke(DialogueContext, name, prevValue, newValue);
        }

        private void OnBoolDataChanged(string name, bool prevValue, bool newValue)
        {
            // Fire only if the dialogue is running
            if (!DialogueContext.IsEnded)
                onLocalBoolDataChanged?.Invoke(DialogueContext, name, prevValue, newValue);
        }

        private void OnDataClear(string name)
        {
            // Fire only if the dialogue is running
            if (!DialogueContext.IsEnded)
                onLocalDataClear?.Invoke(DialogueContext, name);
        }
    }
}