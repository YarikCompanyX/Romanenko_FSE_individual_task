using HtmlAgilityPack;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Romanenko_FSE_individual_task
{
    public class HtmlSaver : ISaver
    {
        public async Task SaveAsync(Stream stream, string content)
        {
            var doc = new HtmlDocument();

            HtmlNode htmlNode = doc.CreateElement("html");
            doc.DocumentNode.AppendChild(htmlNode);
            htmlNode.SetAttributeValue("lang", "uk");

            HtmlNode headNode = doc.CreateElement("head");
            htmlNode.AppendChild(headNode);

            HtmlNode metaNode = doc.CreateElement("meta");
            metaNode.SetAttributeValue("charset", "UTF-8");
            headNode.AppendChild(metaNode);

            HtmlNode titleNode = doc.CreateElement("title");
            titleNode.InnerHtml = "Generated document";
            headNode.AppendChild(titleNode);

            HtmlNode bodyNode = doc.CreateElement("body");
            htmlNode.AppendChild(bodyNode);

            string[] paragraphs = (content ?? "").Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string paragraph in paragraphs)
            {
                HtmlNode pNode = doc.CreateElement("p");
                string trimmedParagraph = paragraph.Trim();
                if (string.IsNullOrEmpty(trimmedParagraph))
                {
                    pNode.InnerHtml = "";
                }
                else
                {
                    pNode.InnerHtml = WebUtility.HtmlEncode(trimmedParagraph);
                }

                bodyNode.AppendChild(pNode);
            }

            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true))
                {
                    doc.Save(writer);
                }
            });
        }
    }
}