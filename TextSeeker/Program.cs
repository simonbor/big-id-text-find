using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TextSeeker
{
    class Program
    {
        static readonly Stopwatch stopwatch = new Stopwatch();
        static readonly IDataService dataService = new DataFeed();
        //static readonly string url = "http://norvig.com/big.txt";
        //static readonly string url = "http://lib.ru/PRIKL/SHPANOW/uchenik_charodeya.txt"; // russian ~1MB
        static readonly string url = "http://lib.ru/ADAMS/liff.txt"; // english ~0.2mb

        // configuration values
        const string NAMES = @"James,John,Robert,Michael,William,David,Richard,Charles,Joseph,Thomas,Christopher,Daniel,Paul,Mark,Donald,George,Kenneth,Steven,Edward,Brian,Ronald,Anthony,Kevin,Jason,Matthew,Gary,Timothy,Jose,Larry,Jeffrey,Frank,Scott,Eric,Stephen,Andrew,Raymond,Gregory,Joshua,Jerry,Dennis,Walter,Patrick,Peter,Harold,Douglas,Henry,Carl,Arthur,Ryan,Roger";
        const int PART = 1000;

        // text find approches
        //static readonly ITechnique typeTechnique = new IndexOfTechnique();
        static readonly ITechnique typeTechnique = new RegexTechnique();

        static async Task Main(string[] args)
        {
            Console.WriteLine("START");
            stopwatch.Start();
            var text = await dataService.GetText(url);

            stopwatch.Stop();
            Console.WriteLine($"big.txt is {text.Length} bytes and was downloaded in {(int)stopwatch.Elapsed.TotalSeconds} seconds");

            stopwatch.Restart();
            var lines = text.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None );
            Console.WriteLine($"big.txt composed from {lines.Length} lines and SPLIT operation took {(int)stopwatch.Elapsed.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            List<string> parts = new List<string>();
            for(var i=0; i<(lines.Length + PART); i+=PART)
            {
                var part = lines.Skip(i).Take(PART).ToArray();
                if (part.Length > 0)
                {
                    parts.Add(string.Join("\r\n", part));
                }
            }
            Console.WriteLine($"big.txt composed from {parts.Count} parts and JOIN operation took {(int)stopwatch.Elapsed.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            var aggregator = new Aggregator();
            int toProcess = parts.Count;
            List<string> names = NAMES.Trim(' ').Split(',').ToList();
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                var partsArr = parts.ToArray();
                var matcher = new Matcher(typeTechnique);

                for(var i=0; i < partsArr.Length; i++) {
                    var line = i;
                    ThreadPool.QueueUserWorkItem(state => 
                    {
                        var result  = matcher.Find(names, partsArr[line], line);
                        aggregator.Add(result);

                        if (Interlocked.Decrement(ref toProcess) == 0)
                        {
                            resetEvent.Set();
                        }
                    });
                };

                resetEvent.WaitOne();
            }
            Console.WriteLine($"Search names by {typeTechnique.GetTypeName()} took {(int)stopwatch.Elapsed.TotalMilliseconds} milliseconds");

            stopwatch.Restart();
            aggregator.Print();
            Console.WriteLine($"Results printing took {(int)stopwatch.Elapsed.TotalMilliseconds} milliseconds");

            Console.WriteLine("END");
            Console.ReadLine();
        }
    }
}
