using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class TxtLoader : ILoader
    {
        public async Task<string> LoadAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
    }
}