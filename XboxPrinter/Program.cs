namespace XboxPrinter
{
    using System;
    using XDevkit;
    using JRPC_Client;
    using Serilog;
    using Microsoft.Extensions.Logging;
    using System.IO;

    internal class Program {
        private static string currentConsoleName;
        private static XboxConsole xbCon;
        private static IXboxDebugTarget xbDebug;
        private static XboxManager xbMgr = new XboxManager();
        static void Main(string[] args) {
            CreateLogger();

            var Consoles = new XboxManager().Consoles;

            int Index = 0;
            foreach(string consoleName in Consoles) {
                Log.Information("{0}: {1}", Index, consoleName);
                Index++;
            }

            Log.Information("Enter the index of the console you want to connect to: ");

            int consoleIndex = Convert.ToInt32(Console.ReadLine());

            if (ConnectToConsole(Consoles[consoleIndex])) {
                Log.Information("Connected to {0}", Consoles[consoleIndex]);

                xbDebug.ConnectAsDebugger("Ozark Debugger", XboxDebugConnectFlags.Force);

                xbCon.OnStdNotify += XbCon_OnStdNotify; ;
            } else
            {
                Log.Error("Failed to connect to {0}", Consoles[consoleIndex]);
            }

        clear:
            Console.Clear();
            Log.Information("Press any key to clear the console...");
            Console.ReadKey();

            goto clear;
        }

        private static void XbCon_OnStdNotify(XboxDebugEventType EventCode, IXboxEventInfo EventInfo) {
            if (EventCode != XboxDebugEventType.DebugString)
                return;

            Log.Information("{0}", EventInfo.Info.Message);
        }

        private static void CreateLogger() {
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Logs")))
                Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Logs"));

            if(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Log.txt")))
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Log.txt"));


            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Log.txt"))
            .WriteTo.Console()
            .CreateLogger();
        }

        private static bool ConnectToConsole(string Name)
        {
            try
            {
                xbCon = xbMgr.OpenConsole(Name);
                xbDebug = xbCon.DebugTarget;
                currentConsoleName = Name;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
