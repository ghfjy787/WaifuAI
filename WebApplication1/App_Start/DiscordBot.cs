using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using ApiAi.Models;
using ApiAi;
using System.Linq;

namespace WebApplication1.App_Start
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient bot;
        private readonly ConfigModel dialogflowConfig;
        private readonly Random random = new Random();

        public DiscordBot()
        {
            bot = new DiscordSocketClient();
            dialogflowConfig = new ConfigModel() {
                AccesTokenClient = "57b94ef7ea6d46efa59db9e6a2db427f",
                Language = ApiAi.Enums.LanguagesEnum.Italian
            };
            bot.Ready += ReadyAsync;
            bot.MessageReceived += MessageReceivedAsync;
        }

        public static void RegisterChatbot()
        {
            new DiscordBot().Start();
        }

        private async void Start()
        {
            await bot.LoginAsync(TokenType.Bot, "NTI2ODg2NjYyOTE2Mjc2MjI2.DwL37Q.w8NYPGUP0S9byw8wk9QVCjuJLl4");
            await bot.StartAsync();

            // Block the program until it is closed.
            await Task.Delay(-1);
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{bot.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == bot.CurrentUser.Id)
                return;

            var res = QueryService.SendRequest(dialogflowConfig, message.Content);
            
            if(res == null) {
                await message.Channel.SendMessageAsync("Input not recognized, oni-chan");
                return;
            }

            var messages = res.Messages.ToArray();
            if(messages.Length == 0) {
                await message.Channel.SendMessageAsync("Capisco ma non saprei come risponderti, oni-chan");
                return;
            }

            await message.Channel.SendMessageAsync(messages[random.Next(0, messages.Length - 1)].Text);
        }
    }
}