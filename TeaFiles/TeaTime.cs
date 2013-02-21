// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime
{
    /// <summary>
    /// The <b>TeaTime</b> namespace provides classes to read and write <a href="http://discretelogics.com/Product/TeaFiles">TeaFiles</a>. TeaFiles 
    /// are a simple and efficient way to store time series in flat files.
    /// </summary>    
    /// <remarks>    
    /// <para><see cref="TeaFile{T}"/> is used to create, write and read TeaFiles</para>
    /// <example>
    /// <code>
    /// struct Tick // the time series item type
    /// {
    ///     public DateTime Time;
    ///     public double Price;
    ///     public int Volume;
    /// }
    /// 
    /// // create file and write some values
    /// using (var tf = TeaFile&lt;Tick&gt;.Create("gold.tea"))
    /// {
    ///     tf.Write(new Tick { Price = 5, Time = DateTime.Now, Volume = 700 });
    ///     tf.Write(new Tick { Price = 15, Time = DateTime.Now.AddHours(1), Volume = 1700 });
    /// }
    /// 
    /// // read typed
    /// using (var tf = TeaFile&lt;Tick&gt;.OpenRead("gold.tea"))
    /// {
    ///     Tick value = tf.Read();
    ///     Console.WriteLine(value);
    /// }
    /// </code>
    /// </example>
    /// <para>If the type of items stored in a TeaFile is unknown, the file can still be opened using the non generic class <see cref="TeaFile"/>.</para>
    /// <example>
    /// read untyped - we know nothing about the type of item in the file
    /// <code>
    /// using (var tf = TeaFile.OpenRead("gold.tea"))
    /// {                
    ///     foreach(Item item in tf.Items)
    ///     {
    ///         Console.WriteLine(tf.Description.ItemDescription.GetNameValueString(item));
    ///     }
    /// }
    /// 
    /// // output:
    /// output:
    /// Price=5 Time=20.8.2011 23:50
    /// Price=15 Time=21.8.2011 00:50
    /// </code>
    /// </example>
    /// If possible, typed reading is preferred, as it is much more performant and convient. Untyped reading should be used otherwise. 
    /// Tools like <a href="http://discretelogics.com/Product/TeaShell">TeaShell</a> that present arbitrary TeaFiles, not known at compile time, 
    /// use untyped reading.<br></br>    
    /// </remarks>
    static class NamespaceDoc
    {
    }
}
