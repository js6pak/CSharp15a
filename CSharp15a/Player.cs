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
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Bedrock.Framework.Protocols;
using CSharp15a.Network;
using CSharp15a.Network.Messages;
using CSharp15a.Worlds;
using Microsoft.AspNetCore.Connections;

namespace CSharp15a
{
    public class Player : IAsyncDisposable
    {
        private readonly ProtocolWriter _writer;
        private readonly MinecraftProtocol _protocol;
        private readonly Timer _pingTimer;

        public byte Id { get; }
        public string Name { get; }

        public World? World { get; private set; }

        public Vector3 Position { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }

        public Player(ProtocolWriter connectionContext, MinecraftProtocol protocol, byte id, string name)
        {
            _writer = connectionContext;
            _protocol = protocol;

            Id = id;
            Name = name;

            _pingTimer = new Timer(PingAsync, null, 500, 500);
        }

        public async ValueTask DisposeAsync()
        {
            await _pingTimer.DisposeAsync();

            if (World != null)
            {
                await World.RemovePlayerAsync(this);
            }
        }

        private async void PingAsync(object? _)
        {
            await SendAsync(new Message1Ping());
        }

        public async Task SendAsync(IMessage message)
        {
            await _writer.WriteAsync(_protocol, message);
        }

        public async Task HandleMessageAsync(IMessage message)
        {
            switch (message)
            {
                case Message5RequestSetBlock setBlock:
                {
                    if (World == null)
                        return;

                    var blockType = setBlock.Mode == Message5RequestSetBlock.RequestMode.Place ? setBlock.BlockType : BlockType.Air;
                    World.Blocks.Set(setBlock.X, setBlock.Y, setBlock.Z, blockType);
                    await World.BroadcastAsync(new Message6SetBlock(setBlock.X, setBlock.Y, setBlock.Z, blockType));

                    break;
                }

                case Message8PositionUpdate positionUpdate:
                {
                    if (World == null)
                        return;

                    Position = new Vector3(positionUpdate.X / 32f, positionUpdate.Y / 32f, positionUpdate.Z / 32f);
                    Yaw = positionUpdate.Yaw * 360 / 256.0F;
                    Pitch = positionUpdate.Pitch * 360 / 256.0F;

                    await BroadcastPositionAsync();
                    break;
                }

                default:
                {
                    throw new ConnectionAbortedException();
                }
            }
        }

        private (short X, short Y, short Z, byte yaw, byte pitch) GetPacketPosition()
        {
            return (
                (short)(Position.X * 32f),
                (short)(Position.Y * 32f),
                (short)(Position.Z * 32f),
                (byte)((int)(Yaw * 256.0F / 360.0F) & 255),
                (byte)((int)(Pitch * 256.0F / 360.0F) & 255)
            );
        }

        public async Task BroadcastPositionAsync()
        {
            if (World == null)
                throw new NullReferenceException(nameof(World));

            var (x, y, z, yaw, pitch) = GetPacketPosition();
            await World.BroadcastAsync(new Message8PositionUpdate(Id, x, y, z, yaw, pitch));
        }

        public async Task SpawnAsync(World world)
        {
            if (World != null)
            {
                await World.RemovePlayerAsync(this);
            }

            World = world;
            World.Players.Add(this);

            await world.SendWorldAsync(this);

            Position = new Vector3(world.SpawnPosition.X + 0.5f, world.SpawnPosition.Y + 0.5f, world.SpawnPosition.Z + 0.5f);
            Yaw = world.SpawnYaw;
            Pitch = world.SpawnPitch;

            var (x, y, z, yaw, pitch) = GetPacketPosition();
            await SendAsync(new Message7SpawnPlayer(byte.MaxValue, Name, x, y, z, yaw, pitch));

            foreach (var otherPlayer in world.Players.Where(otherPlayer => otherPlayer != this))
            {
                await otherPlayer.SendAsync(new Message7SpawnPlayer(Id, Name, x, y, z, yaw, pitch));

                var (otherX, otherY, otherZ, otherYaw, otherPitch) = GetPacketPosition();
                await SendAsync(new Message7SpawnPlayer(otherPlayer.Id, otherPlayer.Name, otherX, otherY, otherZ, otherYaw, otherPitch));
            }
        }
    }
}
