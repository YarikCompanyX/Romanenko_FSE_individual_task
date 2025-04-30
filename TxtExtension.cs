using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_lab12
{
    internal class TxtExtension : IFileHandler
    {
        public async Task SaveFileAsync(Stream stream, string content)
        {
            await using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                await writer.WriteLineAsync(content);
            }
        }
    }
}
