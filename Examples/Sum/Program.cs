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
                if (args.Count() < 1) throw new Exception("Usage: Sum <filename> [m]emorymapped");
                string filename = args[0];
                bool memoryMapped = args.Count() >= 2
                                        ? args[1].StartsWith("m", StringComparison.InvariantCultureIgnoreCase)
                                        : false;

                var sw = Stopwatch.StartNew();
                double sum;
                if (memoryMapped)
                {
                    sum = RunMemoryMapped(filename);
                }
                else
                {
                    sum = Run(filename);
                }
                sw.Stop();
                Console.WriteLine("sum=" + sum);
                Console.WriteLine("execution time = " + sw.Elapsed.TotalMilliseconds + "ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static double Run(string filename)
        {
            using (var tf = TeaFile<Tick>.OpenRead(filename))
            {
                return tf.Items.Sum(item => item.Price);
            }
        }

        static unsafe double RunMemoryMapped(string filename)
        {
            double sum = 0;
            using (var fm = TeaFile<Tick>.OpenRawMemoryMapping(filename))
            {
                for (Tick* tick = (Tick*)fm.ItemAreaStart; tick != fm.ItemAreaEnd; ++tick)
                {
                    sum += tick->Price;
                }
            }
            return sum;
        }
    }
}
