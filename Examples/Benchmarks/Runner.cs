using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TeaTime.Benchmarks
{
    class Run
    {
        public long N;
        public TimeSpan ExecutionTime;

        public double OperationsPerSecond
        {
            get { return N / ExecutionTime.TotalSeconds; }
        }
    }

    class RunSet
    {
        public List<Run> Runs { get; internal set; }
        public double RelativeExecutionTimePercent { get; set; }

        public RunSet()
        {
            this.Runs = new List<Run>();
        }

        public double AverageSeconds()
        {
            return this.Runs.Select(r => r.ExecutionTime.TotalSeconds).Average();
        }

        public double AverageOperationsPerSeconds(long n)
        {
            return (double)n / this.AverageSeconds();
        }
    }

    class Runner
    {
        Dictionary<string, RunSet> operationruns;
        public long N;
        public long TargetValue;

        public Runner(long N)
        {
            this.operationruns = new Dictionary<string, RunSet>();
            this.N = N;
        }

        public void Measure(string name, Action action)
        {
            RunSet runs;
            if (!this.operationruns.TryGetValue(name, out runs))
            {
                this.operationruns[name] = runs = new RunSet();
            }

            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            var r = new Run();
            r.N = this.N;
            r.ExecutionTime = sw.Elapsed;
            runs.Runs.Add(r);
        }

        public void ComputeStats()
        {
            double maxavgseconds = this.operationruns.Select(runs => runs.Value.AverageSeconds()).Max();
            foreach (var runs in operationruns)
            {
                var percent = runs.Value.AverageSeconds() / maxavgseconds * 100;
                percent = Math.Round(percent);
                runs.Value.RelativeExecutionTimePercent = percent;
            }
        }

        public string GetReport()
        {
            ComputeStats();
            var w = new StringWriter();
            using (w.Tag("table"))
            {
                w.WriteLine("<tr><td>benchmark</td><td colspan={0}>runs</td><td>relative</td><td>ops / second</tr>".Formatted(operationruns.First().Value.Runs.Count));
                foreach (var run in this.operationruns)
                {
                    w.WriteLine("<tr><td>{0}</td>{1}<td>{2}</td><td>{3}</td></tr>".Formatted(
                        run.Key,
                        run.Value.Runs.Select(r => "<td align=right>{0}</td>".Formatted(r.ExecutionTime.TotalMilliseconds.ToString("0.00"))).Joined(),
                        "<div class='bar' style='width:{0}px'>&nbsp;</div>".Formatted(run.Value.RelativeExecutionTimePercent),
                        run.Value.AverageOperationsPerSeconds(this.N).ToString("#,##0")
                        ));
                }
            }
            w.WriteLine();
            using (w.Tag("table"))
            {
                w.TableRow("computer specs");
                var spec = new ComputerSpecs();
                foreach (FieldInfo field in spec.GetType().GetFields())
                {
                    w.TableRow(field.Name, field.GetValue(spec) as string);
                }
            }
            string html = File.ReadAllText("results.htm");
            html = html.Replace("results", w.ToString());
            return html;
        }
    }
}
