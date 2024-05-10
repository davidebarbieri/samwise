// (c) Copyright 2022 Davide 'PeevishDave' Barbieri
using System.Collections.Generic;

namespace Peevo.Samwise
{
    public interface IDataRoot
    {
        IDataContext CreateData(IDialogueContext context);
        void ReleaseData(IDataContext data);
        IDataContext LookupDataContext(string contextName);
        IDataContext LookupOrCreateDataContext(string contextName);
        int GetRandom();
        void Clear();
        
        IEnumerable<(string, IDataContext)> GetDataContexes();
        IList<IDataContext> GetSubcontexes(IDataContext context);
    }
}
