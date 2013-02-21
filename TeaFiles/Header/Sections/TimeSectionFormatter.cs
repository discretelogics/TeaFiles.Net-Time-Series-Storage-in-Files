// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Collections.Generic;
using System.Linq;

namespace TeaTime.Header
{
    /// <summary>
    /// This section persists the time epoch and resolution used for time values and the 
    /// offsets of fields that hold time times. 
    /// </summary>
    /// <remarks>
    /// If more than one field of the time series 
    /// item holds time values, the first field in the collection is the event time of the 
    /// time series that must have monotonically increasing values.
    /// </remarks>
    class TimeSectionFormatter : ISectionFormatter
    {
        public int Id
        {
            get { return 0x40; }
        }

        public void Write(WriteContext c)
        {
            if (c.Description == null) return;
            if (c.Description.ItemDescription == null) return;
            var timeFields = c.Description.ItemDescription.Fields.Where(f => f.IsTime).ToList();
            if (!timeFields.Any()) return;

            // time format
            if (!c.Description.Timescale.HasValue)
            {
                // TeaFile<T>.Create will assign the default Timescale to the descrption,
                // so Timescale should always be set.
                throw new InternalErrorException();
            }
            c.Writer.WriteInt64(c.Description.Timescale.Value.Epoch);
            c.Writer.WriteInt64(c.Description.Timescale.Value.TicksPerDay);

            // time fields
            c.Writer.WriteInt32(timeFields.Count);
            timeFields.ForEach(f => c.Writer.WriteInt32(f.Offset));
        }

        public void Read(ReadContext c)
        {
            // time scale
            var epoch = c.Reader.ReadInt64();
            var ticksPerDay = c.Reader.ReadInt64();
            c.Description.Timescale = Timescale.FromEpoch(epoch, ticksPerDay);

            // time fields
            var timeFieldsCount = c.Reader.ReadInt32();
            var offsets = new List<int>();
            timeFieldsCount.Times(() =>
                                  offsets.Add(c.Reader.ReadInt32()));
            c.Description.TimeFieldOffsets = offsets.AsReadOnly();

            // adorn item description with time aspects, if available
            var id = c.Description.ItemDescription;
            if (id != null)
            {
                bool isFirstTimeField = true;
                foreach (var offset in offsets)
                {
                    var f = id.FindFieldByOffset(offset);
                    if (f == null) throw new FileFormatException("Time format section contains an entry for a field at offset {0} but no such field was found in the item description.");
                    f.IsTime = true;
                    f.IsEventTime = isFirstTimeField;
                    isFirstTimeField = false;
                }
            }
        }
    }
}
