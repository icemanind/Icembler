using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Icembler.Converters
{
    public class LineFontToX1Converter80 : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var editor = parameter as CocoRichTextBox;

            if (editor == null) return 0;

            var formattedText = new FormattedText(
                "A",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(editor.FontFamily, editor.FontStyle, editor.FontWeight, editor.FontStretch),
                editor.FontSize,
                Brushes.Black,
                new NumberSubstitution(), 1);

            return formattedText.Width * 80 + 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
