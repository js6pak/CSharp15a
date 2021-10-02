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

namespace CSharp15a.Worlds
{
    public enum BlockType : byte
    {
        Air = 0,
        Stone = 1,
        Grass = 2,
        Dirt = 3,
        Cobblestone = 4,
        Planks = 5,
        Sapling = 6,
        Bedrock = 7,
        FlowingWater = 8,
        StationaryWater = 9,
        FlowingLava = 10,
        StationaryLava = 11,
        Sand = 12,
        Gravel = 13,
        GoldOre = 14,
        IronOre = 15,
        CoalOre = 16,
        Wood = 17,
        Leaves = 18
    }

    public static class BlockTypeExtensions
    {
        public static bool IsLiquid(this BlockType blockType)
        {
            return blockType is BlockType.FlowingWater or BlockType.StationaryWater or BlockType.FlowingLava or BlockType.StationaryLava;
        }
    }
}
