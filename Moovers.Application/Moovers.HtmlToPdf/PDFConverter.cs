using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Moovers.HtmlToPdf
{
    public class PDFGenerateException : Exception
    {
        public PDFGenerateException(string msg) : base(msg) { }
    }

    public enum PDFOrientation
    {
        Portrait,

        // ReSharper disable once UnusedMember.Global
        Landscape
    }

    public class PDFConverter
    {
        /// <summary>
        /// location of the "wkhtmltopdf" executable
        /// </summary>
        private static readonly string Exepath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "wkhtmltopdf", "wkhtmltopdf.exe");

        public byte[] Convert(string html, System.Drawing.Printing.PaperKind paperKind, PDFOrientation orientation)
        {
            var psi = new ProcessStartInfo {
                FileName = Exepath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.WorkingDirectory = Path.GetDirectoryName(psi.FileName);

            // arguments to wkhtmltopdf
            var arguments = new [] {
                "-q", // quiet
                "-n", // disable javascript
                "--disable-smart-shrinking", // Disable the intelligent shrinking strategy used by WebKit that makes the pixel/dpi ratio constant
                "--orientation " + orientation.ToString(), // Landscape or Portrait
                "--outline-depth 0", // Set the depth of the outline (default 4)
                "-s " + paperKind.ToString(), // Set paper size to: A4, Letter, etc.
                "- -" // from a comment on https://code.google.com/p/wkhtmltopdf/wiki/Usage -- using "- -" as an argument reads from stdin and writes to stdout
            };

            var args = String.Join(" ", arguments);
            psi.Arguments = args;
            Process p;
            using (p = Process.Start(psi))
            {
                // write html to process stdin
                using (var stdin = p.StandardInput)
                {
                    stdin.AutoFlush = true;
                    stdin.Write(html);
                }
                
                // read stdout 32k at a time
                var buffer = new byte[32768];
                byte[] file;
                using (var ms = new MemoryStream())
                {
                    using (var stdout = p.StandardOutput.BaseStream)
                    {
                        while (true)
                        {
                            int read = stdout.Read(buffer, 0, buffer.Length);
                            if (read <= 0)
                            {
                                break;
                            }

                            ms.Write(buffer, 0, read);
                        }                        
                    }

                    file = ms.ToArray();
                }


                // wait max of 1 minute for process to return
                var oneMinute = 60000;
                p.WaitForExit(oneMinute);
                
                // on success, return file
                int returnCode = p.ExitCode;
                if (returnCode == 0)
                {
                    return file;
                }

                // on error, throw an error so this is logged
                throw new PDFGenerateException("Could not create PDF, return code: " + returnCode);
            }
        }
    }
}
