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
using System.Threading;
using System.Threading.Tasks;
using Bedrock.Framework.Protocols;
using CSharp15a.Configuration;
using CSharp15a.Network.Messages;
using CSharp15a.Services;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharp15a.Network
{
    public class MinecraftConnectionHandler : ConnectionHandler
    {
        private readonly MinecraftProtocol _protocol = new MinecraftProtocol();

        private readonly ILogger<MinecraftConnectionHandler> _logger;
        private readonly ServerOptions _serverOptions;
        private readonly ServerService _serverService;
        private readonly PlayerManager _playerManager;
        private readonly BetacraftManager _betacraftManager;

        public MinecraftConnectionHandler(ILogger<MinecraftConnectionHandler> logger, IOptions<ServerOptions> serverOptions, ServerService serverService, PlayerManager playerManager, BetacraftManager betacraftManager)
        {
            _logger = logger;
            _serverOptions = serverOptions.Value;
            _serverService = serverService;
            _playerManager = playerManager;
            _betacraftManager = betacraftManager;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            string? disconnectionMessage = null;

            var reader = connection.CreateReader();
            var writer = connection.CreateWriter();

            while (!connection.ConnectionClosed.IsCancellationRequested)
            {
                try
                {
                    var result = await reader.ReadAsync(_protocol);
                    var message = result.Message;
                    if (message == null)
                    {
                        break;
                    }

                    if (message is Message0Handshake handshake)
                    {
                        if (_playerManager.Players.ContainsKey(connection))
                        {
                            throw new ConnectionAbortedException("Tried to send handshake twice");
                        }

                        if (_playerManager.Players.Count >= _playerManager.Max)
                        {
                            throw new ConnectionAbortedException("Server is full");
                        }

                        Guid? uuid = null;

                        if (_serverOptions.OnlineMode)
                        {
                            try
                            {
                                uuid = await _betacraftManager.HasJoinedAsync(connection.LocalEndPoint!, handshake.PlayerName);
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Error during authentication");
                            }

                            if (uuid == null)
                            {
                                throw new ConnectionAbortedException("Authentication failed");
                            }

                            _logger.LogInformation("Player {Name} successfully authenticated as {Uuid}", handshake.PlayerName, uuid);
                        }

                        Player player;

                        lock (_playerManager.Players)
                        {
                            player = new Player(writer, _protocol, _playerManager.GetNextId(), handshake.PlayerName, uuid);
                            if (!_playerManager.Players.TryAdd(connection, player))
                            {
                                throw new ConnectionAbortedException("Failed to add player");
                            }
                        }

                        await player.SendAsync(new Message0Handshake("Hello from C#15a!"));

                        _logger.LogInformation("Player {Name} ({Id}) connected", player.Name, player.Id);

                        if (_serverService.DefaultWorld != null)
                        {
                            await player.SpawnAsync(_serverService.DefaultWorld);
                        }
                    }
                    else
                    {
                        if (!_playerManager.Players.TryGetValue(connection, out var player))
                        {
                            throw new ConnectionAbortedException("Tried to send packets without completing handshake");
                        }

                        await player.HandleMessageAsync(message);
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex is ConnectionResetException or ConnectionAbortedException)
                {
                    disconnectionMessage = ex.Message;

                    var source = new CancellationTokenSource();
                    source.Cancel();
                    connection.ConnectionClosed = source.Token;
                    break;
                }
                finally
                {
                    reader.Advance();
                }
            }

            // disconnected
            {
                if (_playerManager.Players.TryRemove(connection, out var player))
                {
                    _logger.LogInformation("Player {Name} ({Id}) disconnected - {Message}", player.Name, player.Id, disconnectionMessage);
                    await player.DisposeAsync();
                }
                else
                {
                    _logger.LogInformation("Client {Address} disconnected - {Message}", connection.RemoteEndPoint?.ToString(), disconnectionMessage);
                }
            }
        }
    }
}
