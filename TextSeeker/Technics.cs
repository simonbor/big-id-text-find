using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextSeeker
{
    // ====================================================================
    // Other text find techincs implimentation by Strategy Pattern
    // ====================================================================

    public interface ITechnique
    {
        Dictionary<string, List<Offset>> FindNames(List<string> names, string text, int lineOffset);
        string GetTypeName();
    }

    // =======================================
    // Regular Expression Technique
    // =======================================

    public class RegexTechnique : ITechnique
    {
        public Dictionary<string, List<Offset>> FindNames(List<string> names, string text, int lineOffset)
        {
            var result = new Dictionary<string, List<Offset>>();

            names.ForEach(name =>
            {
                var offsets = new List<Offset>();
                var charOffsets = Regex.Matches(text, name).Select(m => m.Index).ToList();
                charOffsets.ForEach(charOffset => offsets.Add(new Offset { LineOffset = lineOffset, CharOffset = charOffset }));

                if (offsets.Count > 0)
                {
                    result.Add(name, offsets);
                }
            });

            return result;
        }

        public string GetTypeName()
        {
            return this.GetType().Name;
        }
    }

    // =======================================
    // IndexOf Technique
    // =======================================

    public class IndexOfTechnique : ITechnique
    {
        public Dictionary<string, List<Offset>> FindNames(List<string> names, string text, int lineOffset)
        {
            var result = new Dictionary<string, List<Offset>>();

            names.ForEach(name =>
            {
                var index = 0;
                var offsets = new List<Offset>();
                while ((index = text.IndexOf(name, index)) != -1)
                {
                    offsets.Add(new Offset { LineOffset = lineOffset, CharOffset = index });
                    index++;
                }

                if (offsets.Count > 0)
                {
                    result.Add(name, offsets);
                }
            });

            return result;
        }

        public string GetTypeName()
        {
            return this.GetType().Name;
        }
    }
}
