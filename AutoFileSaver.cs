using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class AutoFileSaver : ITextChanged
    {
        private readonly MainWindow _mainWindow;
        private static readonly string[] _newlineSeparators = { "\r\n", "\r", "\n" };

        public AutoFileSaver(MainWindow mainWindow)
        {
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        }

        public async Task TextChangedUpdate(string previousText, string currentText, IStorageFile? fileToSave, MainWindow mainWindow)
        {
            if (!DidParagraphCountIncrease(previousText, currentText))
            {
                return;
            }

            Console.WriteLine("[AutoFileSaver] The number of paragraphs has increased");

            if (fileToSave == null)
            {
                Console.WriteLine("AutoSave skipped: file is not specified.");
                return;
            }

            string? extension = Path.GetExtension(fileToSave.Name);
            string? filePathForDisplay = fileToSave.TryGetLocalPath() ?? fileToSave.Name;

            Console.WriteLine($"[AutoFileSaver] AutoSave triggered for: {filePathForDisplay}");

            try
            {
                IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                ISaver saver = factory.CreateSaver();

                await using var stream = await fileToSave.OpenWriteAsync();
                if (stream.CanSeek) stream.SetLength(0);

                await saver.SaveAsync(stream, currentText);

                Console.WriteLine($"[AutoFileSaver] File data '{filePathForDisplay}' successfully updated (autosaving)");
            }
            catch (NotSupportedException nsex)
            {
                Console.WriteLine($"AutoSave Error: {nsex.Message}");
                await mainWindow.ShowMessageBoxAsync("AutoSave Error", $"Не вдалося автоматично зберегти файл: {nsex.Message}", ButtonEnum.Ok, Icon.Warning);
            }
            catch (IOException ioex)
            {
                Console.WriteLine($"AutoSave IO Error: {ioex.Message}");
                await mainWindow.ShowMessageBoxAsync("AutoSave Error", $"Помилка доступу до файлу '{filePathForDisplay}' при автозбереженні: {ioex.Message}", ButtonEnum.Ok, Icon.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoSave General Error: {ex.Message}");
                await mainWindow.ShowMessageBoxAsync("AutoSave Error", $"Невідома помилка при автозбереженні файлу '{filePathForDisplay}': {ex.Message}", ButtonEnum.Ok, Icon.Error);
            }
        }

        private bool DidParagraphCountIncrease(string previousText, string currentText)
        {
            int previousParagraphCount = previousText.Split(_newlineSeparators, StringSplitOptions.None).Length;
            int currentParagraphCount = currentText.Split(_newlineSeparators, StringSplitOptions.None).Length;

            return currentParagraphCount > previousParagraphCount;
        }
    }
}