using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using System.Collections;
using System.Data;
using Microsoft.Data.Sqlite;
using NLog;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.SqlClient;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;


namespace TelegramBotExperiments
{
    struct BotUpdate
    {
        public string text;
        public long id;
        public string? username;
    }

    class Program
    {

        static string kod = System.IO.File.ReadAllText(@"tokenTG.txt");
        static ITelegramBotClient bot = new TelegramBotClient(kod.ToString());

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static ReplyKeyboardMarkup lastkeyboard = new ReplyKeyboardMarkup(new KeyboardButton(""));
        private static ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new KeyboardButton(""));
        //создание листа films строк БД FILMS, параметризированного классом Film
        private static List<Film> films = new List<Film>();

        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            logger.Trace("trace message");
            logger.Debug("debug message");
            logger.Info("info message");
            logger.Warn("warn message");
            logger.Error("error message");
            logger.Fatal("fatal message");

            logger.Debug("log {1}", "EventHandler"); //лог


            //подключение методов
            CreateTable();
            getFilms();

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            //подключение событий
            {
                AllowedUpdates = { }, //получать все типы обновлений
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Shifr();
            Console.ReadLine();

        }

        //получение данных из листа film
        private static void getFilms()
        {
            var connection = new SqliteConnection("Data Source=Films.db");
            connection.Open();
            SqliteCommand command = new SqliteCommand("SELECT * FROM Film", connection);
            SqliteDataReader reader = command.ExecuteReader();
            if (reader.HasRows) // если есть данные
            {
                while (reader.Read())   // построчно считываем данные
                {
                    // Console.WriteLine($"{ID} \t {Name} \t {Genre} \t {Year} \t{AgeLimit} \t{Lasting} \t{Description} \t{URL} ");
                    Film film = new Film(int.Parse(reader.GetValue(0).ToString()), reader.GetValue(1).ToString(), reader.GetValue(2).ToString(), int.Parse(reader.GetValue(3).ToString()), reader.GetValue(4).ToString(), reader.GetValue(5).ToString(), reader.GetValue(6).ToString(), reader.GetValue(7).ToString());
                    films.Add(film);
                }
            }
            connection.Close();
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            logger.Debug("log {0}", "Start/Info/Help.Debug"); //лог
            Console.WriteLine(JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                InsertData(message);
                //Вывод в файл инфы о пользователях
                // System.IO.File.AppendAllText("user.txt", $"Message:{message.Text}, message_id:{message.MessageId}, FROMid:{message.From.Id}, FROMisBot:{message.From.IsBot}, date:{message.Date}\n");

                logger.Debug("log {0}", "Кнопка Start"); //лог

                if (message.Text != null)
                {

                    if (message.Text.ToLower() == "/start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать, юный киноман!");
                        return;
                    }
                    logger.Debug("log {0}", "Кнопка Info"); //лог
                    if (message.Text.ToLower() == "/info")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Моя задача помочь пользователю в подборке фильма.");
                        return;
                    }
                    logger.Debug("log {0}", "Кнопка Help"); //лог
                    if (message.Text.ToLower() == "/help")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Для работы с ботом необходимо воспользоваться кнопками: выбрать категорию жанр, затем сам фильм из предложенных. Также использовать /start, /help, /info, /menu.");
                        return;
                    }
                    logger.Debug("log {0}", "Кнопка Menu"); //лог
                    //создание кнопок
                    if (message.Text.ToLower() == "/menu") //запуск кнопок
                    {
                        replyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                            new []
                            {
                               new KeyboardButton("Триллер")
                            },
                            new []
                            {
                                new KeyboardButton("Мелодрама")
                            },
                            new []
                            {
                                new KeyboardButton("Ужасы")
                            },
                            new []
                            {
                                new KeyboardButton("Комедии")
                            }
                        }
                        );
                        await bot.SendTextMessageAsync(message.From.Id, "Для работы с ботом необходимо воспользоваться кнопками: выбрать категорию жанр, затем сам фильм из предложенных.", replyMarkup: replyKeyboard);
                        return;
                    }
                    //if (message.Text.ToLower() == "назад")
                    //{
                    //    replyKeyboard = lastkeyboard;
                    //    await bot.SendTextMessageAsync(message.From.Id, "Ориентируйтесь на ваш вкус", replyMarkup: replyKeyboard);
                    //    return;
                    //}
                    //ссылка на фильм
                    //inline кнопки
                    if (message.Text == "Триллер")
                    {
                        InlineKeyboardMarkup keyboard = new(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Сонная Лощина", "1"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Отступники", "2"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Молчание ягнят", "3"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Семь", "4"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Бойцовский клуб", "5"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Остров проклятых", "6"),
                        },
                    });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите фильм:", replyMarkup: keyboard);
                        return;
                    }
                    if (message.Text == "Мелодрама")
                    {
                        InlineKeyboardMarkup keyboard = new(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Стажёр", "7"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Ирония судьбы, или С легким паром!", "8"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Вечное сияние чистого разума", "9"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Титаник", "11"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Москва слезам не верит", "10"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Любовь и голуби", "12"),
                        },

                    });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите фильм:", replyMarkup: keyboard);
                        return;
                    }
                    if (message.Text == "Ужасы")
                    {
                        InlineKeyboardMarkup keyboard = new(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("1408", "13"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Пила: Игра на выживание", "14"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Сайлент Хилл", "15"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Мгла", "16"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Заклятие", "17"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Оно", "18"),
                        },
                    });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите фильм:", replyMarkup: keyboard);
                        return;
                    }
                    if (message.Text == "Комедии")
                    {
                        InlineKeyboardMarkup keyboard = new(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Приключения Паддингтона", "19"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Невероятная жизнь Уолтера Митти", "20"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Бриллиантовая рука", "21"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Маска", "22"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Один дома", "23"),
                        },
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Иван Васильевич меняет профессию", "24"),
                        },

                    });
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Выберите фильм:", replyMarkup: keyboard);
                        return;
                    }
                    await botClient.SendTextMessageAsync(message.Chat, "Извините, я не могу Вас понять.");
                }
            }
            //выведение данных о фильме в сообщение ТГбота
            else if (update.Type == UpdateType.CallbackQuery)
            {
                var message = update.CallbackQuery;
                foreach (var film in films) //перебор строк листа фильмов
                {
                    if (message.Data == film.ID.ToString())
                    {
                        var hyperLinkKeyboard = new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Нажми для перехода на ссылку", film.URL));
                        await bot.SendTextMessageAsync(message.Message.Chat.Id, $"{film.Name} - {film.Year}\n" +
                            $"Жанр: {film.Genre}\n" +
                            $"Возрастное ограничение: {film.AgeLimit}\n" +
                            $"Продолжительность: {film.Genre}\n" +
                            $"Описание фильма: {film.Description}", replyMarkup: hyperLinkKeyboard);
                    }
                }
            }

        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        //Работа с БД для аналитики пользователей
        //Соединение с БД
        static SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn;
            //Create a new database connection:
            sqlite_conn = new SQLiteConnection("Data Source= users.db; Version = 3; New = True; Compress = True; ");
            // Open the connection:
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {

            }
            return sqlite_conn;
        }

        //Создание таблицы для взятия данных о пользователях, использовавших бота
        static void CreateTable()
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            string Createsql = "CREATE TABLE IF NOT EXISTS Users (Text text, ID INT, FromID INT, Bot boolean, Date string(40), Username string(30), Firstname string(25), Lastname string(25))";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = Createsql;
            sqlite_cmd.ExecuteNonQuery();
        }

        //Вставка в таблицу данных о пользователях, использовавших бота
        static void InsertData(Message message)
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = $"INSERT INTO Users (Text, ID, FromID, Bot, Date, Username, Firstname, Lastname) " +
                $"VALUES( '{message.Text}', {message.MessageId}, {message.From.Id}, {message.From.IsBot}, '{message.Date}', '{message.From.Username}', '{message.From.FirstName}', '{message.From.LastName}' ); ";
            sqlite_cmd.ExecuteNonQuery();
        }

        //шифрование бд
        private static void Shifr()
        {
            using (var connection = new SqliteConnection("Data Source=users.db"))
            {
                connection.Open();
            }
            Console.Read();
            string sqlExpression = "CREATE MASTER KEY   ENCRYPTION BY PASSWORD = '12345!'";
        }

    }

}