using FFXIVActivity;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace FFXIVActivityCheckerConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var http = new HttpClient();
            var activityChecker = new ActivityChecker(http);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var activityTime = await activityChecker.GetLastActivityTime(20777669);
            stopwatch.Stop();

            Console.WriteLine(activityTime.ToLongDateString());
            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
