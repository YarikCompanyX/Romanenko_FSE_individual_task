using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{

    internal class FileExtensionFactory
    {
        public static IFileHandler CreateHandler(string extensions)
        {
            switch (extensions.ToLower())
            {
                case ".txt":
                    return new TxtExtension();
                case ".bin":
                    return new BinExtension();
                case ".html":
                    return new HtmlExtension();
                default:
                    throw new ArgumentException($"Unsupported file extension: {extensions}");
            }
        }
    }
}
