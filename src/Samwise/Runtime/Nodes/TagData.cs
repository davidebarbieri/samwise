// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public interface ITagData
    {
        bool HasTags();

        bool HasTag(string tag);
        
        // if value is null, tag is "name", otherwise is "name=value"
        void AddTag(string name, string value = null);

        bool RemoveTag(string name);

        string GetTagValue(string name);

        IEnumerable<(string Key, string Value)> GetTags();

        void Clear();
    }

    public class TagData : Dictionary<string, string>, ITagData
    {
        public bool HasTags()
        {
            return Count > 0;
        }

        public bool HasTag(string tag)
        {
            return ContainsKey(tag);
        }
        
        // if value is null, tag is "name", otherwise is "name=value"
        public void AddTag(string name, string value = null)
        {
            this[name] = value;
        }

        public bool RemoveTag(string name)
        {
            return Remove(name);
        }

        public string GetTagValue(string name)
        {
            return TryGetValue(name, out var value) ? value : null;
        }

        public IEnumerable<(string, string)> GetTags()
        {
            foreach (var i in this)
                yield return (i.Key, i.Value);
        }
    }
}