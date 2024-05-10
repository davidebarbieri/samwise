// (c) Copyright 2024 Davide 'PeevishDave' Barbieri

namespace Peevo.Samwise
{
    public interface ITaggable
    {
        TagData TagData { get; set; }
    }

    public static class ITaggableMethods
    {
        public static bool HasTag(this ITaggable taggable, string tag)
        {
            if (taggable.TagData == null)
                return false;

            return taggable.TagData.HasTag(tag);
        }

        public static string GetID(this ITaggable taggable)
        {
            if (taggable.TagData == null)
                return null;

            return taggable.TagData.GetNamedTag("id");
        }

        public static void SetID(this ITaggable taggable, string id)
        {
            if (taggable.TagData == null)
                taggable.TagData = new TagData();

           taggable.TagData.SetNamedTag("id", id);
        }
    }
}