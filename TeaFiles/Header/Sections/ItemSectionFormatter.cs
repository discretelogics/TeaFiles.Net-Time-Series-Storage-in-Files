// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
namespace TeaTime.Header
{
    /// <summary>
    /// A TeaFile's ItemSection stores the layout of the Item's fields.
    /// </summary>
    class ItemSectionFormatter : ISectionFormatter
    {
        public int Id
        {
            get { return 0x0a; }
        }

        public void Write(WriteContext c)
        {
            if (c.Description == null) return;
            var id = c.Description.ItemDescription;
            if (id == null) return;
            var w = c.Writer;
            w.WriteInt32(id.ItemSize);
            w.WriteText(id.ItemTypeName);
            w.WriteInt32(id.Fields.Count);
            foreach (Field f in id.Fields)
            {
                w.WriteInt32((int)f.FieldType);
                w.WriteInt32(f.Offset);
                w.WriteText(f.Name);
            }
        }

        public void Read(ReadContext c)
        {
            var id = new ItemDescription(DescriptionSource.File);
            var r = c.Reader;
            id.ItemSize = r.ReadInt32();
            id.ItemTypeName = r.ReadText();
            var fieldCount = r.ReadInt32();
            fieldCount.Times(() =>
                {
                    var f = id.NewField();
                    f.FieldType = (FieldType)r.ReadInt32();
                    f.Offset = r.ReadInt32();
                    f.Name = r.ReadText();
                });
            c.Description.ItemDescription = id;
        }
    }
}
