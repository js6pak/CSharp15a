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

// Adapted from https://github.com/UnknownShadow200/ClassiCube/blob/76e5c2b8992ba9e788d7d12c949f4fa3629e54a8/src/Generator.c#L61-L112
/*
Copyright (c) 2014 - 2021, UnknownShadow200
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this 
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this 
list of conditions and the following disclaimer in the documentation and/or other 
materials provided with the distribution.

3. Neither the name of ClassiCube nor the names of its contributors may be 
used to endorse or promote products derived from this software without specific prior 
written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;

namespace CSharp15a.Worlds.Generator.Noise
{
    public class ImprovedNoise : INoise
    {
        private readonly int[] _p;

        public ImprovedNoise(Random random)
        {
            _p = new int[512];

            for (var i = 0; i < 256; _p[i] = i++)
            {
            }

            for (var i = 0; i < 256; ++i)
            {
                var r = random.Next(i, 256);
                (_p[i], _p[r]) = (_p[r], _p[i]);
                _p[i + 256] = _p[i];
            }
        }

        // Don't ask me wtf is going on here
        public float Compute(float x, float y)
        {
            var xFloor = x >= 0 ? (int)x : (int)x - 1;
            var yFloor = y >= 0 ? (int)y : (int)y - 1;
            var x2 = xFloor & 0xFF;
            var y2 = yFloor & 0xFF;
            x -= xFloor;
            y -= yFloor;

            var u = x * x * x * (x * (x * 6 - 15) + 10); /* Fade(x) */
            var v = y * y * y * (y * (y * 6 - 15) + 10); /* Fade(y) */
            var a = _p[x2] + y2;
            var b = _p[x2 + 1] + y2;

            /* Normally, calculating Grad involves a function call. However, we can directly pack this table
            (since each value indicates either -1, 0 1) into a set of bit flags. This way we avoid needing
            to call another function that performs branching */
            const int xFlags = 0x46552222;
            const int yFlags = 0x2222550A;

            var hash = (_p[_p[a]] & 0xF) << 1;
            var g22 = (((xFlags >> hash) & 3) - 1) * x + (((yFlags >> hash) & 3) - 1) * y; /* Grad(p[p[A], x, y) */
            hash = (_p[_p[b]] & 0xF) << 1;
            var g12 = (((xFlags >> hash) & 3) - 1) * (x - 1) + (((yFlags >> hash) & 3) - 1) * y; /* Grad(p[p[B], x - 1, y) */
            var c1 = g22 + u * (g12 - g22);

            hash = (_p[_p[a + 1]] & 0xF) << 1;
            var g21 = (((xFlags >> hash) & 3) - 1) * x + (((yFlags >> hash) & 3) - 1) * (y - 1); /* Grad(p[p[A + 1], x, y - 1) */
            hash = (_p[_p[b + 1]] & 0xF) << 1;
            var g11 = (((xFlags >> hash) & 3) - 1) * (x - 1) + (((yFlags >> hash) & 3) - 1) * (y - 1); /* Grad(p[p[B + 1], x - 1, y - 1) */
            var c2 = g21 + u * (g11 - g21);

            return c1 + v * (c2 - c1);
        }
    }
}
