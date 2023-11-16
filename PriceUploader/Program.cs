using PriceUploader.Commands;
using PriceUploader.Contracts;
using PriceUploader.Services;
using System;
using System.Diagnostics;
using System.Windows.Input;
using PriceUploader.Controls;
using ICommand = PriceUploader.Contracts.ICommand;

namespace PriceUploader
{
    internal class Program
    {
        private static ProgressBar _progressBar;

        static void Main(string[] args)
        {
            List<ICommand> allowCommands = CreateCommands(); //Доступные пользователю команды.
            ICommandExecutor executor = new CommandExecutor(allowCommands, PrintText, PrintError);

            ShowInfoHeader(allowCommands);

            CommandsHandler(executor);
            
            Console.WriteLine(
                $"{Environment.NewLine}@Skynet Спасибо что воспользовались нашей программой.{Environment.NewLine}");
        }

        /// <summary>
        /// Обрабатывает введенные пользователем команды.
        /// </summary>
        /// <param name="executor"></param>
        private static void CommandsHandler(ICommandExecutor executor)
        {
            string currentCommandName = string.Empty;
            string exitCommandName = new Exit().Name;

            do
            {
                ICommand command = executor.ExecuteCommand(Console.ReadLine());
                if (command == null) continue; //Получена не известная команда.

                currentCommandName = command.Name;
            }
            while (currentCommandName != exitCommandName);
        }

        /// <summary>
        /// Выводит информацию о программе и доступных командах.
        /// </summary>
        private static void ShowInfoHeader(List<ICommand> allowCommands)
        {
            Console.WriteLine("Загрузка прайс листа на сервер.");
            ShowAllowCommand(allowCommands);
            Console.WriteLine("Введите команду и нажмите Enter");
        }

        /// <summary>
        /// Выводит в консоль список доступных команд.
        /// </summary>
        /// <param name="allowCommands"></param>
        private static void ShowAllowCommand(List<ICommand> allowCommands)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            foreach (ICommand command in allowCommands)
            {
                Console.WriteLine($"Команда {command.Name} {command.Description}.");
            }

            Console.ResetColor();
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

        /// <summary>
        /// Формирую строку с информацией о прогрессе вывожу пользователю.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        private static void PrintProgress(int current, int total)
        {
            if (_progressBar == null)
            {
                Console.Write($"Обработка ");
                _progressBar = new ProgressBar();
            }

            _progressBar.Report((double)current / total);
            if (current == total)
            {
                _progressBar.Dispose();
                _progressBar = null;
            }
        }

        /// <summary>
        /// Создает команды, доступные пользователю.
        /// </summary>
        /// <returns></returns>
        private static List<ICommand> CreateCommands()
        {
            return new List<ICommand>()
            {
                new Upload(PrintText, PrintError, PrintProgress),
                new Exit()
            };
        }
    }
}
