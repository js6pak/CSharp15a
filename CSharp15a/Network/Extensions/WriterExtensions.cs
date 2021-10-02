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
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSharp15a.Network.Extensions
{
    public static class WriterExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this IBufferWriter<byte> writer, byte value)
        {
            writer.GetSpan(sizeof(byte))[0] = value;
            writer.Advance(sizeof(byte));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this IBufferWriter<byte> writer, short value)
        {
            BinaryPrimitives.WriteInt16BigEndian(writer.GetSpan(sizeof(short)), value);
            writer.Advance(sizeof(short));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this IBufferWriter<byte> writer, string text)
        {
            const int stringLength = 64;
            var span = writer.GetSpan(stringLength);
            for (var i = Encoding.UTF8.GetBytes(text, span); i < stringLength; i++)
            {
                span[i] = 0x20;
            }

            writer.Advance(stringLength);
        }
    }
}
