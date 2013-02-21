using System;
using System.Collections.Generic;
using System.IO;

namespace TeaTime.Benchmarks
{
    static class Extensions
    {
        public static string Formatted(this string s, params object[] values)
        {
            return string.Format(s, values);
        }

        public static string Joined<T>(this IEnumerable<T> values, string separator = "")
        {
            return string.Join(separator, values);
        }

        public static IDisposable Tag(this TextWriter writer, string tag)
        {
            writer.Write("<" + tag + ">");
            return new Disposable(() => writer.Write("</" + tag + ">"));
        }

        public static TextWriter TableRow(this TextWriter writer, params string[] tabledata)
        {
            using (writer.Tag("tr"))
            {
                foreach (var s in tabledata)
                {
                    using (writer.Tag("td"))
                    {
                        writer.Write(s);
                    }
                }
            }
            writer.WriteLine();
            return writer;
        }
    }

    class Disposable : IDisposable
    {
        Action action;

        public Disposable(Action a)
        {
            this.action = a;
        }

        public void Dispose()
        {
            this.action();
        }
    }
}
