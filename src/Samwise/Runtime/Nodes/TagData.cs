// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

using System.Collections.Generic;

namespace Peevo.Samwise
{
    public class TagData
    {
        public List<string> Tags { get; private set; }
        public List<string> Comments { get; private set; }

        public bool HasData()
        {
            return (Tags != null && Tags.Count > 0) ||
                (Comments != null && Comments.Count > 0) ||
                (namedTags != null && namedTags.Count > 0) ||
                (namedComments != null && namedComments.Count > 0);
        }

        public bool HasTag(string tag)
        {
            if (Tags == null)
                return false;
            
            return Tags.Contains(tag);
        }

        public void AddComment(string comment)
        {
            if (Comments == null)
                Comments = new List<string>();

            Comments.Add(comment);
        }

        public void AddTag(string tag)
        {
            if (Tags == null)
                Tags = new List<string>();

            Tags.Add(tag);
        }

        public bool RemoveTag(string tag)
        {
            if (Tags != null)
            {
                return Tags.Remove(tag);
            }
            return false;
        }

        public string GetNamedTag(string name)
        {
            if (namedTags == null)
                return null;

            return namedTags.TryGetValue(name, out var value) ? value : null;
        }

        // if value is null, then tag is removed
        public void SetNamedTag(string name, string value)
        {
            if (value == null)
            {
                RemoveNamedTag(name);
                return;
            }

            if (namedTags == null)
                namedTags = new Dictionary<string, string>();

            namedTags[name] = value;
        }

        public bool RemoveNamedTag(string name)
        {
            if (namedTags != null)
                return namedTags.Remove(name);
            return false;
        }

        public string GetNamedComment(string name)
        {
            if (namedComments == null)
                return null;

            return namedComments.TryGetValue(name, out var value) ? value : null;
        }

        // if value is null, then tag is removed
        public void SetNamedComment(string name, string value)
        {
            if (namedComments == null)
                namedComments = new Dictionary<string, string>();

            namedComments[name] = value;
        }

        public IEnumerable<(string, string)> GetNamedTags()
        {
            if (namedTags == null)
                yield break;

            foreach (var i in namedTags)
                yield return (i.Key, i.Value);
        }

        public IEnumerable<(string, string)> GetNamedComments()
        {
            if (namedComments == null)
                yield break;

            foreach (var i in namedComments)
                yield return (i.Key, i.Value);
        }

        public void Clear()
        {
            Tags = null;
            namedTags = null;
            Comments = null;
            namedComments = null;
        }

        Dictionary<string, string> namedTags;
        Dictionary<string, string> namedComments;
    }
}