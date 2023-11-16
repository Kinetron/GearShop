namespace PriceUploader.Contracts
{
    /// <summary>
    /// Читает информацию введенную пользователем с клавиатуры,
    /// выполняет соответствующую команду.
    /// </summary>
    public interface ICommandExecutor
    {
        /// <summary>
        /// Запускает выполнение команды или передает введенную пользователем
        /// информацию работающей команде.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns></returns>
        ICommand ExecuteCommand(string userInput);
    }
}
