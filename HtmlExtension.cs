using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Romanenko_FSE_lab12
{
    public class HtmlExtension : IFileHandler
    {
        public async Task SaveFileAsync(Stream stream, string content)
        {
            string[] paragraphs = Regex.Split(content ?? "", @"\r?\n");
            await using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                await writer.WriteLineAsync("<!DOCTYPE html>");
                await writer.WriteLineAsync("<html lang=\"ua\">");
                await writer.WriteLineAsync("\t<head>");
                await writer.WriteLineAsync("\t\t<meta charset=\"UTF-8\">");
                await writer.WriteLineAsync("\t\t<title>HTML Document</title>");
                await writer.WriteLineAsync("\t</head>");
                await writer.WriteLineAsync("\t<body>");

                foreach (string paragraph in paragraphs)
                {
                    if (!string.IsNullOrWhiteSpace(paragraph))
                    {
                        await writer.WriteLineAsync("\t\t<p>");
                        await writer.WriteLineAsync($"\t\t\t{paragraph}");
                        await writer.WriteLineAsync("\t\t</p>");
                    }
                }
                await writer.WriteLineAsync("\t</body>");
                await writer.WriteLineAsync("</html>");
            }
        }
    }
}
