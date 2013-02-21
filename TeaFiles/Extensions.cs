// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace TeaTime
{
    /// <summary>
    /// Basic extensions on elementary types like strings or collections.
    /// </summary>
    /// <remarks>
    /// These extensions remain internal, to prevent collisions with other TeaTime Extensions.
    /// </remarks>
    static partial class Extensions
    {
        /// <summary>
        /// Returns the object's string representation or the <paramref name="emptyValue"/> if the object reference is null.
        /// </summary>
        /// <param name="o">The object whose string representation shall be returned.</param>
        /// <param name="emptyValue">The string to be returned if <paramref name="o"/> is null.</param>
        /// <returns></returns>
        public static string SafeToString(this object o, string emptyValue)
        {
            return o == null ? emptyValue : o.ToString();
        }

        /// <summary>
        /// Returns the object's string representation or "empty" if the object reference is null.
        /// </summary>
        /// <param name="o">The object whose string representation shall be returned.</param>
        /// <returns></returns>
        public static string SafeToString(this object o)
        {
            return o.SafeToString("empty");
        }

        /// <summary>
        /// A more convenient writing of string.IsNullOrEmpty(s).
        /// </summary>
        /// <param name="s">The string that is checked for null or emptyness</param>
        /// <returns>true if thee string is not null and not empty.</returns>
        public static bool IsSet(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        /// <summary>
        /// IsDefined formulated as generic method. This makes the code a little bit more concise.
        /// </summary>
        /// <typeparam name="A">The attribute type.</typeparam>
        /// <param name="mi">The MemberInfo to be checked for the attribute.</param>
        /// <returns>True if <paramref name="mi"/> is attributed with <typeparamref name="A"/>.</returns>
        public static bool IsDefined<A>(this MemberInfo mi) where A : Attribute
        {
            return mi.IsDefined(typeof (A), false);
        }

        /// <summary>
        /// Execute <paramref name="a"/> <paramref name="n"/> times.
        /// </summary>
        /// <param name="n">Specifies how many times a() is executed.</param>
        /// <param name="a">The action to execute.</param>
        [DebuggerStepThrough]
        public static void Times(this int n, Action a)
        {
            for (int i = 0; i < n; i++) a();
        }

        /// <summary>
        /// Returns a string representation of an objects fields using reflection.
        /// </summary>
        /// <param name="o">The object whose string representation shall be returned.</param>
        /// <param name="separator">The separator between the name=fields parts.</param>
        /// <returns>The string holding field names and values.</returns>
        public static string ToStringFromFields(this object o, string separator)
        {
            return string.Join(separator,
                               o.GetType().GetFields()
                                   .Select(f => f.Name + "=" + f.GetValue(o)));
        }

        /// <summary>
        /// Returns a string representation of an objects fields using reflection. Between each name=value
        /// pair, a space will be used as separator.
        /// </summary>
        /// <param name="o">The object whose string representation shall be returned.</param>
        /// <returns>The string holding field names and values.</returns>
        public static string ToStringFromFields(this object o)
        {
            return ToStringFromFields(o, " ");
        }

        /// <summary>
        /// Allows simpler formulation of string.Format("format", a, b, c) by
        ///                               format.Formatted(a, b, c)
        /// </summary>
        /// <param name="s">The value to be formatted.</param>
        /// <param name="values"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])")]
        public static string Formatted(this string s, params object[] values)
        {
            return string.Format(s, values);
        }


        /// <summary>
        /// Joins the values to a string concatenting them with <paramref name="separator"/>.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string Joined<T>(this IEnumerable<T> values, string separator)
        {
            return string.Join(separator, values);
        }

        /// <summary>
        /// Returns the first part of the string, until character <paramref name="stopCharacter"/> appears. If such 
        /// chracter is not present, the whole string is returned.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="stopCharacter"></param>
        /// <returns></returns>
        public static string GetFirstPart(this string s, char stopCharacter)
        {
            int n = s.IndexOf(stopCharacter);
            if (n == -1) return s;
            return s.Substring(0, n);
        }

        /// <summary>
        /// Provides a ForEach extension for any IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type of the enumerable.</typeparam>
        /// <param name="collection">The enumerable collection.</param>
        /// <param name="a">The action to execute for each item in the collection.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> a)
        {
            foreach (var item in collection) a(item);
        }

        /// <summary>
        /// Composes a name for a type using template syntax used in programmming languages like C++ or C#.
        /// Example: For the Type int, this method returns "int".
        /// Example: For the Type Event&lt;double&gt; it returns Event&lt;double&gt; .
        /// </summary>
        /// <param name="t">The type whose name is composed.</param>
        /// <returns></returns>
        public static string GetLanguageName(this Type t)
        {
            if (t.IsGenericType)
            {
                return t.Name.GetFirstPart('`') + "<" + t.GetGenericArguments().Select(GetLanguageName).Joined(",") + ">";
            }
            return t.Name;
        }

        /// <summary>
        /// Makes any object instance enumerable.
        /// </summary>
        /// <remarks>
        /// Useful to use single instances in LINQ statements.
        /// </remarks>
        /// <typeparam name="T">The type of the instance.</typeparam>
        /// <param name="value">The instance.</param>
        /// <returns>An enumerable that returns the instance.</returns>
        public static IEnumerable<T> ToEnumerable<T>(this T value)
        {
            yield return value;
        }
    }
}
