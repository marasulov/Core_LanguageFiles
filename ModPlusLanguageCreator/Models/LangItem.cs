using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ModPlusLanguageCreator.Models
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LangItem
    {
        public LangItem(CultureInfo culture, string file)
        {
            Name = culture.Name;
            DisplayName = culture.DisplayName;
            TwoLetterISOLanguageName = culture.TwoLetterISOLanguageName;
            FileName = file;
        }
        /// <summary>Имя языка вида ru-RU</summary>
        public string Name { get; }
        /// <summary>Отображаемое имя языка</summary>
        public string DisplayName { get; }
        /// <summary>Имя локального файла для языка</summary>
        public string FileName { get; set; }

        public string TwoLetterISOLanguageName { get; }
    }
}
