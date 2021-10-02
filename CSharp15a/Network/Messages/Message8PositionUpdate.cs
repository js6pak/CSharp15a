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
    public readonly struct Message8PositionUpdate : IMessage
    {
        public MessagesIds Id => MessagesIds.PositionUpdate;

        public byte PlayerId { get; }
        public short X { get; }
        public short Y { get; }
        public short Z { get; }
        public byte Yaw { get; }
        public byte Pitch { get; }

        public Message8PositionUpdate(byte playerId, short x, short y, short z, byte yaw, byte pitch)
        {
            PlayerId = playerId;
            X = x;
            Y = y;
            Z = z;
            Yaw = yaw;
            Pitch = pitch;
        }

        public Message8PositionUpdate(ref SequenceReader<byte> reader)
        {
            PlayerId = reader.ReadByte();
            X = reader.ReadShort();
            Y = reader.ReadShort();
            Z = reader.ReadShort();
            Yaw = reader.ReadByte();
            Pitch = reader.ReadByte();
        }

        public void Write(IBufferWriter<byte> writer)
        {
            writer.Write(PlayerId);
            writer.Write(X);
            writer.Write(Y);
            writer.Write(Z);
            writer.Write(Yaw);
            writer.Write(Pitch);
        }
    }
}
