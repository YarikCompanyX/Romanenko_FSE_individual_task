using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public interface IAutoSave
    {
        Task UpdateAutoSave(string contentToSave, IStorageFile? fileToSave);
    }
}