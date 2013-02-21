// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime
{
    /// <summary>
    /// A field within the layout analysis process.
    /// </summary>
    class AnalyzedField
    {
        readonly FieldPath fieldPath;
        readonly int offset;
        string name;

        public AnalyzedField(FieldPath fieldPath, int offset)
        {
            this.fieldPath = fieldPath;
            this.offset = offset;
            this.name = this.FieldPath.DotPath;
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public FieldPath FieldPath
        {
            get { return this.fieldPath; }
        }

        public int Offset
        {
            get { return this.offset; }
        }

        public override string ToString()
        {
            return string.Join(" ", this.name, this.FieldPath.Last.FieldType, this.FieldPath, this.offset);
        }
    }
}
