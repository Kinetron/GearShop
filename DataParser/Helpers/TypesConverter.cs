using System.Globalization;

namespace DataParser.Helpers
{
    /// <summary>
    /// Вспомогательный инструмент для преобразования типов.
    /// </summary>
    public static class TypesConverter
    {
        /// <summary>
        ///  Конвертирует типы данных. Метод предназначен для работы с рефлексией.
        /// </summary>
        /// <param name="propertyToFill"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object ConvertTypes(object propertyToFill, string text)
        {
            object result;

            switch (propertyToFill)
            {
                case bool data:
                    result = ParseBoolean(text);
                    break;

                case byte data:
                    result = ParseByte(text);
                    break;

                case short data:
                    result = ParseShort(text);
                    break;

                case int data:
                    result = ParseInt32(text);
                    break;

                case decimal data:
                    result = ParseDecimal(text);
                    break;

                case DateTime dateTime:
                    result = ParseDateTime(text);
                    break;

                case string data:
                    result = text;
                    break;

                default:
                    throw new ArgumentException($"Give unknown tуpe: '{propertyToFill.GetType()}', value = '{text}'");
            }

            return result;
        }

        /// <summary>
        /// Пытается преобразовать текст в short, если не удача – возвращает null. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static object ParseShort(string text)
        {
            if (ushort.TryParse(text, out ushort shortResult))
            {
                return shortResult;
            }

            return null;
        }

        /// <summary>
        /// Пытается преобразовать текст в int, если не удача – возвращает null. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static object ParseInt32(string text)
        {
            if (int.TryParse(text, out int intResult))
            {
                return intResult;
            }

            return null;
        }

        /// <summary>
        /// Пытается преобразовать текст в byte, если не удача – возвращает null. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static object ParseByte(string text)
        {
            if (byte.TryParse(text, out byte byteResult))
            {
                return byteResult;
            }

            return null;
        }

        /// <summary>
        /// Пытается преобразовать текст в bool, если не удача – возвращает null.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static object ParseBoolean(string text)
        {
            if (bool.TryParse(text, out bool boolResult))
            {
                return boolResult;
            }

            return null;
        }

        /// <summary>
        /// Пытается преобразовать текст в DateTime, если не удача – возвращает null.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static object ParseDateTime(string text)
        {
            if (DateTime.TryParse(text, out DateTime dateValue))
            {
                return dateValue;
            }

            return null;
        }

        /// <summary>
        /// Пытается преобразовать текст в decimal, если не удача – возвращает null.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static object ParseDecimal(string text)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            NumberStyles style = NumberStyles.Number;

            text = text.Replace(',', '.');

            if (decimal.TryParse(text, style, culture, out decimal dicimalResult))
            {
                return dicimalResult;
            }

            return null;
        }
    }
}