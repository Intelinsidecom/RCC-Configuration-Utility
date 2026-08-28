using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Principal;

namespace RCCConfigurator
{
    class Program
    {
        // not sure if its changed in older or newer binaries, works with 2016
        public string RCC32RegeditDir = @"SOFTWARE\ROBLOX Corporation\Roblox";
        public string RCC64RegeditDir = @"SOFTWARE\WOW6432Node\ROBLOX Corporation\Roblox";
        public string AccessKeyName = @"AccessKey";
        public string SettingsKeyName = @"SettingsKey";
        public string AccessKey = @"";
        public string SettingsKey = @"";
        public bool isElevated;

        static void Main(string[] args)
        {
            Program P = new Program();
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                P.isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            P.PrintBanner();
            if (!P.isElevated)
            {
                Console.WriteLine("The application utilizes administrator privilieges for editing the registry keys and cannot execute the task without them.");
                Console.WriteLine("Press Any Key to exit");
                var exit = Console.ReadKey().Key;
                Environment.Exit(0);
            }
            Console.WriteLine("Which value would you like to set AccessKey to? (appended to every http request like ?AccessKey= by RCC Binary");
            P.AccessKey = Console.ReadLine();
            if (!String.IsNullOrEmpty(P.AccessKey)) // skip it then
            {
                if (P.IsOS64Bit())
                {
                    Console.WriteLine("Editing AccessKey Setting (X64):");
                    P.EditRegistryString(P.RCC64RegeditDir, P.AccessKeyName, P.AccessKey); // it seems to read from 64 node only
                }
                Console.WriteLine("Editing AccessKey Setting:");
                P.EditRegistryString(P.RCC32RegeditDir, P.AccessKeyName, P.AccessKey);
            }
            Console.WriteLine("    "); // keep some space
            Console.WriteLine("Which value would you like to set SettingsKey to?"); // TODO: document what it does
            P.SettingsKey = Console.ReadLine();
            if (!String.IsNullOrEmpty(P.SettingsKey)) // skip it then
            {
                if (P.IsOS64Bit())
                {
                    Console.WriteLine("Editing SettingsKey Setting (X64):");
                    P.EditRegistryString(P.RCC64RegeditDir, P.SettingsKeyName, P.SettingsKey); // it seems to read from 64 node only
                }

                Console.WriteLine("Editing SettingsKey Setting:");
                
                P.EditRegistryString(P.RCC32RegeditDir, P.SettingsKeyName, P.SettingsKey);
            }
            Console.WriteLine(" ");
            Console.WriteLine("Task successful, press Enter to quit or anything else to rerun again");
            var confirm = Console.ReadKey().Key;
            
            if (confirm != ConsoleKey.Enter)
            {
                string[] test = new string[1] {""};
                Console.Clear();
                Program.Main(test);
            }
            Environment.Exit(0);

        }

        public void PrintBanner()
        {
            Console.WriteLine("=======================================================");
            Console.WriteLine("       Roblox RCC Settings Configuration Utility       ");
            Console.WriteLine("           Made By Intelinsidecomputer in 2026         ");
            Console.WriteLine("=======================================================");
            Console.WriteLine("                                                       ");
            Console.WriteLine("                                                       ");
        }

        public void EditRegistryString(string location,string registryName,string registryValue, bool PrintAnything = true)
        {
            try
            {
                RegistryKey key =
                    Registry.LocalMachine.OpenSubKey(location, true);

                if (key == null)
                {
                    key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(location);
                }

                key.SetValue(
                    registryName,
                    registryValue,
                    RegistryValueKind.String);

                key.Close();
                if (PrintAnything)
                {
                    Console.WriteLine(
                        "Successfully changed " + registryName);
                    Console.WriteLine(" ");
                }
            }
            catch (System.Security.SecurityException ex)
            {
                if (PrintAnything)
                {
                    Console.WriteLine(
                        "Security error: " + ex.Message);
                    Console.WriteLine(
                        "Try running the program as Administrator.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                if (PrintAnything)
                {
                    Console.WriteLine(
                        "Access denied: " + ex.Message);
                    Console.WriteLine(
                        "Try running the program as Administrator.");
                }
            }
            catch (Exception ex)
            {
                if (PrintAnything)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        // Source - https://stackoverflow.com/a/1840313
        // Posted by dwhiteho
        // Retrieved 2026-08-28, License - CC BY-SA 2.5

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr LoadLibrary(string libraryName);

        [DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        public extern static IntPtr GetProcAddress(IntPtr hwnd, string procedureName);

        private delegate bool IsWow64ProcessDelegate([In] IntPtr handle, [Out] out bool isWow64Process);

        public bool IsOS64Bit()
        {
            if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static IsWow64ProcessDelegate GetIsWow64ProcessDelegate()
        {
            IntPtr handle = LoadLibrary("kernel32");

            if (handle != IntPtr.Zero)
            {
                IntPtr fnPtr = GetProcAddress(handle, "IsWow64Process");

                if (fnPtr != IntPtr.Zero)
                {
                    return (IsWow64ProcessDelegate)Marshal.GetDelegateForFunctionPointer((IntPtr)fnPtr, typeof(IsWow64ProcessDelegate));
                }
            }

            return null;
        }

        private static bool Is32BitProcessOn64BitProcessor()
        {
            IsWow64ProcessDelegate fnDelegate = GetIsWow64ProcessDelegate();

            if (fnDelegate == null)
            {
                return false;
            }

            bool isWow64;
            bool retVal = fnDelegate.Invoke(Process.GetCurrentProcess().Handle, out isWow64);

            if (retVal == false)
            {
                return false;
            }

            return isWow64;
        }

	}
}
