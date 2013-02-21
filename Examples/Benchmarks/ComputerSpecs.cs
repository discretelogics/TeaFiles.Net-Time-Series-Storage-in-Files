// code adopted from MeasureIt by Vance Morrison - http://blogs.msdn.com/b/vancem/archive/2009/02/06/measureit-update-tool-for-doing-microbenchmarks.aspx
using System.Management;

namespace TeaTime.Benchmarks
{
    public class ComputerSpecs
    {
        public string Name;
        public string Manufacturer;
        public string Model;

        public string OperatingSystem;
        public string OperatingSystemVersion;
        public int OperatingSystemServicePack;

        public int NumberOfDisks;
        public string SystemDiskModel;

        public int NumberOfProcessors;
        public string ProcessorName;
        public string ProcessorDescription;
        public int ProcessorClockSpeedMhz;

        public int MemoryMBytes;
        public int L1KBytes;
        public int L2KBytes;

        public ComputerSpecs()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem");
            foreach (ManagementObject mo in searcher.Get())
            {
                Name = (string)mo["Caption"];
                Manufacturer = (string)mo["Manufacturer"];
                Model = (string)mo["Model"];
                MemoryMBytes = (int)(((ulong)mo["TotalPhysicalMemory"]) / (1024 * 1024));
            }

            searcher = new ManagementObjectSearcher("Select * from Win32_OperatingSystem");
            foreach (ManagementObject mo in searcher.Get())
            {
                OperatingSystem = (string)mo["Caption"];
                OperatingSystemVersion = (string)mo["Version"];
                OperatingSystemServicePack = (int)(ushort)mo["ServicePackMajorVersion"];
                break;
            }

            searcher = new ManagementObjectSearcher("Select * from Win32_DiskDrive");
            ManagementObjectCollection disks = searcher.Get();
            NumberOfDisks = disks.Count;
            foreach (ManagementObject mo in disks)
            {
                SystemDiskModel = (string)mo["Caption"];
                break;
            }

            searcher = new ManagementObjectSearcher("Select * from Win32_Processor");
            ManagementObjectCollection processors = searcher.Get();
            NumberOfProcessors = processors.Count;
            foreach (ManagementObject mo in processors)
            {
                ProcessorName = (string)mo["Name"];
                ProcessorDescription = (string)mo["Description"];
                ProcessorClockSpeedMhz = (int)(uint)mo["MaxClockSpeed"];
                // Console.WriteLine("    NumberOfCores: " + mo["NumberOfCores"]);
                // Console.WriteLine("    NumberOfLogicalProcessors: " + mo["NumberOfLogicalProcessors"]);
                // Console.WriteLine("    L2CacheSize: " + mo["L2CacheSize"]);
                break;
            }

            searcher = new ManagementObjectSearcher("Select * from Win32_CacheMemory");
            foreach (ManagementObject mo in searcher.Get())
            {
                //Console.WriteLine("    Purpose: " + mo["Purpose"]);
                // Console.WriteLine("    InstalledSize: " + mo["InstalledSize"] + " K");
                int level = (ushort)mo["Level"] - 2;
                // Console.WriteLine("    Level: " + level + " K");
                if (level == 1)
                    L1KBytes += (int)(uint)mo["InstalledSize"];
                else if (level == 2)
                    L2KBytes += (int)(uint)mo["InstalledSize"];
            }
        }
    }
}
