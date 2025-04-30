using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class AutoFileSaver : IAutoSave
    {
        private readonly MainWindow _mainWindow;

        public AutoFileSaver(MainWindow mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public async Task UpdateAutoSave(string contentToSave, IStorageFile? fileToSave)
        {
            if (fileToSave == null)
            {
                return;
            }

            string? extension = Path.GetExtension(fileToSave.Name);
            string? filePathForDisplay = fileToSave.TryGetLocalPath() ?? fileToSave.Name;


            try
            {
                IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                ISaver saver = factory.CreateSaver();

                await using var stream = await fileToSave.OpenWriteAsync();

                if (stream.CanSeek) stream.SetLength(0);

                await saver.SaveAsync(stream, contentToSave);

                await _mainWindow.ShowMessageBoxAsync("AutoSave", $"Дані у файлі '{filePathForDisplay}' оновлено.", ButtonEnum.Ok, Icon.Info);

            }
            catch (NotSupportedException nsex)
            {
                Console.WriteLine($"AutoSave Error: {nsex.Message}");
                await _mainWindow.ShowMessageBoxAsync("AutoSave Error", $"Не вдалося автоматично зберегти файл: {nsex.Message}", ButtonEnum.Ok, Icon.Warning);
            }
            catch (IOException ioex)
            {
                Console.WriteLine($"AutoSave IO Error: {ioex.Message}");
                await _mainWindow.ShowMessageBoxAsync("AutoSave Error", $"Помилка доступу до файлу '{filePathForDisplay}' при автозбереженні: {ioex.Message}", ButtonEnum.Ok, Icon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoSave General Error: {ex.Message}");
                await _mainWindow.ShowMessageBoxAsync("AutoSave Error", $"Невідома помилка при автозбереженні файлу '{filePathForDisplay}': {ex.Message}", ButtonEnum.Ok, Icon.Error);
            }
        }
    }
}