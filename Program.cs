using SharpCompress.Archives;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FREBUI
{
    internal static class Program
    {
        // Defines for commandline output needed to make this WinForm also act as a console app
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        private static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run((Form) new Form1());
            }
            else
            {

                // Redirect console output to parent process. Must be before any calls to Console.WriteLine()
                // Needed to make this winform also act as a console app
                AttachConsole(ATTACH_PARENT_PROCESS);

                var form1 = new Form1();

                string[] files = Directory.GetFiles(args[0])
                    .Where(file => file.ToLower().EndsWith("xml") || file.ToLower().EndsWith("7z"))
                    .ToArray();

                var servererrorsbyfrebCsvFiltered = $@"c:\temp\ServerErrorsE2E_Filtered_{DateTime.Now.ToString("s").Replace(":","")}.csv";
                var servererrorsbyfrebCsvRaw = $@"c:\temp\ServerErrorsE2E_Raw_{DateTime.Now.ToString("s").Replace(":","")}.csv";
                
                string sep = "|";
                string header = $"sep={sep}\r\nstatus{sep}endpoint{sep}userName{sep}fullUrl{sep}created{sep}failureReason{sep}milliseconds{sep}response{sep}authenticationType{sep}userAgent{sep}verb{sep}appPool{sep}processId{sep}server{sep}file\r\n";
                File.AppendAllText(servererrorsbyfrebCsvFiltered,header);
                File.AppendAllText(servererrorsbyfrebCsvRaw,header);
                
                for (var index = 0; index < files.Length; index++)
                {
                    Console.WriteLine($"{index}/{files.Length}");

                    var originalFile = files[index];
                    string filePotentialUnzipped = originalFile;

                    if (originalFile.ToLower().EndsWith("7z"))
                    {
                        using(var archive = ArchiveFactory.Open(originalFile)){
                        
                            foreach (var entry in archive.Entries)
                            {
                                entry.WriteToDirectory(@"C:\temp");
                                filePotentialUnzipped = @"C:\temp\" + entry.Key;
                            }
                        }
                    }

                    form1.GetDetailsFromFREBFile(filePotentialUnzipped, out string url, out string verb,
                        out string appPool,
                        out string statusCode, out int timeTaken, out string created, out string userAgent,
                        out bool headless,
                        out string remote, out string response, out string processId, out string remoteUserName,
                        out string userName, out string authenticationType, out string failureReason,
                        out string triggerStatusCode,
                        out string lastSegment);

                    created = DateTime.Parse(created).ToString("s");

                    if (originalFile.ToLower().EndsWith("7z"))
                    {
                        File.Delete(filePotentialUnzipped);
                    }

                    response = response.Replace("\r", "").Replace("\n", "");

                    var server = Environment.MachineName;
                    var dataTemplate = $"{triggerStatusCode}{sep}{lastSegment}{sep}{userName}{sep}{url}{sep}{created}{sep}{failureReason}{sep}{timeTaken}{sep}{response}{sep}{authenticationType}{sep}{userAgent}{sep}{verb}{sep}{appPool}{sep}{processId}{sep}{server}{sep}{filePotentialUnzipped}\r\n";

                    File.AppendAllText(servererrorsbyfrebCsvRaw,dataTemplate);

                    if (headless
                        && lastSegment != "connect"
                        && statusCode != "304"
                    )
                    {
                        File.AppendAllText(servererrorsbyfrebCsvFiltered,dataTemplate);
                    }

                    File.Delete(originalFile);
                }

                Process.Start(servererrorsbyfrebCsvRaw);
                Process.Start(servererrorsbyfrebCsvFiltered);


            }
        }
    }
}
