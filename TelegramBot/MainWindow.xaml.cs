using System.Collections.ObjectModel;
using System.Linq;
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
        //удаление поэлементно
        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            if (logList.SelectedIndex > -1 && logList.SelectedIndex <= logList.Items.Count - 1)
            {
                if (logList.SelectedItems != null && logList.SelectedItems.Count > 0)
                {
                    var toRemove = logList.SelectedItems.Cast<ChatLog>().ToList();
                    var items = logList.ItemsSource as ObservableCollection<ChatLog>;
                    if (items != null)
                    {
                        foreach (var item in toRemove)
                        {
                            items.Remove(item);
                        }
                    }
                }
            }
        }
    }
}
