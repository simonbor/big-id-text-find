using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TextSeeker
{
    // ========================================
    // Interfaces
    // ========================================

    public interface IDataService
    {
        Task<string> GetText(string uri);
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
    // Data Text Feed Serice
    // =======================================

    public class DataFeed : IDataService
    {
        static readonly HttpClient client = new HttpClient();

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
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
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
            var result = _technique.FindNames(names, part, lineOffset);

            //var thread = Thread.CurrentThread;
            //Console.WriteLine($"Thread ID: {thread.ManagedThreadId}, part length: {part.Length};");

            return result;
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
                if (item.Value.Count > 0)
                {
                    //Console.WriteLine($"{item.Key} --> {JsonSerializer.Serialize(item.Value)}");
                    Console.WriteLine($"{item.Key} --> {item.Value.Count} --> {JsonSerializer.Serialize(item.Value)}");
                }
            }
        }
    }
}
