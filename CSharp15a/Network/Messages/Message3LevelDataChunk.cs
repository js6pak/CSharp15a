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
    public readonly struct Message3LevelDataChunk : IMessage
    {
        public MessagesIds Id => MessagesIds.LevelDataChunk;

        public short ChunkLength { get; }

        public byte[] ChunkData { get; }

        public byte PercentComplete { get; }

        public Message3LevelDataChunk(short chunkLength, byte[] chunkData, byte percentComplete)
        {
            ChunkLength = chunkLength;
            ChunkData = chunkData;
            PercentComplete = percentComplete;
        }

        public Message3LevelDataChunk(ref SequenceReader<byte> reader)
        {
            ChunkLength = reader.ReadShort();
            ChunkData = reader.ReadByteArray(1024).ToArray();
            PercentComplete = reader.ReadByte();
        }

        public void Write(IBufferWriter<byte> writer)
        {
            writer.Write(ChunkLength);
            writer.Write(ChunkData);
            writer.Write(PercentComplete);
        }
    }
}
