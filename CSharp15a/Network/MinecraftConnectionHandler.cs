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
using CSharp15a.Network.Messages;
using CSharp15a.Services;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace CSharp15a.Network
{
    public class MinecraftConnectionHandler : ConnectionHandler
    {
        private readonly MinecraftProtocol _protocol = new MinecraftProtocol();

        private readonly ILogger<MinecraftConnectionHandler> _logger;
        private readonly ServerService _serverService;
        private readonly PlayerManager _playerManager;

        public MinecraftConnectionHandler(ILogger<MinecraftConnectionHandler> logger, ServerService serverService, PlayerManager playerManager)
        {
            _logger = logger;
            _serverService = serverService;
            _playerManager = playerManager;
        }

        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
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

                        Player player;

                        lock (_playerManager.Players)
                        {
                            player = new Player(writer, _protocol, _playerManager.GetNextId(), handshake.PlayerName);
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
                    _logger.LogInformation("Player {Name} ({Id}) disconnected", player.Name, player.Id);
                    await player.DisposeAsync();
                }
            }
        }
    }
}
