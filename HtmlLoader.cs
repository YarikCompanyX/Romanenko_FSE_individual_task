using HtmlAgilityPack;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Romanenko_FSE_individual_task
{
    public class HtmlLoader : ILoader
    {
        public async Task<string> LoadAsync(Stream stream)
        {
            var doc = new HtmlDocument();
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                await Task.Run(() => doc.Load(reader));
            }


            var sb = new StringBuilder();
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;

            ExtractTextNodes(bodyNode, sb);

            string result = sb.ToString().Trim();
            result = System.Text.RegularExpressions.Regex.Replace(result, @"(\r?\n){2,}", "\r\n");

            return result;
        }

        private void ExtractTextNodes(HtmlNode node, StringBuilder sb)
        {
            if (node == null) return;

            switch (node.NodeType)
            {
                case HtmlNodeType.Element:
                    string elementName = node.Name.ToLowerInvariant();

                    if (node.HasChildNodes)
                    {
                        foreach (var childNode in node.ChildNodes)
                        {
                            ExtractTextNodes(childNode, sb);
                        }
                    }

                    if (IsBlockLevelElement(elementName) || elementName == "br")
                    {
                        AddSingleLineBreakIfNeeded(sb);
                    }
                    break;

                case HtmlNodeType.Text:
                    string decodedText = WebUtility.HtmlDecode(node.InnerText);
                    string trimmedText = decodedText.Trim();

                    if (!string.IsNullOrWhiteSpace(trimmedText))
                    {
                        if (sb.Length > 0 && !char.IsWhiteSpace(sb[sb.Length - 1]) && !char.IsWhiteSpace(trimmedText[0]))
                        {
                            sb.Append(' ');
                        }
                        sb.Append(trimmedText);
                    }

                    break;

                case HtmlNodeType.Comment:
                case HtmlNodeType.Document:
                    break;
            }
        }

        private void EnsureParagraphBreak(StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                int len = sb.Length;
                bool endsWithDoubleNewline = (len >= 2 && sb[len - 1] == '\n' && sb[len - 2] == '\n') ||
                                             (len >= 4 && sb.ToString(len - 4, 4) == "\r\n\r\n");

                if (!endsWithDoubleNewline)
                {
                    if (sb[len - 1] == '\n' || (len >= 2 && sb.ToString(len - 2, 2) == "\r\n"))
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                }
            }
        }

        private void AddSingleLineBreakIfNeeded(StringBuilder sb)
        {
            if (sb.Length > 0)
            {
                int len = sb.Length;
                bool endsWithNewline = sb[len - 1] == '\n' || (len >= 2 && sb.ToString(len - 2, 2) == "\r\n");

                if (!endsWithNewline)
                {
                    sb.AppendLine();
                }
            }
        }

        private bool IsBlockLevelElement(string tagName)
        {
            string[] blockElements = {
                "p", "div", "h1", "h2", "h3", "h4", "h5", "h6",
                "ul", "ol", "li", "table", "tr", "td", "th", "blockquote",
                "pre", "hr", "form", "header", "footer", "section", "article",
                "aside", "address", "figure", "figcaption"
            };
            return Array.Exists(blockElements, element => element.Equals(tagName, StringComparison.OrdinalIgnoreCase));
        }
    }
}