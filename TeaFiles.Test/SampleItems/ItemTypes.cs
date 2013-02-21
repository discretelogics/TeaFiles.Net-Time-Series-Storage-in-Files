// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System.Runtime.InteropServices;

namespace TeaTime.SampleItems
{
    public struct OHLCV
    {
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public int Volume;

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4}", this.Open, this.High, this.Low, this.Close, this.Volume);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TOHLCV
    {
        public Time Time;
        public double Open;
        public double High;
        public double Low;
        public double Close;

        public int Volume;

        public override string ToString()
        {
            return this.Time.ToString();
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 40)]
    struct OHLCV3
    {
        [FieldOffset(0)]
        public double Open;

        [FieldOffset(8)]
        public double High;

        [FieldOffset(16)]
        public double Low;

        [FieldOffset(24)]
        public double Close;

        [EventTime]
        [FieldOffset(32)]
        public Time Time;
    }

    public struct DecimalTick
    {
        public decimal Price;
        public int Volume;
        public int Id;

        public override string ToString()
        {
            return this.Id + " " + this.Price + " " + this.Volume;
        }
    }

    public struct Tick
    {
        public Time Time;
        public double Price;
        public int Volume;
        public int Id;
    }

    public struct EmptyStruct
    {
    }
}
