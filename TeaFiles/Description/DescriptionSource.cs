// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime
{
    /// <summary>
    /// Describes the origin of an ItemDescription.
    /// </summary>
    /// <remarks>
    /// ItemDescriptions are either ccreated by reflecting and analyzing a .Net Struct or are read from a file.
    /// </remarks>
    enum DescriptionSource
    {
        /// <summary>
        /// The value has not been set.
        /// </summary>
        None = 0,

        /// <summary>
        /// The ItemDescription was read from a TeaFile.
        /// </summary>
        File = 1,

        /// <summary>
        /// The ItemDescription was created by reflecting and analyzing the item's type.
        /// </summary>
        ItemType = 2
    }
}
