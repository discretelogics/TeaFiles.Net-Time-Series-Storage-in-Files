// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Collections.ObjectModel;
using System.Text;

namespace TeaTime
{
    /// <summary>
    /// TeaFiles optionally hold a description, describing item layout and content.
    /// </summary>
    /// <remarks>
    /// The description of a TeaFile describes
    /// <ul>
    /// <li>the layout of the items (<see cref="ItemDescription"/>),</li>
    /// <li>its content as a string (<see cref="ContentDescription"/>)</li>
    /// <li>its content as a collection of (<see cref="NameValueCollection"/>) pairs</li>
    /// <li>the <see cref="Timescale"/> for representation of time (<see cref="Timescale"/>).</li>
    /// </ul>
    /// </remarks>
    public class TeaFileDescription
    {
        /// <summary>
        /// Returns the <seealso cref="ItemDescription"/>, descibing the layout of the items stored in the file.
        /// If the file holds no description of its layout, null will be returned.
        /// </summary>
        public ItemDescription ItemDescription { get; internal set; }

        /// <summary>Gets or sets the time scale. </summary>
        /// <value>The time scale. </value>
        /// <seealso cref="Timescale"/>
        public Timescale? Timescale { get; internal set; }

        /// <summary>Gets or sets a string describing the content. </summary>
        /// <example>"Silver Prices", "NYC Temperature"</example>
        public string ContentDescription { get; internal set; }

        /// <summary>Gets or sets a list of <see cref="NameValue"/> pairs. </summary>
        /// <example>
        /// "decimals" = 2
        /// "data source" = "Reuters"
        /// "id" = {8087E80F-F031-48A1-B1AC-102E51BD173A}
        /// </example>
        public NameValueCollection NameValues { get; internal set; }

        /// <summary>Gets or sets the time field offsets. </summary>
        /// Most times this list will hold a single value which in turn will most times be 0. For instance, the
        /// following TeaFile will have such a list of {0}:
        /// <code>
        /// struct Tick
        /// {
        ///     public Time    Time;
        ///     public double  Price;
        ///     public long    Volume;
        /// }
        /// TeaFile&lt;Tick&gt;.Create("acme");
        /// </code>
        /// Since the Time field above will be at offset=0 inside the item and only a single time fields exists in the type,
        /// the TimeFieldOffsets = {0}.
        public ReadOnlyCollection<int> TimeFieldOffsets { get; internal set; }

        /// <summary>Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </summary>
        /// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />. </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("... TeaFile Description ...");
            sb.AppendLine("#Item");
            sb.AppendLine(this.ItemDescription.SafeToString());
            sb.AppendLine("#Content");
            sb.AppendLine(this.ContentDescription.SafeToString());
            sb.AppendLine("#NameValues");
            sb.AppendLine(this.NameValues.SafeToString());
            sb.AppendLine("... TeaFile Description End ...");
            return sb.ToString();
        }
    }
}
