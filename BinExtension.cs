using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_lab12
{
    internal class BinExtension : IFileHandler
    {
        public async Task SaveFileAsync(Stream stream, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
