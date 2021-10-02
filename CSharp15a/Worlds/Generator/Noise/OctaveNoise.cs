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

namespace CSharp15a.Worlds.Generator.Noise
{
    public class OctaveNoise : INoise
    {
        private readonly ImprovedNoise[] _octaves;

        public OctaveNoise(Random random, int octaves = 8)
        {
            _octaves = new ImprovedNoise[octaves];

            for (var i = 0; i < octaves; ++i)
            {
                _octaves[i] = new ImprovedNoise(random);
            }
        }

        public float Compute(float x, float y)
        {
            var sum = 0.0f;
            var amplitude = 1.0f;

            foreach (var octave in _octaves)
            {
                sum += octave.Compute(x / amplitude, y / amplitude) * amplitude;
                amplitude *= 2.0f;
            }

            return sum;
        }
    }
}
