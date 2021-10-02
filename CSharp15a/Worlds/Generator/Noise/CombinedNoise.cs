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

namespace CSharp15a.Worlds.Generator.Noise
{
    public class CombinedNoise : INoise
    {
        public INoise Noise1 { get; }
        public INoise Noise2 { get; }

        public CombinedNoise(INoise noise1, INoise noise2)
        {
            Noise1 = noise1;
            Noise2 = noise2;
        }

        public float Compute(float x, float y)
        {
            return Noise1.Compute(x + Noise2.Compute(x, y), y);
        }
    }
}
