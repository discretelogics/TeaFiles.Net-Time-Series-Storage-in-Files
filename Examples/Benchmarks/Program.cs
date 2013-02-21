// copyright discretelogics © 2011
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Management;
using System.Reflection;

namespace TeaTime.Benchmarks
{
    class Program
    {
        static void Main()
        {
            const long N = 5 * 1000 * 1000;
            const string filename = "teafiles.benchmark.htm";

            //  compute the sum by formula and print it, as a check value
            const long target = (N * (N - 1) / 2);

            Runner runner = new Runner(N);
            runner.TargetValue = target;
            for (int i = 0; i < 5; i++)
            {
                Operations.RunAll(runner);
            }
            
            File.Delete(filename);
            File.WriteAllText(filename, runner.GetReport());
            Process.Start(filename);
        }        
    }   
}