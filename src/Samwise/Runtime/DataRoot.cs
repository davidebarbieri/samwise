// (c) Copyright 2022 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    // Sample IDataRoot implementation
    public class DataRoot : IDataRoot
    {
        // context, name, value
        public event System.Action<string, string, bool, bool> onBoolDataChanged;
        public event System.Action<string, string, long, long> onIntDataChanged;
        public event System.Action<string, string, string, string> onSymbolDataChanged;
        public event System.Action<string, string> onDataClear;
        public event System.Action<string> onContextClear;

        public event System.Action<IDialogueContext, string, bool, bool> onLocalBoolDataChanged;
        public event System.Action<IDialogueContext, string, long, long> onLocalIntDataChanged;
        public event System.Action<IDialogueContext, string, string, string> onLocalSymbolDataChanged;
        public event System.Action<IDialogueContext, string> onLocalDataClear;
        public event System.Action<IDialogueContext> onLocalContextClear;

        public int GetRandom()
        {
            return random.Next();
        }

        public IDataContext CreateData(IDialogueContext context)
        {
            LocalDataContext localContext;

            if (contextPool.Count > 0)
            {
                localContext = contextPool.Pop();
                localContext.DialogueContext = context;
            }
            else
            {
                localContext = AllocateLocalData(context);
            }


            contexes.Add(localContext);
            return localContext;
        }

        LocalDataContext AllocateLocalData(IDialogueContext context)
        {
            LocalDataContext localContext = new LocalDataContext
            {
                DialogueContext = context
            };

            localContext.onLocalBoolDataChanged += (c, name, prevValue, value) => onLocalBoolDataChanged?.Invoke(c, name, prevValue, value);
            localContext.onLocalIntDataChanged += (c, name, prevValue, value) => onLocalIntDataChanged?.Invoke(c, name, prevValue, value);
            localContext.onLocalSymbolDataChanged += (c, name, prevValue, value) => onLocalSymbolDataChanged?.Invoke(c, name, prevValue, value);
            localContext.onLocalDataClear += (c, name) => onLocalDataClear?.Invoke(c, name);
            localContext.onLocalClear += (c) => onLocalContextClear?.Invoke(c);

            return localContext;
        }

        public void ReleaseData(IDataContext data)
        {
            data.Clear();
            contexes.Remove(data);
            contextPool.Push((LocalDataContext)data);
        }

        public void Clear()
        {
            // Clear local contexes, but do not remove as they are still running
            for (int i=0, len=contexes.Count; i<len; ++i)
            {
                contexes[i].Clear();
            }

            namedContexes.Clear();
            aliasedContexes.Clear();
            subContexes.Clear();
        }

        public IDataContext LookupDataContext(string contextName)
        {
            if (!aliasedContexes.TryGetValue(contextName, out var data))
            {
                return null;
            }
            return data;
        }

        public IList<IDataContext> GetSubcontexes(IDataContext context)
        {
            return subContexes.TryGetValue(context, out var children) ? children : null;
        }

        public IDataContext LookupOrCreateDataContext(string contextName)
        {
            if (!aliasedContexes.TryGetValue(contextName, out var data))
            {
                data = CreateContext(contextName);
            }
            return data;
        }

        IDataContext CreateContext(string contextName)
        {
            // Create parent first
            IDataContext parent = null;

            if (contextName.Length == 0 || (contextName.Length == 1 && contextName[0] == '.'))
            {
                // root!
                
            }
            else
            {
                // has parent
                var idx = contextName.LastIndexOf('.', contextName.Length - 1, contextName.Length); 

                if (idx == contextName.Length - 1) // leading .
                    idx = contextName.LastIndexOf('.', contextName.Length - 2, contextName.Length - 1); 

                if (idx <= 0)
                    parent = LookupOrCreateDataContext(".");
                else
                    parent = LookupOrCreateDataContext(contextName.Substring(0, idx + 1));
            }

            DataContext newDataContext =  new DataContext();

            // register both aliases (eg ctx and .ctx)
            if (!contextName.StartsWith("."))
            {
                aliasedContexes[contextName] = newDataContext; // without '.'
                contextName = "." + contextName;
            }
            else
            {
                aliasedContexes[contextName.Substring(1)] = newDataContext; // without '.'
            }

            newDataContext.Name = contextName;
            
            aliasedContexes[contextName] = newDataContext;   // with '.'
            namedContexes[contextName] = newDataContext; // save the one with "."

            newDataContext.onBoolDataChanged += (name, prevValue, value) => onBoolDataChanged?.Invoke(contextName, name, prevValue, value);
            newDataContext.onIntDataChanged += (name, prevValue, value) => onIntDataChanged?.Invoke(contextName, name, prevValue, value);
            newDataContext.onSymbolDataChanged += (name, prevValue, value) => onSymbolDataChanged?.Invoke(contextName, name, prevValue, value);
            newDataContext.onDataClear += (name) => onDataClear?.Invoke(contextName, name);
            newDataContext.onClear += () => OnContextClear(newDataContext);

            contexes.Add(newDataContext);

            subContexes[newDataContext] = new List<IDataContext>();

            if (parent != null)
            {
                subContexes[parent].Add(newDataContext);
            }

            return newDataContext;
        }

        void OnContextClear(DataContext context)
        {
            // Clear all sub-contexes
            if (subContexes.TryGetValue(context, out var children))
            {
                for (int i=0, count=children.Count; i<count; ++i)
                {
                    children[i].Clear();
                }
            }

            var contextName = context.Name;
            onContextClear?.Invoke(contextName);

            namedContexes.Remove(contextName);
            aliasedContexes.Remove(contextName);
            aliasedContexes.Remove(contextName.Substring(1));
        }
        
        public IEnumerable<(string, IDataContext)> GetDataContexes()
        {
            foreach (var v in namedContexes)
            {
                yield return (v.Key, v.Value);
            }
        }

        Dictionary<string, IDataContext> namedContexes = new Dictionary<string, IDataContext>();
        Dictionary<string, IDataContext> aliasedContexes = new Dictionary<string, IDataContext>();
        List<IDataContext> contexes = new List<IDataContext>();
        Stack<LocalDataContext> contextPool = new Stack<LocalDataContext>();
        
        Dictionary<IDataContext, List<IDataContext>> subContexes = new Dictionary<IDataContext, List<IDataContext>>();

        // Todo: use seed
        System.Random random = new System.Random();
    }
}
