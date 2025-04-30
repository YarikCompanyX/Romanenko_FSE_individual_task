using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class BinLoader : ILoader
    {
        public async Task<string> LoadAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] bytes = memoryStream.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }
    }
}