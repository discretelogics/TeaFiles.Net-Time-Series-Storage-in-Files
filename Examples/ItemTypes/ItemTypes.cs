// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using TeaTime;

namespace ACME.Examples
{
    public struct Tick
    {
        [EventTime]
        public Time Time;

        public double Price;
        public int Volume;

        public override string ToString()
        {
            return "Time=" + this.Time + " Price=" + this.Price + " Volume=" + this.Volume;
        }
    }
}
