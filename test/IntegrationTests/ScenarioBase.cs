﻿using Commitments.API;
using Commitments.Infrastructure.Data;
using Commitments.Core.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net.Http;

namespace IntegrationTests
{
    public class ScenarioBase
    {
        protected TestServer CreateServer()
        {
            var webHostBuilder = new WebHostBuilder()
                    .UseStartup(typeof(Startup))
                    .UseKestrel()
                    .UseConfiguration(GetConfiguration())
                    .ConfigureAppConfiguration((builderContext, config) =>
                    {
                        config
                        .AddJsonFile("settings.json");
                    });

            var testServer = new TestServer(webHostBuilder);
            
            ResetDatabase(testServer.Host);

            return testServer;
        }

        protected void ResetDatabase(IWebHost host)
        {            
            var services = (IServiceScopeFactory)host.Services.GetService(typeof(IServiceScopeFactory));

            using (var scope = services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();

                SeedData.Seed(context);
            }
        }

        protected HubConnection GetHubConnection(HttpMessageHandler httpMessageHandler) 
            => new HubConnectionBuilder()
                            .WithUrl($"http://integrationtests/hub?token={GetAccessToken()}", options => {
                                options.Transports = HttpTransportType.LongPolling;
                                options.HttpMessageHandlerFactory = h => httpMessageHandler;
                            })
                            .Build();

        protected IConfiguration GetConfiguration() => new ConfigurationBuilder()
                .SetBasePath(Path.GetFullPath(@"../../../../../src/Commitments.API/"))
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

        protected string GetAccessToken() {
            var tokenProvider = new TokenProvider(GetConfiguration());
            return tokenProvider.Get("integration@tests.com");
        }
    }
}
