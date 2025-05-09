using Avalonia.Platform.Storage;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Romanenko_FSE_individual_task
{
    public class NamePatternObserver : ITextChanged
    {
        private static readonly Regex NameRegex = new Regex(
            @"\b[А-ЯІЇЄҐ][а-яіїєґ']+ [А-ЯІЇЄҐ]\. [А-ЯІЇЄҐ]\.(?=\s|$)",
            RegexOptions.Compiled);

        private readonly HashSet<string> _detectedNames = new HashSet<string>();

        public async Task TextChangedUpdate(string previousText, string currentText, IStorageFile? currentFile, MainWindow mainWindow)
        {
            MatchCollection currentMatches = NameRegex.Matches(currentText);

            foreach (Match match in currentMatches)
            {
                if (_detectedNames.Add(match.Value))
                {
                    Console.WriteLine($"[NamePatternObserver] Виявлено нове ПІБ: {match.Value}");
                }
            }
            await Task.CompletedTask;
        }
    }
}