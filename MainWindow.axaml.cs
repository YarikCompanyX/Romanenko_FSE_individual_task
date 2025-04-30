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
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.All }
            });

            if (files.Count >= 1)
            {
                var file = files[0];

                SourceTextBox.Text = "";
                FilePathTextBox.Text = file.Name;

                try
                {
                    if (file.TryGetLocalPath() is string path)
                    {
                        FilePathTextBox.Text = path;
                    }

                    await using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream, Encoding.UTF8);

                    string content = await reader.ReadToEndAsync();
                    SourceTextBox.Text = content;
                }
                catch (Exception ex)
                {
                    FilePathTextBox.Text = "";
                    await ShowMessageBoxAsync("Error Opening File", $"Could not read file: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            }
        }

        private async void SaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SourceTextBox.Text))
            {
                await ShowMessageBoxAsync("Cannot Save", "There is no text content to save.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var txtType = new FilePickerFileType("Text Document") { Patterns = new[] { "*.txt" } };
            var binType = new FilePickerFileType("Binary Data") { Patterns = new[] { "*.bin" } };
            var htmlType = new FilePickerFileType("HTML Document") { Patterns = new[] { "*.html" } };

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Text As...",
                SuggestedFileName = "Text",
                DefaultExtension = "txt",
                FileTypeChoices = new[] { txtType, binType, htmlType },
                ShowOverwritePrompt = true
            });

            if (file is not null)
            {
                string? extension = Path.GetExtension(file.Name)?.ToLowerInvariant();

                try
                {
                    IFileHandler fileHandler = FileExtensionFactory.CreateHandler(extension ?? ".txt");

                    await using var stream = await file.OpenWriteAsync();

                    await fileHandler.SaveFileAsync(stream, SourceTextBox.Text ?? "");

                    if (file.TryGetLocalPath() is string path)
                    {
                        FilePathTextBox.Text = path;
                    }
                    else
                    {
                        FilePathTextBox.Text = file.Name + " (Saved)";
                    }

                    await ShowMessageBoxAsync("Save Successful", "File saved successfully!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
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