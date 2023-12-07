using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DesktopPriceUploader
{
    class DirMonitoring
    {
        /// <summary>
        ///Мониторинг появления новых файлов.
        /// </summary>
        FileSystemWatcher _watcherFiles;

        /// <summary>
        /// Делагат для вывода информации.
        /// </summary>
        readonly Action<string, bool> _printInfo;

        /// <summary>
        /// Делегат сворачивания разворачивания формы.
        /// </summary>
        readonly Action<bool> _hideForm;

        public static string pathToTemp = "C:\\Temp\\";
        public static string priceFilePath = "Price";

		public DirMonitoring(Action<string, bool> printInfo, Action<bool> hideForm)
        {
            this._printInfo = printInfo;
            this._hideForm = hideForm;

            if (!CheckNeededFolders()) { return; }
            
            SetTodayInDirWatcher();
            SetTimerForDeleteTempFiles();
            DeleteTempFiles();
        }

        private bool CheckNeededFolders()
        {
            if (!Directory.Exists(pathToTemp))
            {
	            try
                {
                    Directory.CreateDirectory(pathToTemp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось создать папку: " + pathToTemp + " с ошибкой:\n" + ex.Message + "\nСоздайте папку вручную.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            if (!Directory.Exists(priceFilePath))
            {
                try
                {
                    Directory.CreateDirectory(priceFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось создать папку: " + priceFilePath + " с ошибкой:\n" + ex.Message + "\nСоздайте папку вручную.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

            }
            return true;
        }

        public static void DeleteTempFiles()
        {
            try
            {

                string[] FilesInDir = Directory.GetFiles(pathToTemp, "*");
                //Если в папке нет файлов
                if (FilesInDir.Length > 0)
                {
                    foreach (var fi in FilesInDir)
                    {
                        File.Delete(fi);
                    }
                }

                FilesInDir = Directory.GetFiles(priceFilePath, "*");
                if (FilesInDir.Length > 0)
                {
                    foreach (var fi in FilesInDir)
                    {
                        File.Delete(fi);
                    }
                }
            }
            catch (Exception ex)
            {
                return;
            }

        }

        /// <summary>
        /// Назначение вотчера мониторинга новых файлов
        /// </summary>
        private void SetTodayInDirWatcher()
        {
			_watcherFiles = new FileSystemWatcher(priceFilePath);
            _watcherFiles.NotifyFilter = NotifyFilters.LastAccess |
                                        NotifyFilters.LastWrite |
                                        NotifyFilters.FileName |
                                        NotifyFilters.DirectoryName;
            _watcherFiles.Created += new FileSystemEventHandler(OnChangePriceInfo);
            _watcherFiles.EnableRaisingEvents = true;
            //watcherTodayInDir.Renamed += new RenamedEventHandler(OnChangeTodayInDir);
            //watcherTodayInDir.Changed += new FileSystemEventHandler(OnChangeTodayInDir);

        }

		private void SetTimerForDeleteTempFiles()
        {
            var timer = new System.Windows.Forms.Timer();

            timer.Interval = 15000;
            //timer.Tick += new EventHandler(()=>{});

            timer.Start();

        }

        private void DeleteFiles()
        {
            string[] FilesInDir = Directory.GetFiles(priceFilePath, "*");
            foreach (string file in FilesInDir)
            {
	            File.Delete(file);
            }
        }
        
        public void OnChangePriceInfo(object sender, FileSystemEventArgs e)
        {

            if (e.Name.Contains(".xlsx"))
            {
				Thread.Sleep(500);
				_hideForm(false); //Развернуть форму.
				_printInfo($"Файл {e.Name}", false);
				DeleteFiles();
				Thread.Sleep(50);
				Thread.Sleep(2000);
				_hideForm(true); //Развернуть форму.
			}
		}
    }
}
