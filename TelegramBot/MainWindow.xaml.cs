using System.Windows;


namespace TelegramBot
{
    public partial class MainWindow : Window
    {
        TelegramBott client;
        public MainWindow()
        {
            InitializeComponent();
            client = new TelegramBott(this, @"H:\token.txt");

            logList.ItemsSource = client.BotMessageLog;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
