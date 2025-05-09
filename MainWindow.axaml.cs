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
        private readonly List<ITextChanged> _textChangedObservers = new List<ITextChanged>();
        private IStorageFile? _currentFile = null;
        private string _previousText = "";
        private bool _isProcessingTextChange = false;

        public MainWindow()
        {
            InitializeComponent();

            var autoSaver = new AutoFileSaver(this);
            AttachTextChangedObserver(autoSaver);

            var nameObserver = new NamePatternObserver();
            AttachTextChangedObserver(nameObserver);

            _previousText = SourceTextBox.Text ?? "";

            SourceTextBox.TextChanged += SourceTextBox_TextChanged;
        }

        public void AttachTextChangedObserver(ITextChanged observer)
        {
            if (!_textChangedObservers.Contains(observer))
            {
                _textChangedObservers.Add(observer);
            }
        }

        public void DetachTextChangedObserver(ITextChanged observer)
        {
            _textChangedObservers.Remove(observer);
        }

        private async Task NotifyTextChangedObservers(string previousText, string currentText, IStorageFile? currentFile)
        {
            var observersToNotify = _textChangedObservers.ToList();
            foreach (var observer in observersToNotify)
            {
                try
                {
                    await observer.TextChangedUpdate(previousText, currentText, currentFile, this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error notifying observer {observer.GetType().Name}: {ex.Message}");
                }
            }
        }

        private async void SourceTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_isProcessingTextChange) return;

            _isProcessingTextChange = true;
            try
            {
                string currentText = SourceTextBox.Text ?? "";

                // UpdateCounters(currentText);

                await NotifyTextChangedObservers(_previousText, currentText, _currentFile);

                _previousText = currentText;
            }
            finally
            {
                _isProcessingTextChange = false;
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

                    SourceTextBox.TextChanged -= SourceTextBox_TextChanged;
                    SourceTextBox.Text = content;
                    SourceTextBox.TextChanged += SourceTextBox_TextChanged;

                    _previousText = content;
                    // UpdateCounters(content);

                }
                catch (NotSupportedException nsex)
                {
                    _currentFile = null;
                    FilePathTextBox.Text = "";
                    SourceTextBox.Text = "";
                    await ShowMessageBoxAsync("Error", nsex.Message, ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    _previousText = "";
                    // UpdateCounters("");
                }
                catch (Exception ex)
                {
                    _currentFile = null;
                    FilePathTextBox.Text = "";
                    SourceTextBox.Text = "";
                    await ShowMessageBoxAsync("Error Opening File", $"Could not read or process file: {ex.Message}", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                    _previousText = "";
                    // UpdateCounters("");
                }
            }
        }

        private async void SaveAsMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourceTextBox.Text))
            {
                await ShowMessageBoxAsync("Cannot Save As", "There is no text content to save.", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Warning);
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var txtType = new FilePickerFileType("Text Document (*.txt)") { Patterns = new[] { "*.txt" } };
            var binType = new FilePickerFileType("Binary Data (*.bin)") { Patterns = new[] { "*.bin" } };
            var htmlType = new FilePickerFileType("HTML Document (*.html)") { Patterns = new[] { "*.html", "*.htm" } };

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
                string? extension = Path.GetExtension(_currentFile.Name);
                string? filePathForDisplay = _currentFile.TryGetLocalPath() ?? _currentFile.Name;

                try
                {
                    IFileHandlerFactory factory = FileHandlerProvider.GetFactory(extension);
                    ISaver saver = factory.CreateSaver();

                    string contentToSave = SourceTextBox.Text ?? "";

                    await using var stream = await _currentFile.OpenWriteAsync();
                    if (stream.CanSeek) stream.SetLength(0);
                    await saver.SaveAsync(stream, contentToSave);

                    _previousText = contentToSave;

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

        // private void UpdateCounters(string text)
        // {
        //     if (CharCountTextBlock == null || WordCountTextBlock == null) return;

        //     int charCount = text.Length;
        //     CharCountTextBlock.Text = $"Chars: {charCount}";
        //     string[] words = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        //     int wordCount = words.Length;
        //     WordCountTextBlock.Text = $"Words: {wordCount}";
        // }
    }
}