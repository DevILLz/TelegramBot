using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot
{
    public class ChatLog
    {
        /// <summary>
        /// Время получения сообщения
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// ID чата
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Сообщения пользователя
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Данные полученного сообщения для вывода в консоль
        /// </summary>
        /// <param name="Time">Время получения сообщения</param>
        /// <param name="Msg">Сообщения пользователя</param>
        /// <param name="FirstName">Имя пользователя</param>
        /// <param name="Id">ID чата</param>
        public ChatLog(string Time, string Msg, string FirstName, long Id)
        {
            this.Time = Time;
            this.Msg = Msg;
            this.FirstName = FirstName;
            this.Id = Id;
        }
        public ChatLog(string Msg, string FirstName, long Id)
        {
            this.Time = DateTime.Now.ToLongTimeString();
            this.Msg = Msg;
            this.FirstName = FirstName;
            this.Id = Id;
        }
    }
}

