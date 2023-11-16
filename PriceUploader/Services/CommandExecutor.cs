using PriceUploader.Contracts;

namespace PriceUploader.Services
{
    /// <summary>
    /// Читает информацию введенную пользователем с клавиатуры,
    /// вызывает соответствующую команду.
    /// </summary>
    public class CommandExecutor : ICommandExecutor
    {
        /// <summary>
        /// Доступные для обработки команды.
        /// </summary>
        private readonly List<ICommand> _commands;

        /// <summary>
        /// Отправляет сообщение об ошибке пользователю.
        /// </summary>
        private readonly Action<string> _sendErrorToUser;

        /// <summary>
        /// Отправка текста пользователю.
        /// </summary>
        private readonly Action<string> _sendTextToUser;

        /// <summary>
        /// Текущая выполняемая команда.
        /// </summary>
        private ICommand _currentCommand;

        /// <summary>
        /// Работает ли какая либо команда?
        /// </summary>
        private bool _isCommandRunning;

        public CommandExecutor(List<ICommand> commands, Action<string> sendTextToUser, Action<string> sendErrorToUser)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _sendErrorToUser = sendErrorToUser ?? throw new ArgumentNullException(nameof(sendErrorToUser));
            _sendTextToUser = sendTextToUser ?? throw new ArgumentNullException(nameof(sendTextToUser));
        }

        /// <summary>
        /// Запускает выполнение команды или передает введенную пользователем
        /// информацию работающей команде.
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns></returns>
        public ICommand ExecuteCommand(string userInput)
        {
            string command = userInput.Trim().ToLower();

            if (_isCommandRunning) //Находимся в режиме передачи текста от пользователя команде.
            {
                _currentCommand.ReadUserInput(command);
            }
            else //Находимся в режиме ввода текста команды.
            {
                return RunCommand(command);
            }

            return null;
        }

        /// <summary>
        /// Запускает команду.
        /// </summary>
        /// <returns></returns>
        private ICommand RunCommand(string commandName)
        {
            if (!_commands.Select(x=>x.Name).Contains(commandName))
            {
                _sendErrorToUser($"Не известная команда {commandName} ");
                return null;
            }

            _currentCommand = _commands.First(x => x.Name == commandName);
            _currentCommand.EventEndWork += OnCommandEndWork;
            _isCommandRunning = true;
            Task.Run(() => { _currentCommand.Run(); });
            
            return _currentCommand;
        }

        /// <summary>
        /// Обработка события окончания выполнения команды.
        /// </summary>
        private void OnCommandEndWork()
        {
            _sendTextToUser($"[ОК]{Environment.NewLine}");

            //Возвращаемся в исходное состояние.
            _currentCommand.EventEndWork -= OnCommandEndWork;
            _isCommandRunning = false;
        }
    }
}
