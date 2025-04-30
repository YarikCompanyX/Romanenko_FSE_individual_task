using System;

namespace Romanenko_FSE_individual_task
{
    public static class FileHandlerProvider
    {
        public static IFileHandlerFactory GetFactory(string? fileExtension)
        {
            switch (fileExtension?.ToLowerInvariant())
            {
                case ".txt":
                    return new TxtFactory();
                case ".bin":
                    return new BinFactory();
                case ".html":
                case ".htm":
                    return new HtmlFactory();
                default:
                    throw new NotSupportedException($"File extension '{fileExtension}' is not supported.");
            }
        }
    }
}