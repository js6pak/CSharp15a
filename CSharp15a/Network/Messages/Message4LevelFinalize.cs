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

using System.Buffers;
using CSharp15a.Network.Extensions;

namespace CSharp15a.Network.Messages
{
    public readonly struct Message4LevelFinalize : IMessage
    {
        public MessagesIds Id => MessagesIds.LevelFinalize;

        public short X { get; }

        public short Y { get; }

        public short Z { get; }

        public Message4LevelFinalize(short x, short y, short z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Message4LevelFinalize(ref SequenceReader<byte> reader)
        {
            X = reader.ReadShort();
            Y = reader.ReadShort();
            Z = reader.ReadShort();
        }

        public void Write(IBufferWriter<byte> writer)
        {
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
        }
    }
}
