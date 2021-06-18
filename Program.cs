using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FREBUI
{
    internal static class Program
    {

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

                var form1 = new Form1();

                string[] files = Directory.GetFiles(args[0],"*.xml");
                string sep = "|";
                List<string> lines = new List<string>();
                foreach (var file in files)
                {
                    form1.GetDetailsFromFREBFile(file, out string url, out string verb, out string appPool,
                        out string statusCode, out int timeTaken, out string created, out string userAgent,
                        out bool headless,
                        out string remote, out string response, out string processId, out string remoteUserName,
                        out string userName, out string authenticationType, out string failureReason,
                        out string triggerStatusCode,
                        out string lastSegment);

                    response = response.Replace("\r", "").Replace("\n", "");

                    if (headless
                        && lastSegment != "connect"
                        && statusCode != "304"
                    )
                    {
                        lines.Add($"{triggerStatusCode}{sep}{statusCode}{sep}{lastSegment}{sep}{headless}{sep}{created}{sep}{failureReason}{sep}{timeTaken}{sep}{remote}{sep}{response}{sep}{remoteUserName}{sep}{userName}{sep}{authenticationType}{sep}{userAgent}{sep}{url}{sep}{verb}{sep}{appPool}{sep}{processId}{sep}{file}");
                    }
                    
                }

                lines.Sort();
                lines.Reverse();
                
                string header = $"sep={sep}\r\ntriggerStatusCode{sep}statusCode{sep}Endpoint{sep}headless{sep}created{sep}failureReason{sep}timeTaken{sep}remote{sep}response{sep}remoteUserName{sep}userName{sep}authenticationType{sep}userAgent{sep}url{sep}verb{sep}appPool{sep}processId{sep}file\r\n";
                lines.Insert(0,header);

                string linesAsText = lines.Aggregate((x,y) => x + y + "\r\n");

                var servererrorsbyfrebCsv = $@"ServerErrorsbyFREB{DateTime.Now.ToString("s").Replace(":","")}.csv";

                File.WriteAllText(servererrorsbyfrebCsv,linesAsText);

                Process.Start(servererrorsbyfrebCsv);

            }
        }
    }
}
