using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;
using System.Text;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.All }
            });

            if (files.Count >= 1)
            {
                var file = files[0];

                SourceTextBox.Text = "";
                string? filePathForDisplay = file.TryGetLocalPath() ?? file.Name;
                FilePathTextBox.Text = filePathForDisplay;

                try
                {
                    string? extension = Path.GetExtension(file.Name);
                    IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                    ILoader loader = factory.CreateLoader();

                    await using var stream = await file.OpenReadAsync();
                    string content = await loader.LoadAsync(stream);
                    SourceTextBox.Text = content;
                }
                catch (NotSupportedException nsex)
                {
                    FilePathTextBox.Text = "";
                    SourceTextBox.Text = "";
                    await ShowMessageBoxAsync("Error", nsex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
                catch (Exception ex)
                {
                    FilePathTextBox.Text = "";
                    SourceTextBox.Text = "";
                    await ShowMessageBoxAsync("Error Opening File", $"Could not read or process file: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            }
        }

        private async void SaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
            {
                await ShowMessageBoxAsync("Cannot Save", "There is no text content to save.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var txtType = new FilePickerFileType("Text Document (*.txt)") { Patterns = new[] { "*.txt" } };
            var binType = new FilePickerFileType("Binary Data (*.bin)") { Patterns = new[] { "*.bin" } };
            var htmlType = new FilePickerFileType("HTML Document (*.html)") { Patterns = new[] { "*.html" } };

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Content As...",
                SuggestedFileName = Path.GetFileNameWithoutExtension(FilePathTextBox.Text) ?? "Document",
                DefaultExtension = Path.GetExtension(FilePathTextBox.Text)?.TrimStart('.') ?? "txt",
                FileTypeChoices = new[] { txtType, binType, htmlType, FilePickerFileTypes.All },
                ShowOverwritePrompt = true
            });

            if (file is not null)
            {
                string? extension = Path.GetExtension(file.Name);
                string? filePathForDisplay = file.TryGetLocalPath() ?? file.Name;

                try
                {
                    IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                    ISaver saver = factory.CreateSaver();

                    await using var stream = await file.OpenWriteAsync();
                    await saver.SaveAsync(stream, SourceTextBox.Text ?? "");

                    FilePathTextBox.Text = filePathForDisplay;

                    await ShowMessageBoxAsync("Save Successful", "File saved successfully!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                }
                catch (NotSupportedException nsex)
                {
                    await ShowMessageBoxAsync("Error", nsex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
                catch (Exception ex)
                {
                    await ShowMessageBoxAsync("Save Error", $"Could not save file: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            }
        }

        private void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async Task ShowMessageBoxAsync(string title, string message, ButtonEnum buttons, Icon icon)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, icon);
            await msgBox.ShowWindowDialogAsync(this);
        }
    }
}