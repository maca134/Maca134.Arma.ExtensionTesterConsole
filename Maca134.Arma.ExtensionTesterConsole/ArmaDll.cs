using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Maca134.Arma.ExtensionTesterConsole
{
    internal class ArmaDll : IDisposable
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function);

        private const int OutputSize = 10240;
        private readonly bool _loaded;
        private readonly IntPtr _dllPtr;
        private readonly RvExtension _rvextension;

        public ArmaDll(string path)
        {
            if (!File.Exists(path))
                throw new ArmaDllException("no dll found");
            var cpu = GetDllMachineType(path);
            if (cpu == MachineType.IMAGE_FILE_MACHINE_I386 && IntPtr.Size == 8)
                throw new ArmaDllException("you can not run a x86 dll in the x64 exe");
            if (cpu == MachineType.IMAGE_FILE_MACHINE_AMD64 && IntPtr.Size == 4)
                throw new ArmaDllException("you can not run a x64 dll in the x86 exe");

            try
            {
                _dllPtr = LoadLibrary(path);
                _rvextension = LoadFunction<RvExtension>(IntPtr.Size == 4 ? "_RVExtension@12" : "RVExtension");
            }
            catch (Exception ex)
            {
                throw new ArmaDllException("error loading the dll", ex);
            }
            _loaded = true;
        }

        private T LoadFunction<T>(string name) where T : class
        {
            return Marshal.GetDelegateForFunctionPointer(GetProcAddress(_dllPtr, name), typeof(T)) as T;
        }

        public string Call(string input)
        {
            if (input.Length > OutputSize)
                throw new ArmaDllException("input is too big!");
            var output = new StringBuilder();
            try
            {
                _rvextension(output, OutputSize, input);
            }
            catch (Exception ex)
            {
                throw new ArmaDllException("error calling the arma dll", ex);
            }
            return output.ToString();
        }

        public void Dispose()
        {
            if (!_loaded)
                return;
            FreeLibrary(_dllPtr);
        }

        public static MachineType GetDllMachineType(string dllPath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).
            //
            var fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            var br = new BinaryReader(fs);
            fs.Seek(0x3c, SeekOrigin.Begin);
            var peOffset = br.ReadInt32();
            fs.Seek(peOffset, SeekOrigin.Begin);
            var peHead = br.ReadUInt32();

            if (peHead != 0x00004550) // "PE\0\0", little-endian
                throw new ArmaDllException("Can't find PE header");

            var machineType = (MachineType)br.ReadUInt16();
            br.Close();
            fs.Close();
            return machineType;
        }

        public enum MachineType : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
            IMAGE_FILE_MACHINE_AM33 = 0x1d3,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM = 0x1c0,
            IMAGE_FILE_MACHINE_EBC = 0xebc,
            IMAGE_FILE_MACHINE_I386 = 0x14c,
            IMAGE_FILE_MACHINE_IA64 = 0x200,
            IMAGE_FILE_MACHINE_M32R = 0x9041,
            IMAGE_FILE_MACHINE_MIPS16 = 0x266,
            IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
            IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
            IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
            IMAGE_FILE_MACHINE_R4000 = 0x166,
            IMAGE_FILE_MACHINE_SH3 = 0x1a2,
            IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
            IMAGE_FILE_MACHINE_SH4 = 0x1a6,
            IMAGE_FILE_MACHINE_SH5 = 0x1a8,
            IMAGE_FILE_MACHINE_THUMB = 0x1c2,
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
        }
    }
}