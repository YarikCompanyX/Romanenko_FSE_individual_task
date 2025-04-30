using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_lab12
{
    public interface IFileHandler
    {
        Task SaveFileAsync(Stream stream, string content);
    }
}
