using JamesMann.BotFramework.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoomBookingBot.Chatbot.Bots;
using System.Collections.Generic;

namespace RoomBookingBot.Chatbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);

            // do not use in memory storage for production!
            // for more information see https://github.com/jamesemann/JamesMann.BotFramework
            var authTokenStorage = new InMemoryAuthTokenStorage(); 
            
            services.AddSingleton<IAuthTokenStorage>(authTokenStorage);
            
            services.AddBot<Bot>((options) => {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                
                // 1. add typing middleware to provide visual cue to user when long running operations are happening
                options.Middleware.Add(new TypingMiddleware());

                // 2. add azure ad auth middleware to get an authorization token to use to connect to Office 365 Graph
                options.Middleware.Add(new AzureAdAuthMiddleware(authTokenStorage, Configuration));

                // 3. add conversation state so that we can use Dialogs
                options.Middleware.Add(new ConversationState<Dictionary<string, object>>(new MemoryStorage()));

                // 4. add spell check middleware to automatically correct inbound message spelling
                options.Middleware.Add(new SpellCheckMiddleware(Configuration));
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseBotFramework();
        }
    }
}
