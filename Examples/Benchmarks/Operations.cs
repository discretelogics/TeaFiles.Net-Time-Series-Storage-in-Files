using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using ACME.Examples;

namespace TeaTime.Benchmarks
{
    class Operations
    {        
        string filename = "bench.tea";

        public Operations(string filename)
        {
            this.filename = filename;
        }

        public static void RunAll(Runner runner)
        {
            Console.WriteLine("RunAll");
            Stopwatch sw = Stopwatch.StartNew();            

            var op = new Operations("testfile.tea");
            op.MeasureArray(runner);
            op.MeasureList(runner);
            op.MeasureListPreAllocated(runner);
            op.MeasureTeaFile(runner);
            op.MeasureTeaFileMemoryMapped(runner);
            op.MeasureTeaFileRawMemoryMapped(runner);
            op.MeasureTeaFileRawMemoryMappedTick(runner);
            
            sw.Stop();

            Console.WriteLine(sw.Elapsed);
            //op.Cleanp();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();
        }

        void Cleanp()
        {
            File.Delete(this.filename);
        }

        public void MeasureArray(Runner runner)
        {
            long N = runner.N;
            double[] values = null;
            runner.Measure("allocate array", () =>
                { 
                    values = new double[N];
                }
            );
            runner.Measure("fill array", () =>
                {
                    for (int i = 0; i < N; i++)
                    {
                        values[i] = i;
                    }
                });
            runner.Measure("sum array", () =>
                {
                    double sum = 0;
                    for (int i = 0; i < N; i++)
                    {
                        sum += values[i];
                    }
                    Console.WriteLine(sum);
                    if(sum != runner.TargetValue) throw new Exception("wrong result");
                });
        }

        public void MeasureList(Runner runner)
        {
            long N = runner.N;
            List<double> values = new List<double>();
            runner.Measure("fill list", () =>
            {
                for (int i = 0; i < N; i++)
                {
                    values.Add(i);
                }
            });
            runner.Measure("sum list", () =>
            {
                double sum = 0;
                for (int i = 0; i < N; i++)
                {
                    sum += values[i];
                }
                Console.WriteLine(sum);
                if (sum != runner.TargetValue) throw new Exception("wrong result");
            });
        }

        public void MeasureListPreAllocated(Runner runner)
        {
            long N = runner.N;
            List<double> values = new List<double>((int)N);
            runner.Measure("fill list pre", () =>
            {
                for (int i = 0; i < N; i++)
                {
                    values.Add(i);
                }
            });
            runner.Measure("sum list pre", () =>
            {
                double sum = 0;
                for (int i = 0; i < N; i++)
                {
                    sum += values[i];
                }
                Console.WriteLine(sum);
                if (sum != runner.TargetValue) throw new Exception("wrong result");
            });
        }

        public void MeasureTeaFile(Runner runner)
        {
            long N = runner.N;
            File.Delete(filename);
            using (var tf = TeaFile<double>.Create(filename))
            {
                runner.Measure("fill teafile<double>", () =>
                {
                    for (int i = 0; i < N; i++)
                    {
                        tf.Write(i); // conversion
                    }
                });
            }
            using (var tf = TeaFile<double>.OpenRead(filename))
            {
                runner.Measure("sum teafile<double>", () =>
                    {
                        double sum = 0;
                        for (int i = 0; i < N; i++)
                        {
                            sum += tf.Read();
                        }
                        Console.WriteLine(sum);
                        if (sum != runner.TargetValue) throw new Exception("wrong result");
                    });
            }
        }

        public void MeasureTeaFileMemoryMapped(Runner runner)
        {
            long N = runner.N;
            ManagedMemoryMapping<double> view = null;
            runner.Measure("TeaFile<double>.OpenMemoryMapping", () =>
            {
                view = TeaFile<double>.OpenMemoryMapping(filename);
            });
            double sum = 0d;
            runner.Measure("memmap sum += view[i]", () =>
            {
                for (int i = 0; i < N; i++)
                {
                    sum += view[i];
                }
            });
            Console.WriteLine(sum);
            runner.Measure("memmap sum += view[i] 2nd run", () =>
            {
                for (int i = 0; i < N; i++)
                {
                    sum += view[i];
                }
            });
            view.Dispose();
            view = null;
        }

        public unsafe void MeasureTeaFileRawMemoryMapped(Runner runner)
        {
            long N = runner.N;
            RawMemoryMapping<double> view = null;
            //string copyfilename = "copy" + filename;
            //if (File.Exists(copyfilename)) File.Delete(copyfilename);
            //File.Copy(filename, copyfilename);
            //if(File.Exists(filename)) File.Delete(filename);
            runner.Measure("TeaFile<double>.OpenRawMemoryMapping", () =>
            {
                view = TeaFile<double>.OpenRawMemoryMapping(filename);
            });
            runner.Measure("memmap raw sum += view[i]", () =>
                {
                    double sum = 0d;
                    double* p = (double*)view.ItemAreaStart;
                    for (int i = 0; i < N; i++)
                    {
                        sum += *p;
                        p++;
                    }
                    Console.WriteLine(sum);
                    if (sum != runner.TargetValue) throw new Exception("wrong result");
            });
            runner.Measure("memmap raw sum += view[i] 2nd run", () =>
            {
                double sum = 0d;
                double* p = (double*)view.ItemAreaStart;
                for (int i = 0; i < N; i++)
                {
                    sum += *p;
                    p++;
                }
                Console.WriteLine(sum);
                if (sum != runner.TargetValue) throw new Exception("wrong result");
            });
            runner.Measure("memmap raw sum += view[i] 3rd run", () =>
            {
                double sum = 0d;
                double* p = (double*)view.ItemAreaStart;
                for (int i = 0; i < N; i++)
                {
                    sum += *p;
                    p++;
                }
                Console.WriteLine(sum);
                if (sum != runner.TargetValue) throw new Exception("wrong result");
            });
            runner.Measure("memmap raw sum += view[i] 4th run, pointer only", () =>
            {
                double sum = 0d;
                double* p = (double*)view.ItemAreaStart;
                double* end = (double*)view.ItemAreaEnd;
                while(p < end)
                {
                    sum += *p;
                    p++;
                }
                Console.WriteLine(sum);
                if (sum != runner.TargetValue) throw new Exception("wrong result");
            });
            view.Dispose();
            view = null;
        }

        public unsafe void MeasureTeaFileRawMemoryMappedTick(Runner runner)
        {
            long N = runner.N;
            RawMemoryMapping<Tick> view = null;
            File.Delete(filename);
            using (var tf = TeaFile<Tick>.Create(filename))
            {
                for (int i = 0; i < N; i++)
                {
                    tf.Write(new Tick{Price = i});
                }                
            }

            runner.Measure("memmap raw sum using ticks", () =>
            {
                using(view = TeaFile<Tick>.OpenRawMemoryMapping(filename))
                {
                    double sum = 0d;
                    Tick* tick = (Tick*)view.ItemAreaStart;
                    for (int i = 0; i < N; i++)
                    {
                        sum += tick->Price;
                        tick++;
                    }
                    Console.WriteLine(sum);
                    if (sum != runner.TargetValue) throw new Exception("wrong result");
                }
            });
        }
    }    
}
