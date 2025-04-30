using System.IO;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public interface ILoader
    {
        Task<string> LoadAsync(Stream stream);
    }
}