using SharpCompress.Archives;
using System;
using System.Collections.Generic;
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

                Dictionary<string,string> codes = new Dictionary<string, string>();

                codes.Add("0", "0 More Than 10 sec");
                codes.Add("100", "100 Continue");
                codes.Add("101", "101 Switching Protocol");
                codes.Add("102", "102 Processing");
                codes.Add("103", "103 Early Hints");
                codes.Add("200", "200 OK");
                codes.Add("201", "201 Created");
                codes.Add("202", "202 Accepted");
                codes.Add("203", "203 Non-Authoritative Information");
                codes.Add("204", "204 No Content");
                codes.Add("205", "205 Reset Content");
                codes.Add("206", "206 Partial Content");
                codes.Add("207", "207 Multi-Status");
                codes.Add("208", "208 Already Reported");
                codes.Add("226", "226 IM Used");
                codes.Add("300", "300 Multiple Choice");
                codes.Add("301", "301 Moved Permanently");
                codes.Add("302", "302 Found");
                codes.Add("303", "303 See Other");
                codes.Add("304", "304 Not Modified");
                codes.Add("305", "305 Use Proxy");
                codes.Add("306", "306 unused");
                codes.Add("307", "307 Temporary Redirect");
                codes.Add("308", "308 Permanent Redirect");
                codes.Add("400", "400 Bad Request");
                codes.Add("401", "401 Unauthorized");
                codes.Add("402", "402 Payment Required");
                codes.Add("403", "403 Forbidden");
                codes.Add("404", "404 Not Found");
                codes.Add("405", "405 Method Not Allowed");
                codes.Add("406", "406 Not Acceptable");
                codes.Add("407", "407 Proxy Authentication Required");
                codes.Add("408", "408 Request Timeout");
                codes.Add("409", "409 Conflict");
                codes.Add("410", "410 Gone");
                codes.Add("411", "411 Length Required");
                codes.Add("412", "412 Precondition Failed");
                codes.Add("413", "413 Payload Too Large");
                codes.Add("414", "414 URI Too Long");
                codes.Add("415", "415 Unsupported Media Type");
                codes.Add("416", "416 Range Not Satisfiable");
                codes.Add("417", "417 Expectation Failed");
                codes.Add("418", "418 I'm a teapot");
                codes.Add("421", "421 Misdirected Request");
                codes.Add("422", "422 Unprocessable Entity");
                codes.Add("423", "423 Locked");
                codes.Add("424", "424 Failed Dependency");
                codes.Add("425", "425 Too Early");
                codes.Add("426", "426 Upgrade Required");
                codes.Add("428", "428 Precondition Required");
                codes.Add("429", "429 Too Many Requests");
                codes.Add("431", "431 Request Header Fields Too Large");
                codes.Add("451", "451 Unavailable For Legal Reasons");
                codes.Add("500", "500 Internal Server Error");
                codes.Add("501", "501 Not Implemented");
                codes.Add("502", "502 Bad Gateway");
                codes.Add("503", "503 Service Unavailable");
                codes.Add("504", "504 Gateway Timeout");
                codes.Add("505", "505 HTTP Version Not Supported");
                codes.Add("506", "506 Variant Also Negotiates");
                codes.Add("507", "507 Insufficient Storage");
                codes.Add("508", "508 Loop Detected");
                codes.Add("510", "510 Not Extended");
                codes.Add("511", "511 Network Authentication Required");




                var form1 = new Form1();

                string[] files = Directory.GetFiles(args[0])
                    .Where(file => file.ToLower().EndsWith("xml") || file.ToLower().EndsWith("7z"))
                    .ToArray();

                var stamp = DateTime.Now.ToString("s").Replace(":","");

                var servererrorsbyfrebCsvFiltered = $@"c:\temp\ServerErrorsE2E_{stamp}_Filtered.csv";
                var servererrorsbyfrebCsvRaw = $@"c:\temp\ServerErrorsE2E_{stamp}_Raw.csv";
                
                string sep = "|";
                string header = $"sep={sep}\r\nstatus{sep}endpoint{sep}userName{sep}fullUrl{sep}createdLcl{sep}createdUtc{sep}failureReason{sep}milliseconds{sep}response{sep}authenticationType{sep}userAgent{sep}verb{sep}appPool{sep}processId{sep}server{sep}file\r\n";
                File.AppendAllText(servererrorsbyfrebCsvFiltered,header);
                File.AppendAllText(servererrorsbyfrebCsvRaw,header);
                
                for (var index = 0; index < files.Length; index++)
                {
                    Console.WriteLine($"{index+1}/{files.Length}");

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

                    if (codes.TryGetValue(triggerStatusCode,out var lookedup))
                    {
                        triggerStatusCode = lookedup;
                    }

                    var createdLcl = DateTime.Parse(created).ToString("s");

                    if (originalFile.ToLower().EndsWith("7z"))
                    {
                        File.Delete(filePotentialUnzipped);
                    }

                    response = response.Replace("\r", "").Replace("\n", "");

                    var server = Environment.MachineName;
                    var dataTemplate = $"{triggerStatusCode}{sep}{lastSegment}{sep}{userName}{sep}{url}{sep}{createdLcl}{sep}{created}{sep}{failureReason}{sep}{timeTaken}{sep}{response}{sep}{authenticationType}{sep}{userAgent}{sep}{verb}{sep}{appPool}{sep}{processId}{sep}{server}{sep}{filePotentialUnzipped}\r\n";

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
