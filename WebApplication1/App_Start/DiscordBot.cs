using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.WebSocket;

namespace WebApplication1.App_Start
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient bot;

        public DiscordBot()
        {
            bot = new DiscordSocketClient();

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

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("pong!");
        }
    }
}