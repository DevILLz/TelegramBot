using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;


namespace TelegramBot
{
    public partial class MainWindow : Window
    {
        private TelegramBott client;
        public MainWindow()
        {
            InitializeComponent();
            ClientStart();
            logList.ItemsSource = client.BotMessageLog;
        }

        /// <summary>
        /// Запуск бота и поиска файла токена
        /// </summary>
        void ClientStart()
        {
            string FileName = @"H:\\token.txt";
            while (true)
            {
                try //обработка исключений
                {
                    this.client = new TelegramBott(this, FileName);
                    break;
                }
                catch
                {
                    MessageBox.Show("файл не найден или токен не дейсвителен\n" +
                        "Выберите файл с действительным токеном");
                    
                    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                    dialog.Filter = "All files (*.*)|*.*";
                    dialog.FilterIndex = 0;
                    //dialog.DefaultExt = "json";
                    Nullable<bool> result = dialog.ShowDialog();
                    if (result == true)
                    {
                        FileName = dialog.FileName;
                    }
                }
            }
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
