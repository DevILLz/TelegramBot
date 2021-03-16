using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;


namespace TelegramBot
{
    public partial class MainWindow : Window
    {
        private TelegramBott client;
        
        private string defaultLogFileName = "Log.json";
        private string token = @"H:\token.txt";
        public MainWindow()
        {
            InitializeComponent();
            ClientStart(this.token, true);
            if (File.Exists(defaultLogFileName))
            {
                Import(defaultLogFileName);
            }
            else logList.ItemsSource = client.BotMessageLog;
        }

        /// <summary>
        /// Запуск бота
        /// </summary>
        /// <param name="token">Имя файла, в котором хранится токен телегам бота</param>
        /// <param name="check">Выводить или не выводить сообщение при исключении</param>
        void ClientStart(string token, bool check)
        {
            while (true)
            {
                try //обработка исключений
                {
                    this.client = new TelegramBott(this, token);
                    break;
                }
                catch
                {
                    if (check)
                    MessageBox.Show("файл не найден или токен не дейсвителен\n" +
                        "Выберите файл с действительным токеном");
                    
                    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                    dialog.Filter = "All files (*.*)|*.*";
                    dialog.FilterIndex = 0;
                    //dialog.DefaultExt = "json";
                    Nullable<bool> result = dialog.ShowDialog();
                    if (result == true)
                    {
                        token = dialog.FileName;
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
        private void Export_button(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            dialog.FilterIndex = 0;
            dialog.DefaultExt = "json";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                Export(dialog.FileName);
            }
        }
        private void Import_button(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            dialog.FilterIndex = 0;
            dialog.DefaultExt = "json";
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                Import(dialog.FileName);
            }
        }
        /// <summary>
        /// импорт данных
        /// </summary>
        /// <param name="filename">Имя файла</param>
        private void Import(string fileName)
        {
            string json = File.ReadAllText(fileName);
            client.BotMessageLog = JsonConvert.DeserializeObject<ObservableCollection<ChatLog>>(json);
            logList.ItemsSource = client.BotMessageLog;
        }

        /// <summary>
        /// Экспорт данных
        /// </summary>
        /// <param name="filename">Имя файла</param>
        private void Export(string fileName)
        {
            string json = JsonConvert.SerializeObject(client.BotMessageLog);
            File.WriteAllText(fileName, json);
        }

        private void Weather_new(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = dialog.ShowDialog();
            if (result == true)
            {
                client.weatherToken = dialog.FileName;
            }
        }
        private void New_bot(object sender, RoutedEventArgs e)
        {
            ClientStart("", false);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Export(defaultLogFileName);
        }
    }
}
