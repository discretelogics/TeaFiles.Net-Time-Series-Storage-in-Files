// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.IO;

namespace TeaTime
{
    class TestUtils
    {
        public static bool IsLocked(string filename)
        {
            try
            {
                var movedfilename = filename + "moved";
                File.Move(filename, movedfilename);
                File.Move(movedfilename, filename);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        public static Stream GetTeaFileEventInt7Values()
        {
            var stream = new MemoryStream();
            using (var tf = TeaFile<Event<int>>.Create(stream))
            {
                Time t = new Time(2000, 1, 1);
                for (int i = 0; i < 7; i++)
                {
                    tf.Write(new Event<int> {Time = t, Value = i * 1100});
                    t = new Time(t.Ticks + Time.Scale.TicksPerDay); // add methods for Time are not available (yet?)
                }
            }
            stream.Position = 0;
            return stream;
        }
    }
}
