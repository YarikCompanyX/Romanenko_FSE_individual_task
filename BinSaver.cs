using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class BinSaver : ISaver
    {
        public async Task SaveAsync(Stream stream, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content ?? "");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}