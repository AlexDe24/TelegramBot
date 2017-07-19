using System.ComponentModel;
using System.Windows;
using TelegramBot.Logic;

namespace TelegramBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileClass _fileWork;
        BackgroundWorker _backWorker;

        public MainWindow()
        {
            InitializeComponent();

            _fileWork = new FileClass();

            _backWorker = new BackgroundWorker();
            _backWorker.DoWork += backWorker_DoWork;
        }

        void backWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var key = e.Argument as string; 

            try
            {
                _fileWork.BotWork();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_backWorker.IsBusy != true)
            {
                _backWorker.RunWorkerAsync();
            }
        }
    }
}
