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
using System.Buffers;
using Bedrock.Framework.Protocols;
using CSharp15a.Network.Extensions;
using CSharp15a.Network.Messages;

namespace CSharp15a.Network
{
    public class MinecraftProtocol : IMessageReader<IMessage?>, IMessageWriter<IMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out IMessage? message)
        {
            var reader = new SequenceReader<byte>(input);
            if (!reader.TryRead(out var id))
            {
                message = default;
                return false;
            }

            message = (MessagesIds) id switch
            {
                MessagesIds.Handshake => new Message0Handshake(ref reader),
                MessagesIds.Ping => new Message1Ping(),
                MessagesIds.LevelInitialize => new Message2LevelInitialize(),
                MessagesIds.LevelDataChunk => new Message3LevelDataChunk(ref reader),
                MessagesIds.LevelFinalize => new Message4LevelFinalize(ref reader),
                MessagesIds.RequestSetBlock => new Message5RequestSetBlock(ref reader),
                MessagesIds.SetBlock => new Message6SetBlock(ref reader),
                MessagesIds.SpawnPlayer => new Message7SpawnPlayer(ref reader),
                MessagesIds.PositionUpdate => new Message8PositionUpdate(ref reader),
                MessagesIds.DespawnPlayer => new Message9DespawnPlayer(ref reader),
                _ => throw new ArgumentOutOfRangeException()
            };

            examined = consumed = reader.Position;
            return true;
        }

        public void WriteMessage(IMessage message, IBufferWriter<byte> output)
        {
            output.Write((byte) message.Id);
            message.Write(output);
        }
    }
}
