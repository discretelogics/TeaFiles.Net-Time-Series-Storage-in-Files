// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TeaTime;

namespace ACME.Examples
{
    class Program
    {
        class TradingSession
        {
            public Time Begin;
            public Time End;
            public int TickCount;

            public TradingSession(Time time)
            {
                this.Begin = time.Date;
                this.End = this.Begin.AddDays(1);
            }

            public override string ToString()
            {
                return this.Begin + " " + this.TickCount;
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Count() < 1) throw new Exception("Usage: AnalyzeTicks <filename> [m]emorymapped|read [n]times");
                string filename = args[0];
                bool memoryMapped = args.Count() >= 2
                                        ? args[1].StartsWith("m", StringComparison.InvariantCultureIgnoreCase)
                                        : false;
                int n = args.Count() >= 3 ? int.Parse(args[2]) : 1;

                Console.WriteLine(Environment.Is64BitProcess ? "64bit" : "32bit");

                for (int i = 1; i <= n; i++)
                {
                    Console.WriteLine("----------------------------------");
                    Console.WriteLine(i + ". run");
                    var sw = Stopwatch.StartNew();
                    Run(filename, memoryMapped, n == 1);
                    sw.Stop();
                    Console.WriteLine("execution time = " + sw.Elapsed.TotalMilliseconds + "ms");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        class Statistics
        {
            public double MinPrice;
            public double MaxPrice;
            public List<TradingSession> Sessions;

            public Statistics()
            {
                this.Sessions = new List<TradingSession>();
            }
        }

        static void Run(string filename, bool memoryMapped, bool displayValues)
        {
            using (var tf = TeaFile<Tick>.OpenRead(filename, ItemDescriptionElements.None))
            {
                if (tf.Items.Count == 0) throw new Exception("File holds no items.");
            }

            Statistics stats;
            if (memoryMapped)
            {
                stats = GetSessionDataMemMapped(filename);
            }
            else
            {
                stats = GetSessionData(filename);
            }

            int minTransactions = int.MaxValue;
            int maxTransactions = int.MinValue;
            foreach (var session in stats.Sessions)
            {
                minTransactions = Math.Min(minTransactions, session.TickCount);
                maxTransactions = Math.Max(maxTransactions, session.TickCount);
            }
            Console.WriteLine("min price = " + stats.MinPrice);
            Console.WriteLine("max price = " + stats.MaxPrice);
            Console.WriteLine("min ticks per session = " + minTransactions);
            Console.WriteLine("max ticks per session = " + maxTransactions);

            var tickCounts = stats.Sessions.Select(s => s.TickCount).ToArray();
            Array.Sort(tickCounts);
            int median = tickCounts[tickCounts.Length / 2];
            Console.WriteLine("median = " + median);

            if (displayValues)
            {
                double minimumExpectedTicksPerSession = median / 2.0;
                Console.WriteLine("First 10 sessions:");
                foreach (var s in stats.Sessions.Take(10))
                {
                    Console.WriteLine(s + " " + ((s.TickCount >= minimumExpectedTicksPerSession) ? "OK" : "QUESTIONABLE"));
                }
            }
        }

        static Statistics GetSessionDataMemMapped(string filename)
        {
            using (var mf = RawMemoryMapping<Tick>.OpenRead(filename))
            unsafe
            {
                var stats = new Statistics();
                Tick* firstTick = (Tick*)mf.ItemAreaStart;
                double minPrice = firstTick->Price;
                double maxPrice = firstTick->Price;
                var session = new TradingSession(firstTick->Time);
                stats.Sessions.Add(session);
                for (var t = firstTick; t != mf.ItemAreaEnd; t++)
                {
                    if (t->Time >= session.End)
                    {
                        session = new TradingSession(t->Time);
                        stats.Sessions.Add(session);
                    }
                    session.TickCount++;
                    minPrice = Math.Min(t->Price, minPrice);
                    maxPrice = Math.Max(t->Price, maxPrice);
                }
                stats.MinPrice = minPrice;
                stats.MaxPrice = maxPrice;
                return stats;
            }
        }

        static Statistics GetSessionData(string filename)
        {
            using (var tf = TeaFile<Tick>.OpenRead(filename, ItemDescriptionElements.None))
            {
                var stats = new Statistics();
                Tick firstTick = tf.Items.First();
                double minPrice = firstTick.Price;
                double maxPrice = firstTick.Price;
                var session = new TradingSession(firstTick.Time);
                stats.Sessions.Add(session);
                int n = 0;
                foreach (var tick in tf.Items)
                {
                    if (tick.Time >= session.End)
                    {
                        session = new TradingSession(tick.Time);
                        stats.Sessions.Add(session);
                    }
                    session.TickCount++;

                    minPrice = Math.Min(tick.Price, minPrice);
                    maxPrice = Math.Max(tick.Price, maxPrice);
                    n++;
                }
                stats.MinPrice = minPrice;
                stats.MaxPrice = maxPrice;
                return stats;
            }
        }
    }
}
