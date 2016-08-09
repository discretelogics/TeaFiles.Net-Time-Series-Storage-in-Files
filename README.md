TeaFiles.Net - Time Series Storage in Files
===========================================

Use TeaFiles.Net to create, read and write files holding time series data.



Features
--------

* highest **performance**
* very **simple**
* **versatile** - open TeaFiles with C++, C#, Python, R, on any platform



TeaFiles = Binary Data + Header
-------------------------------

* TeaFile is a **file format**
* to store **time series**
* as binary **flat files**
* optional **header** holds a **description** of file contents (metadata)
* including a description of the **item type layout** (schema)
* simple format, designed to make API writing very easy
* http://www.discretelogics.com published the file format and
* releases APIs for C#, C++, Python under the GPL



Performance
-----------
> 250 Mio Items per seconds cold  
> 465 Mio Items per seconds warm

benchmark results at http://www.discretelogics.com/resources/benchmarks/teafilesbm.htm



Installation
------------

* Get the NuGet package from nuget.org/packages/TeaFiles.Net or  


Documentation
-------------

The API class documentation is available online at www.discretelogics.com/doc/teafiles.net

TeaTime product family
----------------------

TeaFiles are the foundation of the TeaTime product family, the toolset for persistence, visualisation and real time analysis of time series.

Related products: 
* [TeaShell](http://www.discretelogics.com/teashell) - Browse Time Series like Photos

Examples
--------

```C#
// define time series item type
struct Tick
{
    public Time Time;
    public double Price;
    public int Volume;
}
 // create file and write values
using (var tf = TeaFile<Tick>.Create("acme.tea"))
{
    tf.Write(new Tick { Price = 5, Time = DateTime.Now, Volume = 700 });
    tf.Write(new Tick { Price = 15, Time = DateTime.Now.AddHours(1), Volume = 1700 });
    // ...
}
// sum the prices of all items in the file
using (var tf = TeaFile<Tick>.OpenRead("acme.tea"))
{
    return tf.Items.Sum(item => item.Price);
}
```
