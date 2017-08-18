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
        public MainWindow()
        {
            InitializeComponent();

            var apiWork = new APIClass(Properties.Settings.Default.Token);

            try
            {
                apiWork.BotWork();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
