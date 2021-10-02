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
using System.Numerics;
using System.Threading.Tasks;
using fNbt;

namespace CSharp15a.Worlds
{
    public class ClassicWorld : World
    {
        public const byte FormatVersion = 1;

        public string Path { get; }

        public ClassicWorld(string name, Guid uuid, Vector3<int> size, string path) : base(name, uuid, size)
        {
            Path = path;
        }

        public static ClassicWorld Load(string file)
        {
            var nbt = new NbtFile(file).RootTag;

            if (nbt.Name != "ClassicWorld")
            {
                throw new FormatException("NBT file is not an ClassicWorld");
            }

            var formatVersion = nbt["FormatVersion"].ByteValue;

            if (formatVersion != FormatVersion)
            {
                throw new FormatException("Unsupported ClassicWorld format version");
            }

            var name = nbt["Name"]?.StringValue ?? "Unnamed world";
            var uuid = nbt["UUID"].ByteArrayValue;

            var sizeX = nbt["X"].ShortValue;
            var sizeY = nbt["Y"].ShortValue;
            var sizeZ = nbt["Z"].ShortValue;

            var world = new ClassicWorld(name, new Guid(uuid), new Vector3<int>(sizeX, sizeY, sizeZ), file);

            var createdBy = nbt["CreatedBy"];

            if (createdBy != null)
            {
                var service = createdBy["Service"].StringValue;
                var username = createdBy["Username"].StringValue;
            }

            var mapGenerator = nbt["MapGenerator"];

            if (mapGenerator != null)
            {
                var software = mapGenerator["Software"].StringValue;
                var mapGeneratorName = mapGenerator["MapGeneratorName"].StringValue;
            }

            var timeCreated = nbt["TimeCreated"]?.LongValue;
            var lastAccessed = nbt["LastAccessed"]?.LongValue;
            var lastModified = nbt["LastModified"]?.LongValue;

            var spawn = nbt["Spawn"];
            var spawnX = spawn["X"].ShortValue;
            var spawnY = spawn["Y"].ShortValue;
            var spawnZ = spawn["Z"].ShortValue;
            var yaw = spawn["H"].ByteValue;
            var pitch = spawn["P"].ByteValue;

            world.SpawnPosition = new Vector3<int>(spawnX, spawnY, spawnZ);
            world.SpawnYaw = yaw;
            world.SpawnPitch = pitch;

            var blocks = nbt["BlockArray"].ByteArrayValue;

            if (blocks.Length != world.Blocks.Length)
            {
                throw new FormatException("BlockArray size doesn't match world size");
            }

            Buffer.BlockCopy(blocks, 0, world.Blocks.Array, 0, blocks.Length);

            var metadata = nbt["Metadata"];

            return world;
        }

        public override Task SaveAsync()
        {
            var nbt = new NbtCompound("ClassicWorld")
            {
                new NbtByte("FormatVersion", FormatVersion),
                new NbtString("Name", Name),
                new NbtByteArray("UUID", Uuid.ToByteArray()),

                new NbtShort("X", (short)Size.X),
                new NbtShort("Y", (short)Size.Y),
                new NbtShort("Z", (short)Size.Z),

                new NbtCompound("Spawn")
                {
                    new NbtShort("X", (short)SpawnPosition.X),
                    new NbtShort("Y", (short)SpawnPosition.Y),
                    new NbtShort("Z", (short)SpawnPosition.Z),
                    new NbtByte("H", (byte)SpawnYaw),
                    new NbtByte("P", (byte)SpawnPitch),
                },

                new NbtByteArray("BlockArray", Blocks.AsBytes()),
                new NbtCompound("Metadata")
            };

            new NbtFile(nbt).SaveToFile(Path, NbtCompression.GZip);

            return Task.CompletedTask;
        }
    }
}
