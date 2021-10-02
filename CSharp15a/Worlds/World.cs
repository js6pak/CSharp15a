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
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using CSharp15a.Network.Messages;

namespace CSharp15a.Worlds
{
    public abstract class World
    {
        public string Name { get; }

        public Guid Uuid { get; }

        public Vector3<int> Size { get; }

        public Vector3<int> SpawnPosition { get; set; }

        public float SpawnYaw { get; set; }

        public float SpawnPitch { get; set; }

        public BlocksHolder Blocks { get; }

        public float WaterLevel => Size.Y / 2f;

        public List<Player> Players { get; } = new List<Player>();

        protected World(string name, Guid uuid, Vector3<int> size)
        {
            Size = size;
            Uuid = uuid;
            Name = name;
            Blocks = new BlocksHolder(Size);
        }

        public abstract Task SaveAsync();

        public int GetHighestY(int x, int z)
        {
            int y;

            for (y = Size.Y; Blocks.Get(x, y - 1, z) == BlockType.Air && y > 0; --y)
            {
            }

            return y;
        }

        public bool ContainsXZ(int x, int z)
        {
            return x >= 0 && x < Size.X &&
                   z >= 0 && z < Size.Z;
        }

        public bool Contains(int x, int y, int z)
        {
            return ContainsXZ(x, z) &&
                   y >= 0 && y < Size.Y;
        }

        public bool Contains(Vector3<int> position)
        {
            return Contains(position.X, position.Y, position.Z);
        }

        public Task BroadcastAsync(IMessage message)
        {
            return Task.WhenAll(Players.Select(player => player.SendAsync(message)));
        }

        public async Task SendWorldAsync(Player player)
        {
            await using var memoryStream = new MemoryStream();
            await using (var compressedStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                compressedStream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Blocks.Length)));
                compressedStream.Write(Blocks.AsBytes());
            }

            var data = memoryStream.ToArray();
            var chunkData = new byte[1024];

            await player.SendAsync(new Message2LevelInitialize());

            var chunksCount = Math.Ceiling(data.Length / 1024d);

            for (var i = 0; i < chunksCount; i++)
            {
                var offset = chunkData.Length * i;
                var length = Math.Min(chunkData.Length, data.Length - offset);

                Buffer.BlockCopy(data, offset, chunkData, 0, length);
                await player.SendAsync(new Message3LevelDataChunk((short)length, chunkData, (byte)((i + 1) / chunksCount * 100)));
            }

            await player.SendAsync(new Message4LevelFinalize((short)Size.X, (short)Size.Y, (short)Size.Z));
        }

        public async Task RemovePlayerAsync(Player player)
        {
            if (!Players.Contains(player))
                return;

            Players.Remove(player);
            await BroadcastAsync(new Message9DespawnPlayer(player.Id));
        }
    }
}
