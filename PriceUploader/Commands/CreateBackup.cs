using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PriceUploader.Contracts;
using RemoteControlApi;

namespace PriceUploader.Commands
{
	/// <summary>
	/// Create backups web site.
	/// Создает бэкапы web cайта.
	/// </summary>
	public class CreateBackup : ICommand
	{
		public string Name { get; } = "bk";
		public string Description { get; } = "Создает бэкапы web cайта.";

		public event Action EventEndWork;
		
		private readonly Action<string> _sendTextToUser;
		private readonly Action<string> _sendErrorToUser;
		private readonly Action<int, int> _printProgress;

		public CreateBackup(Action<string> sendTextToUser, Action<string> sendErrorToUser, Action<int, int> printProgress)
		{
			_sendTextToUser = sendTextToUser;
			_sendErrorToUser = sendErrorToUser;
			_printProgress = printProgress;
		}

		public void Run()
		{
			_sendTextToUser($"Будет создан бэкап сайта.{Environment.NewLine}");

			RemoteApi api = new RemoteApi(_sendTextToUser, _sendErrorToUser, _printProgress);
			api.DownloadRootFiles();

			EventEndWork?.Invoke();
		}

		public void ReadUserInput(string text)
		{
		}
	}
}
