// This file is part of CSharp15a.
// 
// CSharp15a is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CSharp15a is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with CSharp15a. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bedrock.Framework;
using CSharp15a.Configuration;
using CSharp15a.Network;
using CSharp15a.Services;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Tommy.Extensions.Configuration;

namespace CSharp15a
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("C#15a starting...");

            try
            {
                await Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration(configuration =>
                    {
                        configuration.AddTomlFile("config.toml");
                    })
                    .ConfigureServices((host, services) =>
                    {
                        services.AddOptions<ServerOptions>().Bind(host.Configuration.GetSection(ServerOptions.Key));
                        services.AddOptions<Dictionary<string, WorldOptions>>().Bind(host.Configuration.GetSection(WorldOptions.Key));

                        services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);

                        services.AddSingleton<PlayerManager>();
                        services
                            .AddSingleton<ServerService>()
                            .AddSingleton<IHostedService, ServerService>(serviceProvider => serviceProvider.GetRequiredService<ServerService>());

                        services.AddSingleton<Server>(serviceProvider =>
                        {
                            return new ServerBuilder(serviceProvider)
                                .UseSockets(sockets =>
                                {
                                    sockets.ListenAnyIP(5565, builder =>
                                    {
                                        builder.UseConnectionLogging().UseConnectionHandler<MinecraftConnectionHandler>();
                                    });
                                }).Build();
                        });

                        services.AddSingleton(serviceProvider => new Lazy<Server>(serviceProvider.GetRequiredService<Server>));

                        services.AddHostedService<AutoSaveService>();
                    })
                    .UseSerilog((host, logger) =>
                    {
                        logger.WriteTo.Console();
                        logger.ReadFrom.Configuration(host.Configuration);
                    })
                    .RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
