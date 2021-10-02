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
using System.Numerics;

namespace CSharp15a.Worlds.Generator
{
    public interface IWorldGenerator
    {
        void Generate(World world);

        void SetSpawnPosition(World world)
        {
            int x, y, z;

            do
            {
                x = Random.Shared.Next(world.Size.X / 2) + world.Size.X / 4;
                z = Random.Shared.Next(world.Size.Z / 2) + world.Size.Z / 4;
                y = world.GetHighestY(x, z) + 1;
            } while (y <= world.WaterLevel || world.Blocks.Get(x, y - 2, z).IsLiquid());

            world.SpawnPosition = new Vector3<int>(x, y, z);
        }
    }
}
