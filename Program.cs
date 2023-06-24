using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace TelegramBotExperiments
{
    class Program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5996274903:AAE_YEk2zYAqdMd8guTXTuXQ82f_eTwxtKU");
        static HttpClient httpClient = new HttpClient();
        

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;

                if (message.Text.ToLower() == "/start")
                {
                    string welcomeMessage = "Привіт, доброго дня!\n\n" +
                         "Цей бот надає різні функції для отримання інформації про фільми та цитати.\n\n" +
                         "Доступні команди:\n" +
                         "/movies - відправить жанри фільмів на Netflix\n" +
                         "/countries - відправить список країн, доступних на Netflix\n" +
                         "/services - відправить список стрімінгових сервісів\n" +
                         "/quotes - відправить випадкові відомі цитати\n" +
                         "/actors - відправить інформацію про випадкових акторів\n\n" +
                         "Щоб отримати інформацію, просто введіть одну з команд.";

                    await botClient.SendTextMessageAsync(message.Chat, welcomeMessage);
                    return;

                }
              
                else if (message.Text.ToLower() == "/movies")
                {
                    await SendNetflixGenres(message.Chat);
                    return;
                }
                else if (message.Text.ToLower() == "/countries")
                {
                    await SandNetflixCountries(message.Chat);
                    return;
                }
                else if (message.Text.ToLower() == "/services")
                {
                    await SendNetflixServices(message.Chat);
                }
                else if (message.Text.ToLower() == "/quotes")
                {
                    await SendFamousQuotes(message.Chat);
                }
                else if (message.Text.ToLower() == "/actors")
                {
                    await SendNetflixActors(message.Chat);
                }
                else
                {

                    string errorMessage = "Невідома команда. Доступні команди:\n" +
                                          "/start - початок роботи з ботом\n" +
                                          "/movies - жанри фільмів на Netflix\n" +
                                          "/countries - список країн на Netflix\n" +
                                          "/services - список стрімінгових сервісів\n" +
                                          "/quotes - випадкові відомі цитати\n" +
                                          "/actors - інформація про випадкових акторів";

                    await botClient.SendTextMessageAsync(message.Chat, errorMessage);
                    return;
                }
                }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        static async Task SendNetflixServices(Chat chat)
        {
            string url = "https://streaming-availability.p.rapidapi.com/v2/services";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-RapidAPI-Key", "4f8fc65159msh728cfee1c0ed9e3p17c49fjsn13d0446ee982");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string services = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(services);
            var results = json["result"];

            int limit = 2;
            var firstTenServices = results.Take(limit);

            StringBuilder messageBuilder = new StringBuilder();
            foreach (var service in firstTenServices)
            {
                messageBuilder.AppendLine(service.ToString());
            }

            string messageText = messageBuilder.ToString();
            await SendLongMessageAsync(chat, messageText);
        }

        static async Task SendLongMessageAsync(Chat chat, string message)
        {
            int maxMessageLength = 4096;
            int numMessages = (int)Math.Ceiling((double)message.Length / maxMessageLength);

            for (int i = 0; i < numMessages; i++)
            {
                int startIndex = i * maxMessageLength;
                int length = Math.Min(maxMessageLength, message.Length - startIndex);
                string part = message.Substring(startIndex, length);

                await bot.SendTextMessageAsync(chat, part);

               
                await Task.Delay(1000);
            }
        }



        static async Task SendFamousQuotes(Chat chat)
        {
            string url = "https://andruxnet-random-famous-quotes.p.rapidapi.com/?cat=famous&count=10";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-RapidAPI-Key", "4f8fc65159msh728cfee1c0ed9e3p17c49fjsn13d0446ee982");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string quotesJson = await response.Content.ReadAsStringAsync();
            var quotes = JArray.Parse(quotesJson);

            StringBuilder messageBuilder = new StringBuilder();
            foreach (var quote in quotes)
            {
                string quoteText = quote["quote"].ToString();
                string author = quote["author"].ToString();
                string category = quote["category"].ToString();

                string quoteInfo = $"Category: {category}\n" +
                                   $"Quote: {quoteText}\n" +
                                   $"Author: {author}\n\n";

                messageBuilder.Append(quoteInfo);
            }

            await bot.SendTextMessageAsync(chat, messageBuilder.ToString());
        }




        static async Task SendNetflixActors(Chat chat)
        {
            string url = "https://moviesdatabase.p.rapidapi.com/actors/random?limit=10";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-RapidAPI-Key", "4f8fc65159msh728cfee1c0ed9e3p17c49fjsn13d0446ee982");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string actorsJson = await response.Content.ReadAsStringAsync();
            var actorsData = JObject.Parse(actorsJson);
            var results = actorsData["results"];

            StringBuilder messageBuilder = new StringBuilder();
            foreach (var actor in results)
            {
                string name = actor["primaryName"].ToString();
                string profession = actor["primaryProfession"].ToString();
               

                string actorInfo = $"Name: {name}\n" +
                                   $"Profession: {profession}\n";

                messageBuilder.Append(actorInfo);
            }

            string messageText = messageBuilder.ToString();
            await SendLongMessageAsync(chat, messageText);
        }



        static async Task SendNetflixGenres(Chat chat)
        {
            string url = "https://unogsng.p.rapidapi.com/genres";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-RapidAPI-Key", "4f8fc65159msh728cfee1c0ed9e3p17c49fjsn13d0446ee982");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string genres = await response.Content.ReadAsStringAsync();

            int maxMessageLength = 4096; 
            int numMessages = (int)Math.Ceiling((double)genres.Length / maxMessageLength);

            for (int i = 0; i < numMessages; i++)
            {
                int startIndex = i * maxMessageLength;
                int length = Math.Min(maxMessageLength, genres.Length - startIndex);
                string messageText = genres.Substring(startIndex, length);

                await bot.SendTextMessageAsync(chat, messageText);

                
                await Task.Delay(1000);
            }
        }

        static async Task SandNetflixCountries(Chat chat)
        {
            string url = "https://unogsng.p.rapidapi.com/countries";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-RapidAPI-Key", "4f8fc65159msh728cfee1c0ed9e3p17c49fjsn13d0446ee982");

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string countries = await response.Content.ReadAsStringAsync();
            var countriesJson = JObject.Parse(countries)["results"];

            int maxMessageLength = 4096;
            int numMessages = (int)Math.Ceiling((double)countriesJson.Count() / maxMessageLength);

            for (int i = 0; i < numMessages; i++)
            {
                int startIndex = i * maxMessageLength;
                int length = Math.Min(maxMessageLength, countriesJson.Count() - startIndex);

                var selectedCountries = countriesJson
                    .Skip(startIndex)
                    .Take(length)
                    .Select(c => new
                    {
                        country = c["country"].ToString(),
                        countrycode = c["countrycode"].ToString(),
                        id = c["id"].ToString()
                    })
                    .ToList();

                string messageText = JsonConvert.SerializeObject(selectedCountries);

                await bot.SendTextMessageAsync(chat, messageText);

                await Task.Delay(1000);
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("bot" + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };

            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );

            Console.ReadLine();
        }
    }
}
