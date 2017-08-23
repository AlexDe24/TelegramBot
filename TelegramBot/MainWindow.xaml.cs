using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using TelegramBot.Logic;
using TelegramBot.Logic.APITranslate;
using TelegramBot.Logic.APIWeather;
using TelegramBot.Logic.Repositories;

namespace TelegramBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<long> _isWaitAnswerForWeather { get; set; }
        private List<long> _isWaitAnswerForRememder { get; set; }

        private ICanGetWeatherByCoordinate _weatherByCoordinate;
        private ICanGetWeatherByName _weatherByName;
        private ICanTranslate _translator;

        private Bot apiWork;

        private CancellationTokenSource _cancelTokenSource;

        public MainWindow()
        {
            InitializeComponent();

            _isWaitAnswerForWeather = new List<long>();
            _isWaitAnswerForRememder = new List<long>();

            _translator = new Translator(Properties.Settings.Default.TokenTranslator, Properties.Settings.Default.RequestTranslator);


            CanGetByName.Items.Add("Open Weather Map");
            CanGetByName.Items.Add("Dark Sky");

            CanGetByCoordinate.Items.Add("Dark Sky");

        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Start.Visibility = Visibility.Hidden;
            Stop.Visibility = Visibility.Visible;

            _cancelTokenSource = new CancellationTokenSource();

            try
            {
                if (CanGetByName.SelectedItem.ToString() == "Open Weather Map")
                {
                    _weatherByName = new OpenWeatherMap(Properties.Settings.Default.TokenOpenWeather, Properties.Settings.Default.RequestOpenWeather);
                }
                else if (CanGetByName.SelectedItem.ToString() == "Dark Sky")
                {
                    _weatherByName = new DarkSky(_translator, Properties.Settings.Default.TokenDarkSky, Properties.Settings.Default.RequestDarkSky);
                }

                if (CanGetByCoordinate.SelectedItem.ToString() == "Dark Sky")
                {
                    _weatherByCoordinate = new DarkSky(_translator, Properties.Settings.Default.TokenDarkSky, Properties.Settings.Default.RequestDarkSky);
                }

                var token = _cancelTokenSource.Token;

                apiWork = new Bot(Properties.Settings.Default.TokenBot, token, _weatherByCoordinate, _weatherByName, _translator, () => new UsersRepository());

                apiWork.BotWork();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Stop.Visibility = Visibility.Hidden;
            Start.Visibility = Visibility.Visible;

            _cancelTokenSource.Cancel();
        }
    }
}
