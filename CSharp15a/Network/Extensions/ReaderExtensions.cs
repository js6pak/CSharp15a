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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace CSharp15a.Network.Extensions
{
    public static class ReaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ref SequenceReader<byte> input)
        {
            if (input.TryRead(out var value))
            {
                return value;
            }

            throw new EndOfStreamException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(this ref SequenceReader<byte> input)
        {
            if (input.TryReadBigEndian(out short value))
            {
                return value;
            }

            throw new EndOfStreamException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> ReadByteArray(this ref SequenceReader<byte> input, int length)
        {
            try
            {
                return input.CurrentSpan[input.CurrentSpanIndex..length];
            }
            finally
            {
                input.Advance(length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(this ref SequenceReader<byte> input)
        {
            const int stringLength = 64;
            var bytes = input.CurrentSpan[input.CurrentSpanIndex..stringLength];
            input.Advance(stringLength);
            return Encoding.UTF8.GetString(bytes).Trim();
        }
    }
}
