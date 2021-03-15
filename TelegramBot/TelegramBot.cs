using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Telegram.Bot;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace TelegramBot
{
    class TelegramBott
    {
        private MainWindow w;
        /// <summary>
        /// Разрешение на скачивание файлов
        /// </summary>
        private bool downloadFlag = false;
        /// <summary>
        /// Колличество скаеенных ботом файлов в текущей сессии
        /// </summary>
        private int itemsCount = 0;
        private WebClient weather;
        static private TelegramBotClient bot; // бот
        public ObservableCollection<ChatLog> BotMessageLog { get; set; } // лог чата текущей сессии 
        public TelegramBott(MainWindow W, string PathToken) // инициализация бота
        {
            this.BotMessageLog = new ObservableCollection<ChatLog>();
            this.w = W;
            weather = new WebClient() {Encoding = Encoding.UTF8 };
            bot = new TelegramBotClient(File.ReadAllText(PathToken));

            bot.OnMessage += MessageListener;

            bot.StartReceiving();
        }

        /// <summary>
        /// Взаимодействие с сообщением пользователя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            itemsCount++;
            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new ChatLog(
                    DateTime.Now.ToLongTimeString(), e.Message.Text, e.Message.Chat.FirstName, e.Message.Chat.Id));
            });

            string msgType = e.Message.Type.ToString();
            w.Dispatcher.Invoke(() =>
            {
                if (!downloadFlag && msgType != "Text")
                {
                    bot.SendTextMessageAsync(e.Message.Chat.Id,
                            $"Что бы загружать файлы на сервер бота необходимо дать своё согласие на это (/DownloadThings)");
                    return;
                }
                switch (msgType)
                {
                    case "Text":
                        SendMessage(e.Message.Text.ToLower(), e.Message.Chat.Id);
                        return;

                    case "Audio":
                        if (!Directory.Exists($"Downloaded/{msgType}")) Directory.CreateDirectory($"Downloaded/{msgType}");
                        string aType = e.Message.Audio.MimeType.Replace(@"audio/", "");
                        DownLoad(e.Message.Audio.FileId,
                            $@"Downloaded/{msgType}/{msgType}_{itemsCount}.{aType}",
                            e.Message.Chat.Id);
                        return;

                    case "Document":
                        if (!Directory.Exists($"Downloaded/{msgType}")) Directory.CreateDirectory($"Downloaded/{msgType}");
                        DownLoad(e.Message.Document.FileId,
                            $@"Downloaded/{msgType}/{e.Message.Document.FileName}",
                            e.Message.Chat.Id);
                        return;

                    case "Photo":
                        if (!Directory.Exists($"Downloaded/{msgType}")) Directory.CreateDirectory($"Downloaded/{msgType}");
                        DownLoad(e.Message.Photo[e.Message.Photo.Length - 1].FileId, $@"Downloaded/{msgType}/Img_{itemsCount}.jpg", e.Message.Chat.Id);
                        return;

                    case "Video":
                        if (!Directory.Exists($"Downloaded/{msgType}")) Directory.CreateDirectory($"Downloaded/{msgType}");
                        string vType = e.Message.Video.MimeType.Replace(@"video/", "");
                        DownLoad(e.Message.Video.FileId,
                            $@"Downloaded/{msgType}/Video_{itemsCount}.{vType}",
                            e.Message.Chat.Id);
                        return;

                    default:
                        return;
                }
            });
        }

        
        /// <summary>
        /// Обработка сообщений пользователя
        /// </summary>
        /// <param name="messageText">Текст сообщения пользователя</param>
        /// <param name="chatId">ID пользователя</param>
        public void SendMessage(string messageText, long chatId)
        {
            
            string text = "";
            switch (messageText)
            {
                case "hello":
                    text = "Hello mate";
                    break;

                case "/downloadthings":
                    if (!downloadFlag)
                    {
                        downloadFlag = true;
                        text = "Теперь я могу скачивать присылаемые вами файлы";
                    }
                    else
                    {
                        downloadFlag = false;
                        text = "Теперь я НЕ могу скачивать присылаемые вами файлы";
                    }
                    break;

                case "/weather": // погода, попытка взаимодействия с API стороннего сайта
                    string city = "yaroslavl";
                    text = $" {WeatherParser(city)}\n\n " +
                        $"Для вывода погоды других городов введите их название и страну латиницей.\n" +
                        $"Например \"Moscow,ru\"" ;
                    break;

                case "/help":
                    text = "Вам доступны команды: \n" +
                        "/DownloadThings - разшерить или запретить боту скачивание ваших файлов\n" +
                        "/weather - погода\n" +
                        "3 - test\n";
                    break;

                default:
                        text = $"{WeatherParser(messageText)}";                 
                    break;
            }
            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new ChatLog(
                    text, 
                    "Bot himself",
                    chatId));
                bot.SendTextMessageAsync(chatId, text);
            });
            
        }
        string weatherToken = "2628d760746e3fbfc599caaab455c698";// Решил не убирать в отдельный файл, т.к. заблокирую его после сдачи проекта

        /// <summary>
        /// Парсер openweathermap
        /// </summary>
        /// <param name="city">Требуемый город (латиницей)</param>
        /// <returns></returns>
        private string WeatherParser(string city)
        {
            string json;
            try //обработка исключений
            {
                json = weather.DownloadString($@"http://api.openweathermap.org/data/2.5/find?q={city}&type=like&units=metric&lang=ru&APPID={weatherToken}");
            }
            catch 
            {
                return "Не удалось найти город, попробуйте еще раз";
            }
            if (Int32.Parse(JObject.Parse(json)["count"].ToString()) == 0) return "Не удалось найти город, попробуйте еще раз"; //обработка исключений

            var cities = JObject.Parse(json)["list"].ToArray();
            string text = $"Текущая температура в {city} {JObject.Parse(cities[0].ToString())["main"]["temp"]} с° \n" +
                $"Ощущается как: {JObject.Parse(cities[0].ToString())["main"]["feels_like"]} с° \n" +
                $"Ветер: {JObject.Parse(cities[0].ToString())["wind"]["speed"]} м/c \n" +
                $"{JObject.Parse(JObject.Parse(cities[0].ToString())["weather"].ToArray()[0].ToString())["description"]}";
            //в json включен массив с описанием погоды. Строка выше забирает из него первое значение.

            return text;
        }

        /// <summary>
        /// Загрузка файла, присылаемого боту
        /// </summary>
        /// <param name="fileId">ID файла</param>
        /// <param name="path">Путь сохранения файла</param>
        /// <param name="chatId">ID пользователя</param>
        static async void DownLoad(string fileId, string path, long chatId)
        {
            var file = await bot.GetFileAsync(fileId);
            if (file.FileSize > 31_457_280)
            {
                await bot.SendTextMessageAsync(chatId, $"File size must be less then 30 mb"); 
            }
            else
            {
                await bot.SendTextMessageAsync(chatId, $"Загружаю"); 
                FileStream fs = new FileStream(path, FileMode.Create);
                await bot.DownloadFileAsync(file.FilePath, fs);
                fs.Close();
                fs.Dispose();
            }
        }
    }
}
