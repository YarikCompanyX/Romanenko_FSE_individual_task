using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public interface ITextChanged
    {
        Task TextChangedUpdate(string previousText, string currentText, IStorageFile? currentFile, MainWindow mainWindow);
    }
}