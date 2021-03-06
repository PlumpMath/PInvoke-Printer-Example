﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ESCPOSTester
{
    public class RawPrinterHelper
    {
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PRINTER_DEFAULTS
        {
            public IntPtr pDatatype;
            public IntPtr pDevMode;
            public int DesiredAccess;
        }

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.OpenPrinter
        /// </summary>
        /// <param name="szPrinter"></param>
        /// <param name="hPrinter"></param>
        /// <param name="pd"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.ClosePrinter
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.EndDocPrinter
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.EndPagePrinter
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.WritePrinter
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <param name="pBytes"></param>
        /// <param name="dwCount"></param>
        /// <param name="dwWritten"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);


        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.ResetPrinter
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <param name="pd"></param>
        /// <returns></returns>
        [DllImport("winspool.Drv", EntryPoint = "ResetPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ResetPrinter(IntPtr hPrinter, ref PRINTER_DEFAULTS pd);

        /// <summary>
        /// http://www.pinvoke.net/default.aspx/winspool.ReadPrinter
        /// </summary>
        /// <param name="hPrinter"></param>
        /// <param name="pBuf"></param>
        /// <param name="cbBuf"></param>
        /// <param name="pNoBytesRead"></param>
        /// <returns></returns>
        [DllImport("winspool.drv", EntryPoint = "ReadPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        static extern bool ReadPrinter(IntPtr hPrinter, [MarshalAs(UnmanagedType.LPStr)] StringBuilder pBuf, int cbBuf, out int pNoBytesRead);

        /// <summary>
        /// Send unmanaged data to the target printer.
        /// When the function is given a printer name and an unmanaged array
        /// of bytes, the function sends those bytes to the print queue.
        /// Returns true on success, false on failure.
        /// </summary>
        /// <param name="szPrinterName">String name of printer</param>
        /// <param name="pBytes">Pointer to data</param>
        /// <param name="dwCount">Length of data in bytes</param>
        /// <returns>bool</returns>
        public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
           
            Int32 dwError = 0, dwWritten = 0;
            IntPtr hPrinter = new IntPtr(0);
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            // Can't send empty string
            if (string.IsNullOrEmpty(szPrinterName))
            {
                return bSuccess;
            }

            di.pDocName = "ESCPOSTTester";
            di.pDataType = "RAW";

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                // Start a document.
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    // Start a page.
                    if (StartPagePrinter(hPrinter))
                    {
                        // Write your bytes.
                        bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }

            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }

        /// <summary>
        /// Send file to printer as a print job. If the path does not exists
        /// an exception will be thrown
        /// </summary>
        /// <param name="szPrinterName">String name of printer</param>
        /// <param name="szFileName">Path to file to print</param>
        /// <returns>bool</returns>
        public static bool SendFileToPrinter(string szPrinterName, string szFileName)
        {
            // Open the file
            using (FileStream fs = new FileStream(szFileName, FileMode.Open))
            using(BinaryReader br = new BinaryReader(fs))
            {
                // Dim an array of bytes big enough to hold the file's contents.
                Byte[] bytes = new Byte[fs.Length];
                bool bSuccess = false;
                // Your unmanaged pointer.
                IntPtr pUnmanagedBytes = new IntPtr(0);
                int nLength;

                nLength = Convert.ToInt32(fs.Length);

                // Read the contents of the file into the array.
                bytes = br.ReadBytes(nLength);

                // Allocate some unmanaged memory for those bytes.
                pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);

                // Copy the managed byte array into the unmanaged array.
                Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);

                // Send the unmanaged bytes to the printer.
                bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);

                // Free the unmanaged memory that you allocated earlier.
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
                return bSuccess;
            }
        }
     
        /// <summary>
        /// Convenience wrapper around SendBytesToPrinter 
        /// </summary>
        /// <param name="szPrinterName">String name of printer</param>
        /// <param name="szString">String to send to printer</param>
        /// <returns></returns>
        public static bool SendStringToPrinter(string szPrinterName, string szString)
        {
            IntPtr pBytes;
            Int32 dwCount;
            // How many characters are in the string?
            dwCount = szString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(szString);
            // Send the converted ANSI string to the printer.
            SendBytesToPrinter(szPrinterName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);
            return true;
        }
 
        /// <summary>
        /// Request a printer reboot
        /// Note: Not supported by Reliance/Phoenix
        /// </summary>
        /// <param name="szPrinterName">String name of printer</param>
        /// <returns></returns>
        public static bool RebootPrinter(string szPrinterName)
        {
            Int32 dwError = 0;
            IntPtr hPrinter = new IntPtr(0);
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            // Open the printer.
            if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {

                PRINTER_DEFAULTS pd = new PRINTER_DEFAULTS();
                bSuccess = ResetPrinter(hPrinter, ref pd);

                ClosePrinter(hPrinter);
            }
          
            // If you did not succeed, GetLastError may give more information
            // about why not.
            if (bSuccess == false)
            {
                dwError = Marshal.GetLastWin32Error();
            }
            return bSuccess;
        }
    }
}
