using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TextSeeker
{
    class Program
    {
        // configuration values
        const string URL = "http://norvig.com/big.txt";
        const string NAMES = @"James,John,Robert,Michael,William,David,Richard,Charles,Joseph,Thomas,Christopher,Daniel,Paul,Mark,Donald,George,Kenneth,Steven,Edward,Brian,Ronald,Anthony,Kevin,Jason,Matthew,Gary,Timothy,Jose,Larry,Jeffrey,Frank,Scott,Eric,Stephen,Andrew,Raymond,Gregory,Joshua,Jerry,Dennis,Walter,Patrick,Peter,Harold,Douglas,Henry,Carl,Arthur,Ryan,Roger";
        const int PART = 1000;

        static readonly Stopwatch stopwatch = new Stopwatch();
        static readonly IDataService dataService = new DataFeed();

        // text find approaches (uncomment necessary)
        static readonly ITechnique typeTechnique = new RegexTechnique();
        //static readonly ITechnique typeTechnique = new IndexOfTechnique();

        static async Task Main(string[] args)
        {
            Console.WriteLine("STARTED...");

            // download and prepare a big text
            var text = await dataService.GetText(URL);
            var parts = dataService.DivideByParts(text, PART);

            // search names by thread pool
            stopwatch.Start();
            var aggregator = new Aggregator();
            int toProcess = parts.Count;
            var names = NAMES.Split(',').ToList();
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                var partsArr = parts.ToArray();
                var matcher = new Matcher(typeTechnique);

                for(var i=0; i < partsArr.Length; i++) {
                    var lineIndex = i;
                    ThreadPool.QueueUserWorkItem(state => 
                    {
                        var result  = matcher.Find(names, partsArr[lineIndex], lineIndex * 1000);
                        aggregator.Add(result);

                        if (Interlocked.Decrement(ref toProcess) == 0)
                        {
                            resetEvent.Set();
                        }
                    });
                };

                resetEvent.WaitOne();
            }
            stopwatch.Stop();

            // print results
            aggregator.Print();

            Console.WriteLine($"Search names by {typeTechnique.GetTypeName()} took {(int)stopwatch.Elapsed.TotalMilliseconds} milliseconds");
            Console.WriteLine("ENDED");
            Console.ReadLine();
        }
    }
}
