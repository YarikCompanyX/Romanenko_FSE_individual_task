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
using System.Collections.Generic;
using Avalonia.Controls.Primitives;

namespace Romanenko_FSE_individual_task
{
    public partial class MainWindow : Window
    {
        private readonly List<IAutoSave> _autoSaveObservers = new List<IAutoSave>();

        public void AttachAutoSaveObserver(IAutoSave observer)
        {
            if (!_autoSaveObservers.Contains(observer))
            {
                _autoSaveObservers.Add(observer);
            }
        }

        public void DetachAutoSaveObserver(IAutoSave observer)
        {
            _autoSaveObservers.Remove(observer);
        }

        private async Task NotifyAutoSaveObservers(string currentContent, IStorageFile? currentFile)
        {
            if (currentFile == null) return;

            var observersToNotify = _autoSaveObservers.ToList();
            foreach (var observer in observersToNotify)
            {
                try
                {
                    await observer.UpdateAutoSave(currentContent, currentFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying observer {observer.GetType().Name}: {ex.Message}");
                }
            }
        }

        private IStorageFile? _currentFile = null;

        public MainWindow()
        {
            InitializeComponent();
            var autoSaver = new AutoFileSaver(this);
            AttachAutoSaveObserver(autoSaver);

            SourceTextBox.TextChanged += SourceTextBox_TextChanged;
        }

        private bool _isSaving = false;
        private async void SourceTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_isSaving) return;

            _isSaving = true;
            try
            {
                await NotifyAutoSaveObservers(SourceTextBox.Text ?? "", _currentFile);
            }
            finally
            {
                _isSaving = false;
            }
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
                _currentFile = files[0];

                SourceTextBox.Text = "";
                string? filePathForDisplay = _currentFile.TryGetLocalPath() ?? _currentFile.Name;
                FilePathTextBox.Text = filePathForDisplay;

                try
                {
                    string? extension = Path.GetExtension(_currentFile.Name);
                    IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                    ILoader loader = factory.CreateLoader();

                    await using var stream = await _currentFile.OpenReadAsync();
                    string content = await loader.LoadAsync(stream);
                    SourceTextBox.Text = content;
                }
                catch (NotSupportedException nsex)
                {
                    _currentFile = null;
                    FilePathTextBox.Text = "";
                    SourceTextBox.Text = "";
                    await ShowMessageBoxAsync("Error", nsex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
                catch (Exception ex)
                {
                    _currentFile = null;
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
                SuggestedFileName = _currentFile?.Name ?? "Document",
                DefaultExtension = Path.GetExtension(_currentFile?.Name)?.TrimStart('.') ?? "txt",
                FileTypeChoices = new[] { txtType, binType, htmlType, FilePickerFileTypes.All },
                ShowOverwritePrompt = true
            });

            if (file is not null)
            {
                _currentFile = file;
                string? extension = Path.GetExtension(file.Name);
                string? filePathForDisplay = file.TryGetLocalPath() ?? file.Name;

                try
                {
                    IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                    ISaver saver = factory.CreateSaver();

                    string contentToSave = SourceTextBox.Text ?? "";

                    await using var stream = await _currentFile.OpenWriteAsync();
                    if (stream.CanSeek) stream.SetLength(0);
                    await saver.SaveAsync(stream, SourceTextBox.Text ?? "");

                    FilePathTextBox.Text = filePathForDisplay;

                    await ShowMessageBoxAsync("Save Successful", "File saved successfully!", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Info);
                }
                catch (NotSupportedException nsex)
                {
                    await ShowMessageBoxAsync("Save Error", nsex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
                catch (IOException ioex)
                {
                    await ShowMessageBoxAsync("Save Error", $"File access error while saving '{filePathForDisplay}': {ioex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
                catch (Exception ex)
                {
                    await ShowMessageBoxAsync("Save Error", $"Could not save file '{filePathForDisplay}': {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                }
            }
        }

        private void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public async Task ShowMessageBoxAsync(string title, string message, ButtonEnum buttons, Icon icon)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, icon);
            await msgBox.ShowWindowDialogAsync(this);
        }
    }
}