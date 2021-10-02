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

using System.Numerics;
using System.Runtime.CompilerServices;

namespace CSharp15a.Worlds
{
    public class BlocksHolder
    {
        public Vector3<int> Size { get; }
        public BlockType[] Array { get; }
        public int Length => Array.Length;

        public BlocksHolder(Vector3<int> size)
        {
            Size = size;
            Array = new BlockType[size.X * size.Y * size.Z];
        }

        public int GetBlockIndex(int x, int y, int z)
        {
            return (y * Size.Z + z) * Size.X + x;
        }

        public int GetBlockIndex(Vector3<int> position)
        {
            return GetBlockIndex(position.X, position.Y, position.Z);
        }

        public BlockType this[int i]
        {
            get => Array[i];
            set => Array[i] = value;
        }

        public BlockType this[Vector3<int> position]
        {
            get => this[GetBlockIndex(position)];
            set => this[GetBlockIndex(position)] = value;
        }

        public BlockType Get(int x, int y, int z)
        {
            return this[GetBlockIndex(x, y, z)];
        }

        public void Set(int x, int y, int z, BlockType value)
        {
            this[GetBlockIndex(x, y, z)] = value;
        }

        public byte[] AsBytes()
        {
            return Unsafe.As<byte[]>(Array);
        }
    }
}
