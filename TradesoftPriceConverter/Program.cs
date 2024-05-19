namespace TradesoftPriceConverter
{
	internal class Program
	{

		private const string PriceFolder = "Files";
		private const string ResultFileName = "result.xls";

		static void Main(string[] args)
		{
			Console.WriteLine("Converting one format to another");
			
			PriceConverter converter = new PriceConverter(PrintText, PrintError);
			converter.ConvertFile(PriceFolder, ResultFileName);

			Console.WriteLine("Successfully");
		}

		/// <summary>
		/// Выводит в консоль ошибку.
		/// </summary>
		/// <param name="text"></param>
		private static void PrintError(string text)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(text);
			Console.ResetColor();
		}

		/// <summary>
		/// Выводит текст в консоль.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="cleanLine"></param>
		private static void PrintText(string text)
		{
			Console.Write(text);
		}
	}
}
