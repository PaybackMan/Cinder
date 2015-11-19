using SHS.Sage.OrientDb.Tests;
using SHS.Sage.OrientDb.Tests.Things;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage
{
    public static class Program
    {
        public static void Main()
        {
            var repoTests = new RepositoryTests();
            repoTests.Setup();

            var p = repoTests.Get<Patient>("#13:285");

            var stopwatch = new Stopwatch();
            var count = 0;
            var total = 0;
            stopwatch.Start();
            for (int i = 0; i < 20; i++)
            {
                repoTests.CanReadFastEnough(true, 50, out count);
                total += count;
            }
            stopwatch.Stop();
            Console.WriteLine(total);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine((double)total / ((double)stopwatch.ElapsedMilliseconds/1000d));
            stopwatch.Reset();


            count = 0;
            total = 0;
            stopwatch.Start();
            for (int i = 0; i < 20; i++)
            {
                repoTests.CanReadFastEnough(false, 50, out count);
                total += count;
            }
            stopwatch.Stop();
            Console.WriteLine(total);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine((double)total / ((double)stopwatch.ElapsedMilliseconds / 1000d));
            stopwatch.Reset();
        }
    }
}
