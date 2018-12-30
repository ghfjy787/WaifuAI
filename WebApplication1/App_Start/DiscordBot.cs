using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Api.Ai;
using System.Linq;
using System.Collections.Generic;
using Api.Ai.ApplicationService.Factories;
using SimpleInjector;
using Api.Ai.Domain.Service.Factories;
using Api.Ai.Infrastructure.Factories;
using Api.Ai.Domain.DataTransferObject.Request;
using Api.Ai.ApplicationService.Interfaces;
using Api.Ai.Domain.DataTransferObject.Response;
using WebApplication1.Areas.HelpPage.DialogflowLogic.App_Start;

namespace WebApplication1.App_Start
{
    public class DiscordBot
    {
        private readonly DiscordSocketClient bot;
        private readonly IApiAiAppServiceFactory dialogflowInitializer;
        private readonly IQueryAppService dialogflowService;
        private readonly CodeRunnerAction actionResolver;
        private readonly Random random = new Random();

        public DiscordBot(IApiAiAppServiceFactory dialogflowInitializer)
        {
            bot = new DiscordSocketClient();
            this.dialogflowInitializer = dialogflowInitializer ?? throw new Exception("Dialogflow instance not found!");
            dialogflowService = dialogflowInitializer.CreateQueryAppService(
                "https://api.api.ai/v1", "57b94ef7ea6d46efa59db9e6a2db427f");

            actionResolver = CodeRunnerAction.Instance ?? new CodeRunnerAction();

            bot.Ready += ReadyAsync;
            bot.MessageReceived += MessageReceivedAsync;
        }

        public static void RegisterChatbot()
        {
            var container = new Container();
            container.RegisterInstance<IServiceProvider>(container);
            container.Register<IApiAiAppServiceFactory, ApiAiAppServiceFactory>();
            container.Register<IHttpClientFactory, HttpClientFactory>();
            
            new DiscordBot(container.GetInstance<IApiAiAppServiceFactory>()).Start();
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

            QueryRequest queryRequest = actionResolver.ResolveInputAction(new Dictionary<string, object>()
            {
                { "sessionId", message.Author.Id.ToString() },
                { "message", message.Content },
                { "previousResponse", null }
            });

            if(queryRequest != null)
            {
                QueryResponse queryResponse = await dialogflowService.PostQueryAsync(queryRequest);

                if(queryResponse.Result != null)
                {
                    string messageResponse = actionResolver.ResolveOutputAction(queryResponse.Result.Action, queryResponse);
                    await message.Channel.SendMessageAsync(messageResponse);
                }
            }
        }
    }
}