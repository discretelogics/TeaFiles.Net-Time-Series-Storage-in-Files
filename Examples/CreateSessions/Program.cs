// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.IO;
using TeaTime;

namespace ACME.Examples
{
    class Program
    {
        static Random r;

        static bool DrawRandom(double percentageProbability)
        {
            return r.Next(0, 99) < percentageProbability;
        }

        /// <summary>
        /// This program creates a TeaFile that holds ticks on n days. 90% of the days will be good days, holding around 1000 ticks,
        /// while the other 10% are bad days that hold only 1% of that number, so about 10 ticks.
        /// Such sample file can then be used to run the AnalyzeTicks program that detects errors in data files.
        /// </summary>        
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2) throw new Exception("Usage: CreateSessions <filename> <number of days>");
                string filename = args[0];
                int n = int.Parse(args[1]);
                CreateSampleFile(filename, n);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void CreateSampleFile(string filename, int n)
        {
            r = new Random(1);
            var t = new Time(2000, 1, 1);
            File.Delete(filename);
            using (var tf = TeaFile<Tick>.Create(filename))
            {
                while (n-- > 0)
                {
                    bool isGoodDay = DrawRandom(90);
                    WriteDailyTicks(tf, t, isGoodDay);
                    t = t.AddDays(1);
                }
            }
        }

        static void WriteDailyTicks(TeaFile<Tick> tf, Time day, bool isGoodDay)
        {
            Time t = day.AddHours(9); // start trading session at 09:00
            Time end = day.AddHours(17.5); // end trading session at 17:30
            while (t < end)
            {
                //  on a good day, we write many ticks, on a bad day however only 1 percent as much
                if (isGoodDay || DrawRandom(1))
                {
                    double p = r.NextDouble() * 100000.0;
                    tf.Write(new Tick {Time = t, Price = p, Volume = 10});
                }
                t = t.AddSeconds(15 + r.Next(0, 20));
            }
        }
    }
}
