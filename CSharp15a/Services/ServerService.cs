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
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Bedrock.Framework;
using CSharp15a.Configuration;
using CSharp15a.Worlds;
using CSharp15a.Worlds.Generator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CSharp15a.Services
{
    public class ServerService : IHostedService
    {
        private readonly ILogger<ServerService> _logger;
        private readonly Dictionary<string, WorldOptions> _worldOptions;
        private readonly Lazy<Server> _server;
        private readonly PlayerManager _playerManager;

        public List<World> Worlds { get; } = new List<World>();

        public World? DefaultWorld { get; private set; }

        public ServerService(ILogger<ServerService> logger, IOptions<Dictionary<string, WorldOptions>> worldOptions, Lazy<Server> server, PlayerManager playerManager)
        {
            _logger = logger;
            _worldOptions = worldOptions.Value;
            _server = server;
            _playerManager = playerManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_worldOptions.Values.Count(x => x.Default) != 1)
            {
                throw new Exception("You need exactly 1 default world");
            }

            Directory.CreateDirectory("worlds");

            foreach (var (name, worldOptions) in _worldOptions)
            {
                World world;
                var path = Path.Combine("worlds", name + ".cw");

                if (File.Exists(path))
                {
                    world = ClassicWorld.Load(path);
                    _logger.LogInformation("Loaded {Name} from {Path}", world.Name, path);
                }
                else
                {
                    world = new ClassicWorld(name, Guid.NewGuid(), new Vector3<int>(512, 64, 512), path);

                    IWorldGenerator generator = worldOptions.Generator switch
                    {
                        "classic" => new ClassicWorldGenerator(),
                        "flat" => new FlatWorldGenerator(),
                        _ => throw new ArgumentOutOfRangeException("Unknown world generator: " + worldOptions.Generator)
                    };

                    _logger.LogInformation("Generating {Name} world...", world.Name);
                    generator.Generate(world);
                    generator.SetSpawnPosition(world);

                    await world.SaveAsync();
                }

                if (worldOptions.Default)
                {
                    DefaultWorld = world;
                }

                Worlds.Add(world);
            }

            _logger.LogInformation("Loaded {Worlds} worlds, set {Default} as the default", Worlds.Count, DefaultWorld!.Name);

            await _server.Value.StartAsync(cancellationToken);
            _logger.LogInformation("Listening on {Port}", _server.Value.EndPoints.OfType<IPEndPoint>().Single());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var (connection, player) in _playerManager.Players)
            {
                await connection.DisposeAsync();
                await player.DisposeAsync();
            }

            await _server.Value.StopAsync(cancellationToken);
        }
    }
}
