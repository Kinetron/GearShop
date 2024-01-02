using DataParser.Models;
using DataParser;
using PriceUploader.Contracts;
using System.IO;
using System.Diagnostics;
using DataParser.Services;
using RemoteControlApi;

namespace PriceUploader.Commands
{
    public class Parse : ICommand
    {
        private readonly Action<string> _sendTextToUser;
        private readonly Action<string> _sendErrorToUser;
        private readonly Action<int, int> _printProgress;
        public string Name { get; } = "ps";
        public string Description { get; } = "Загрузка прайс листа на сервер.";

        public Parse(Action<string> sendTextToUser, Action<string> sendErrorToUser, Action<int, int> printProgress)
        {
            _sendTextToUser = sendTextToUser;
            _sendErrorToUser = sendErrorToUser;
            _printProgress = printProgress;
        }
        public void Run()
        {
	        RemoteApi remote = new RemoteApi(_sendTextToUser, _sendErrorToUser, _printProgress);
	        if (!remote.ParseData().Result)
	        {
		        _sendErrorToUser(remote.LastError);
	        }
	        EventEndWork?.Invoke();
		}

        public void ReadUserInput(string text)
        {
            
        }

        public event Action? EventEndWork;
    }
}
