using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriceUploader.Contracts;

namespace PriceUploader.Commands
{
    public class Exit : ICommand
    {
        public string Name { get; } = "exit";
        public string Description { get; } = "выход из приложения";

        public event Action EventEndWork;

        public void Run()
        {
            EventEndWork?.Invoke();
        }

        public void ReadUserInput(string text)
        {
        }
    }
}
