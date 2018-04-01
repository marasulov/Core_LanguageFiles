using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ModPlusLanguageCreator.Helpers
{
    public class VersionValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = Convert.ToString(value);
            if (!string.IsNullOrEmpty(str))
            {
                if(Version.TryParse(str, out _))
                    return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Version must be valid 0.0.0.0");
        }
    }
}
