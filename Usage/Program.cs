using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TeaTime;

namespace ACME.Examples
{
    // define the item type of a TeaFile
    struct Tick
    {
        public Time Time;
        public double Price;

        public override string ToString()
        {
            return Time + " " + Price;
        }
    }

    class Program
    {
        static void Main()
        {
            //  clean up from previous runs
            File.Delete("acme.tea");

            //  create file
            using (var tf = TeaFile<Tick>.Create("acme.tea"))
            {
                tf.Write(new Tick { Time = DateTime.Now, Price = 12 });
                tf.Write(new Tick { Time = DateTime.Now.AddDays(1), Price = 15 });
                tf.Write(new Tick { Time = DateTime.Now.AddDays(2), Price = 18 });
            }

            //  read file
            using (var tf = TeaFile<Tick>.OpenRead("acme.tea"))
            {
                foreach (var tick in tf.Items)
                {
                    Console.WriteLine(tick);
                }
                tf.Write(new Tick { Time = DateTime.Now.AddDays(1), Price = 15 });
                tf.Write(new Tick { Time = DateTime.Now.AddDays(2), Price = 18 });
            }

            // create file with description
            File.Delete("acme.tea");
            using (var tf = TeaFile<Tick>.Create("acme.tea", 
                "this file holds acme prices", 
                NameValueCollection.From("decimals", 2, "datafeed","bloomfield")))
            {
                tf.Write(new Tick { Time = DateTime.Now, Price = 12 });
                tf.Write(new Tick { Time = DateTime.Now.AddDays(1), Price = 15 });
                tf.Write(new Tick { Time = DateTime.Now.AddDays(2), Price = 18 });
            }

            //  get the file description by filename
            using (var tf = TeaFile<Tick>.OpenRead("acme.tea"))
            {
                Console.WriteLine(tf.Description.ContentDescription);
                Console.WriteLine(tf.Description.NameValues.GetValue<int>("decimals"));
                Console.WriteLine(tf.Description.NameValues.GetValue<string>("datafeed"));
            }
        }
    }
}
