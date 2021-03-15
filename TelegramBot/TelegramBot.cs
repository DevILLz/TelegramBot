using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

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

        static private TelegramBotClient bot; // бот
        public ObservableCollection<ChatLog> BotMessageLog { get; set; } // лог чата текущей сессии 
        public TelegramBott(MainWindow W, string PathToken) // инициализация бота
        {
            this.BotMessageLog = new ObservableCollection<ChatLog>();
            this.w = W;

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

        string weatherToken = "";
        /// <summary>
        /// Обработка сообщений пользователя
        /// </summary>
        /// <param name="messageText">Текст сообщения пользователя</param>
        /// <param name="chatId">ID пользователя</param>
        public void SendMessage(string messageText, long chatId)
        {
            string Text = "";
            switch (messageText)
            {
                case "hello":
                    Text = "Hello mate";
                    break;

                case "/downloadthings":
                    if (!downloadFlag)
                    {
                        downloadFlag = true;
                        Text = "Теперь я могу скачивать присылаемые вами файлы";
                    }
                    else
                    {
                        downloadFlag = false;
                        Text = "Теперь я НЕ могу скачивать присылаемые вами файлы";
                    }
                    break;

                case "/weather": // погода, попытка взаимодействия с API стороннего сайта
                    string city = "petersburg";
                    Text = $@"http://api.openweathermap.org/data/2.5/find?q={city}&type=like&APPID=2628d760746e3fbfc599caaab455c698";
                    break;

                case "/help":
                    Text = "Вам доступны команды: \n" +
                        "/DownloadThings - разшерить или запретить боту скачивание ваших файлов\n" +
                        "/weather - погода\n" +
                        "3 - бла бла бла\n" +
                        "4 - бла бла бла\n" +
                        "5 - бла бла бла\n" +
                        "6 - бла бла бла\n";
                    break;

                default:
                    break;
            }

            bot.SendTextMessageAsync(chatId, Text);
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
