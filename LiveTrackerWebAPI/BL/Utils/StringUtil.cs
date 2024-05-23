using System.Globalization;
using System.Text;

namespace BL.Utils;

public static class StringUtil
{
    public static string RemoveDiacritics(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in from c in normalizedString let unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c) 
                 where unicodeCategory != UnicodeCategory.NonSpacingMark select c)
        {
            stringBuilder.Append(c);
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}