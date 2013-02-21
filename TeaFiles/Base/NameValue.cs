// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TeaTime
{
    /// <summary>
    /// A name / value pair.
    /// </summary>
    /// <remarks>
    /// The value type might be an int32, double, string or Guid.
    /// </remarks>
    public class NameValue
    {
        string name;
        object value;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <remarks>This overload is used for string values.</remarks>
        /// <param name="name">The name.</param>
        /// <param name="value">The string value.</param>
        public NameValue(string name, string value)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <remarks>This overload is used for int values.</remarks>
        /// <param name="name">The name.</param>
        /// <param name="value">The int value.</param>
        public NameValue(string name, int value)
        {
            if (name == null) throw new ArgumentNullException("name");
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <remarks>This overload is used for double values.</remarks>
        /// <param name="name">The name.</param>
        /// <param name="value">The double value.</param>
        public NameValue(string name, double value)
        {
            if (name == null) throw new ArgumentNullException("name");
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <remarks>This overload is used for Guid values.</remarks>
        /// <param name="name">The name.</param>
        /// <param name="value">The Guid value.</param>
        public NameValue(string name, Guid value)
        {
            if (name == null) throw new ArgumentNullException("name");
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// The name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// The <see cref="Kind"/> of the value.
        /// </summary>
        internal ValueKind Kind
        {
            get
            {
                if (this.value is int) return ValueKind.Int32;
                if (this.value is double) return ValueKind.Double;
                if (this.value is string) return ValueKind.Text;
                if (this.value is Guid) return ValueKind.Guid;
                var msg = (this.value == null) ? "NameValue has a null value." : "NameValue has a value that is not allowed: " + this.value.GetType().FullName;
                throw new InvalidStateException(msg);
            }
        }

        /// <summary>The value as a string. </summary>
        public string ValueText
        {
            get { return this.value.ToString(); }
        }

        /// <summary>The typed value. </summary>
        /// <typeparam name="T">The type of the value. </typeparam>
        /// <returns>The value. </returns>
        /// <remarks>
        /// Usually, the type of a value is known. In this case its value could be retrieved calling
        /// <code>
        /// using (var tf = TeaFile&lt;Tick&gt;.Create("acme.tea", null, NameValueCollection.From("decimals", 2)))
        /// {
        /// }
        /// 
        /// var tf = TeaFile.OpenRead("acme.tea");
        /// int n = tf.Description.NameValues.GetValue{int}();
        /// </code>
        /// This will conveniently return the integer value. In the rare case were the type is not known, 
        /// this method can be called with a type of object. Lets assume we are not sure if the decimals property 
        /// is stored as integer, double or string. Then the code
        /// <code>
        /// object n = tf.Description.NameValues.GetValue&lt;object&gt;();
        /// Console.WriteLine(n);
        /// </code>
        /// will return the value as plain object.
        /// </remarks>
        public T GetValue<T>()
        {
            return (T)this.value;
        }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </summary>
        /// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </returns>
        public override string ToString()
        {
            return this.name + "=" + this.value;
        }

        /// <summary>
        /// Specifies the type of a value in a NameValue item.
        /// </summary>
        /// <remarks>
        /// Serializes as Int32.
        /// </remarks>
        internal enum ValueKind
        {
            Invalid = 0,
            Int32 = 1,
            Double = 2,
            Text = 3,
            Guid = 4
        }
    }

    /// <summary>
    /// A collection of name / value pairs.
    /// </summary>
    /// <remarks>Provides typesafe getter and specialiced Add methods fo each type. The name properties of 
    /// the stored name/value pairs do not need to be unique, although practically this is usually reasonable.</remarks>
    public class NameValueCollection : IEnumerable<NameValue>
    {
        List<NameValue> nameValues;

        /// <summary>Constructor.</summary>
        public NameValueCollection()
        {
            this.nameValues = new List<NameValue>();
        }

        /// <summary>
        /// Factory method.
        /// </summary>
        /// <remarks>
        /// This method simplifies the creation of <see cref="NameValueCollection"/> instances:<br/>
        /// TeaFile{T}.Create("acme.tea", "acme prices", NameValueCollection.From("decimals", 2, "sensor", 42, "country", "UK"));
        /// </remarks>
        /// <param name="args">An array of name/value pairs. Its length must be even and the first value of each pair must be a string.</param>
        /// <returns></returns>
        public static NameValueCollection From(params object[] args)
        {
            if (args == null) throw new ArgumentNullException("args");
            if (args.Length % 2 == 1)
            {
                throw new ArgumentException("<args> must have even count.");
            }

            NameValueCollection nvc = new NameValueCollection();
            for (int i = 0; i < args.Length; i += 2)
            {
                var name = args[i] as string;
                if (name == null) throw new ArgumentException("args[0,2,...] must be of type string");
                var value = args[i + 1];
                if (value is int)
                    nvc.Add(new NameValue(name, (int)value));
                if (value is string)
                    nvc.Add(new NameValue(name, (string)value));
                else if (value is double)
                    nvc.Add(new NameValue(name, (double)value));
                else if (value is Guid)
                    nvc.Add(new NameValue(name, (Guid)value));
            }
            return nvc;
        }

        /// <summary>
        /// The number of name / value pairs in the collection.
        /// </summary>
        public int Count
        {
            get { return this.nameValues.Count; }
        }

        /// <summary>
        /// Adds the a <see cref="NameValue"/>.
        /// </summary>
        /// <param name="nameValue">The name/value pair.</param>
        /// <returns>This instance, to allow fluent addition of name / values.</returns>
        /// <remarks>The name property of <paramref name="nameValue"/> is not checked for uniqueness.</remarks>
        public NameValueCollection Add(NameValue nameValue)
        {
            this.nameValues.Add(nameValue);
            return this;
        }

        /// <summary>
        /// Adds the specified name / value pair.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The string value.</param>
        /// <returns>This instance, to allow fluent addition of name / values.</returns>
        /// <remarks>The name is not checked for uniqueness.</remarks>
        public NameValueCollection Add(string name, string value)
        {
            this.nameValues.Add(new NameValue(name, value));
            return this;
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The int value.</param>
        /// <returns>This instance, to allow fluent addition of name / values.</returns>
        /// <remarks>The name is not checked for uniqueness.</remarks>
        public NameValueCollection Add(string name, int value)
        {
            this.nameValues.Add(new NameValue(name, value));
            return this;
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The double value.</param>
        /// <returns>This instance, to allow fluent addition of name / values.</returns>
        /// <remarks>The name is not checked for uniqueness.</remarks>
        public NameValueCollection Add(string name, double value)
        {
            this.nameValues.Add(new NameValue(name, value));
            return this;
        }

        /// <summary>
        /// Adds the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The Guid value.</param>
        /// <returns>This instance, to allow fluent addition of name / values.</returns>
        /// <remarks>The name is not checked for uniqueness.</remarks>
        public NameValueCollection Add(string name, Guid value)
        {
            this.nameValues.Add(new NameValue(name, value));
            return this;
        }

        /// <summary>
        /// Gets the value stored in a NameValue having name == <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="name">The name under which the value is stored.</param>
        /// <returns>The value.</returns>
        /// <remarks>Throws exceptions if no value is present under <paramref name="name"/> or the value is not T.</remarks>
        public T GetValue<T>(string name)
        {
            return this.nameValues.First(nv => nv.Name == name).GetValue<T>();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.</returns>
        /// <remarks></remarks>
        public IEnumerator<NameValue> GetEnumerator()
        {
            return this.nameValues.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
        /// <remarks></remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </summary>
        /// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </returns>
        public override string ToString()
        {
            var s = string.Join(Environment.NewLine, this.nameValues.Take(10));
            if (this.nameValues.Count > 10) s += Environment.NewLine + "...";
            return s;
        }
    }
}
