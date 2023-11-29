using System;
using System.Windows.Forms;
using System.Drawing;
using DesktopPriceUploader.Forms;
using DesktopPriceUploader.WDM;

namespace DesktopPriceUploader
{
	public partial class MainForm : Form
    {
        /// <summary>
        /// Виртуальный рабочий стол. Компонент для правильного переключения между столами.
        /// </summary>
        private VirtualDesktopManager vdm;

		/// <summary>
		/// Делегат для вывода сообщений в основное окно.
		/// </summary>
		Action<string, bool> _printInfo;

		/// <summary>
		///Делегат для скрытия формы.
		/// </summary>
		/// <returns></returns>
		readonly Action<bool> _hideForm;
        
        public MainForm()
        {
            InitializeComponent();
            
            // Корректное отображение на виртуальных столах.
            vdm = new VirtualDesktopManager();

            // Запускаем таймер
            WDMTimer.Enabled = true;

            // Создаем значок в панели задач
            CreatedNotifyIcon();

            _printInfo = PrintInfoMessage; // Назначение функции для делегата.
            _hideForm = HideForm; // Назначение функции для делегата открытия/скрытия формы.

            DirMonitoring dm = new DirMonitoring(_printInfo, _hideForm);

            //Получаю интервал проверки наличия новых сообщений
            timerReadMessages.Interval = 21;
            timerReadMessages.Start();
        }

        /// <summary>
        /// Создаем значок в панели задач
        /// </summary>
        private void CreatedNotifyIcon()
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.robot;
            notifyIcon.Visible = true;

            // Событие двойного щелчка мышкой по значку в трее
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            // Привязка контекстного меню
            notifyIcon.ContextMenuStrip = this.contextMenuStripTray;
        }

        /// <summary>
        /// Двойной щелчок мышкой по значку в трее
        /// </summary>  
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenFormFromTray();
        }

        /// <summary>
        /// Открывает форму если она скрыта, иначе скрывает
        /// </summary>
        private void OpenFormFromTray()
        {
            if (this.Visible == true)
            {
                Hide();
            }
            else
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// Функция скрытия формы.
        /// </summary>
        /// <param name="hide"></param>
        void HideForm(bool hide)
        {
            if (hide)
            {
                //Так как из другого потока вызывается.
                this.Invoke(new Action(() => Hide()));
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                }));
            }
        }

        /// <summary>
        /// Функция для делегата вывода сообщений в основное окно.
        /// </summary>
        void PrintInfoMessage(string text, bool isError)
        {
            //Необходимо вывести сообщение об ошибке.
            if (isError)
            {
                Invoke(new Action(() => ShowError(text)));
                return;
            }

            //Пустая строка.
            if (String.IsNullOrEmpty(text))
            {
                richTextBox.Invoke(new Action(() => richTextBox.Clear())); //Очистить данные.
                return;
            }

            richTextBox.Invoke(new Action(() => richTextBox.AppendText(text)));
        }

        void ShowError(string Mess)
        {
            MessageBox.Show(this, Mess,
                                   "Ошибка", MessageBoxButtons.OK,
                                   MessageBoxIcon.Question);
        }

        private void WDMTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!vdm.IsWindowOnCurrentVirtualDesktop(Handle))
                {
                    using (NewWindow nw = new NewWindow())
                    {
                        nw.Show(null);
                        vdm.MoveWindowToDesktop(Handle, vdm.GetWindowDesktopId(nw.Handle));
                    }
                }
            }
            catch
            {
                return;
            }
        }

        // Метод чтения сообщений с БД
        private void ReadNewMessgesFromDatabase()
        {
            //Нет новых сообщений.
            //if (msg.Count == 0) return;

            ////Вывожу все новые сообщения.

            //HideForm(false);

            //foreach (string str in msg)
            //{
            //    setInfoText(str, 5);
            //}
        }

        /// <summary>
        /// Выводит сообщение о процессе работы. type=0 обычное сообщение,1-текст синим цветом ,type=3 ошибка,4-процесс,5 warning;
        /// type=4 обновлять только последню строку
        /// </summary>
        /// <param name="Mess"></param>
        /// <param name="type"></param>
        private void setInfoText(string Mess, int type)
        {
            richTextBox.SelectionStart = richTextBox.TextLength;
            richTextBox.SelectionLength = 0;

            System.Drawing.Color color = System.Drawing.Color.Black;

            switch (type)
            {
                case 0: color = System.Drawing.Color.Black; break;
                case 1: color = System.Drawing.Color.Blue; break;
                case 2: color = System.Drawing.Color.BlueViolet; break;
                //Cообщение об ошибке
                case 3:
                    color = System.Drawing.Color.Red;
                    break;
                //Прогресс бар-обновление строки 
                case 4: color = System.Drawing.Color.Blue; break;
                //Warning
                case 5:
                    color = System.Drawing.Color.DarkCyan;
                    break;
                //Warning Розовый
                case 6:
                    color = Color.FromArgb(255, 20, 147);
                    break;
                case 7: color = System.Drawing.Color.Red; break;
            }

            richTextBox.SelectionColor = color;
            if (type != 4)
            {
                //richTextBoxInfo.SelectionFont = font;
                richTextBox.AppendText(Mess + "\r\n");
                richTextBox.SelectionColor = richTextBox.ForeColor;
                richTextBox.ScrollToCaret();
                richTextBox.Update();
            }
            else
            {
                //Обновлять только последнюю строку
                int endLine = richTextBox.Lines.Length - 2;
                int s1 = richTextBox.GetFirstCharIndexFromLine(endLine);
                int s2 = richTextBox.GetFirstCharIndexFromLine(endLine + 1) - 1;

                richTextBox.Select(s1, s2 - s1);
                richTextBox.SelectionColor = color;
                richTextBox.SelectedText = Mess;

                richTextBox.SelectionColor = richTextBox.ForeColor;
                richTextBox.ScrollToCaret();
                richTextBox.Update();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Событие по закрытию формы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
        
        private void tsmiOpen_Click(object sender, EventArgs e)
        {
            OpenFormFromTray();
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InfoForm infoForm = new InfoForm();
            infoForm.ShowDialog();
        }

        private void timerReadMessages_Tick(object sender, EventArgs e)
        {
            ReadNewMessgesFromDatabase();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.Show();
        }
    }
}