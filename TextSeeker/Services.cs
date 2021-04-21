using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TextSeeker
{
    // ========================================
    // Interfaces
    // ========================================

    public interface IDataService
    {
        Task<string> GetText(string uri);
        List<string> DivideByParts(string text, int partSize);
    }

    public interface IMatcherService
    {
        Dictionary<string, List<Offset>> Find(List<string> names, string part, int lineOffset);
    }

    public interface IAggregatorService
    {
        void Add(Dictionary<string, List<Offset>> offsets);
        void Print();
    }

    // =======================================
    // Data Text Service
    // =======================================

    public class DataFeed : IDataService
    {
        static readonly HttpClient client = new HttpClient();

        public List<string> DivideByParts(string text, int partSize)
        {
            List<string> parts = new List<string>();
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            for (var i = 0; i < (lines.Length + partSize); i += partSize)
            {
                var part = lines.Skip(i).Take(partSize).ToArray();
                if (part.Length > 0)
                {
                    parts.Add(string.Join("\r\n", part));
                }
            }

            return parts;
        }

        public async Task<string> GetText(string uri)
        {
            string responseBody = "";

            try
            {
                var result = await client.GetAsync(uri);
                using var streamReader = new StreamReader(await result.Content.ReadAsStreamAsync());
                responseBody = streamReader.ReadToEnd();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("Exception Caught - Message :{0} ", e.Message);
            }

            return responseBody;
        }
    }

    // =======================================
    // Matcher Service
    // =======================================

    public class Matcher : IMatcherService
    {
        readonly ITechnique _technique;

        public Matcher(ITechnique technique)
        {
            _technique = technique;
        }

        public Dictionary<string, List<Offset>> Find(List<string> names, string part, int lineOffset)
        {
            return _technique.FindNames(names, part, lineOffset);
        }
    }

    // =======================================
    // Aggregator Service
    // =======================================

    public class Aggregator : IAggregatorService
    {
        Dictionary<string, List<Offset>> total = new Dictionary<string, List<Offset>>();
        private readonly object _lock = new object();

        public void Add(Dictionary<string, List<Offset>> offsets)
        {
            lock (_lock)
            {
                foreach (var item in offsets)
                {
                    if (total.ContainsKey(item.Key))
                    {
                        item.Value.ForEach(offset => total[item.Key].Add(offset));
                    }
                    else
                    {
                        total.Add(item.Key, item.Value);
                    }
                }
            }
        }

        public void Print()
        {
            foreach (var item in total)
            {
                Console.WriteLine($"{item.Key} --> {JsonSerializer.Serialize(item.Value)}");
            }
        }
    }
}
