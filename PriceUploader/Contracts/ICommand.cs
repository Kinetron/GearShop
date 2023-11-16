namespace PriceUploader.Contracts
{
    /// <summary>
    /// Команда хранилища.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Имя команды.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Описание действия команды. Например: "Команда add инициализирует ввод данных транзакции".
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Запускает выполнение команды.
        /// </summary>
        void Run();

        /// <summary>
        /// Обрабатывает введенный пользователем текст.
        /// </summary>
        /// <param name="text"></param>
        void ReadUserInput(string text);

        /// <summary>
        /// Событие окончания обработки команды.
        /// </summary>
        event Action EventEndWork;
    }
}
