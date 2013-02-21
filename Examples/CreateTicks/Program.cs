// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Diagnostics;
using System.Linq;
using TeaTime;

namespace ACME.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Count() != 2)
                {
                    Console.WriteLine("Usage: CreateTicks <filename> <number of ticks>");
                    return;
                }
                string filename = args[0];
                int n = int.Parse(args[1]);

                var sw = Stopwatch.StartNew();

                Time t = DateTime.Now;
                using (var tf = TeaFile<Tick>.Create(filename))
                {
                    tf.Write(Enumerable.Range(1, n).Select(i => new Tick {Time = t.AddDays(i), Price = i * 101.0, Volume = i * 1000}));
                }

                sw.Stop();
                Console.WriteLine("execution time = " + sw.Elapsed.TotalMilliseconds + "ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
