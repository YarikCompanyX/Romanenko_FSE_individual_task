using System.IO;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public interface ISaver
    {
        Task SaveAsync(Stream stream, string content);
    }
}