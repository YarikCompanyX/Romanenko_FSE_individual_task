using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class TxtSaver : ISaver
    {
        public async Task SaveAsync(Stream stream, string content)
        {
            await using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
            await writer.WriteAsync(content ?? "");
        }
    }
}