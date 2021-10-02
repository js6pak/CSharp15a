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

namespace CSharp15a.Worlds.Generator
{
    public class FlatWorldGenerator : IWorldGenerator
    {
        public void Generate(World world)
        {
            for (var x = 0; x < world.Size.X; x++)
            {
                var i = 0;

                for (var y = world.Size.Y / 2; y >= 0; y--)
                {
                    for (var z = 0; z < world.Size.Z; z++)
                    {
                        world.Blocks.Set(x, y, z, i switch
                        {
                            0 => BlockType.Grass,
                            <= 5 => BlockType.Dirt,
                            _ => BlockType.Stone
                        });
                    }

                    i++;
                }
            }
        }
    }
}
