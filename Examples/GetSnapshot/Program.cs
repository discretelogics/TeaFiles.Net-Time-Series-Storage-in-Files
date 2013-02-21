// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
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
                if (args.Count() != 1) throw new Exception("Usage: CreateTicks <filename>");
                string filename = args.First();

                using (var tf = TeaFile<Tick>.OpenRead(filename))
                {
                    Console.WriteLine(tf.Description);
                    Console.WriteLine("ItemAreaStart={0}", tf.ItemAreaStart);
                    Console.WriteLine("ItemAreaEnd={0}", tf.ItemAreaEnd);
                    Console.WriteLine("ItemAreaSize={0}", tf.ItemAreaSize);
                    foreach (Tick tick in tf.Items.Take(5))
                    {
                        Console.WriteLine(tick);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
