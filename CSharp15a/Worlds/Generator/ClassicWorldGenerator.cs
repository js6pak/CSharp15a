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

// Based off https://github.com/UnknownShadow200/ClassiCu=/wiki/Minecraft-Classic-map-generation-algorithm

using System;
using System.Collections.Generic;
using System.Numerics;
using CSharp15a.Worlds.Generator.Noise;

namespace CSharp15a.Worlds.Generator
{
    public class ClassicWorldGenerator : IWorldGenerator
    {
        private readonly Random _random = new Random();

        public void Generate(World world)
        {
            var heightMap = CreateHeightMap(world);
            CreateStrata(world, heightMap);
            CarveCaves(world);
            // CarveOreVeins coal, iron, gold

            FloodFillWater(world);
            // FloodFillLava

            CreateSurfaceLayer(world, heightMap);
            PlantTrees(world, heightMap);
        }

        private int[] CreateHeightMap(World world)
        {
            var heightMap = new int[world.Size.X * world.Size.Z];

            var noise1 = new CombinedNoise(new OctaveNoise(_random), new OctaveNoise(_random));
            var noise2 = new CombinedNoise(new OctaveNoise(_random), new OctaveNoise(_random));
            var noise3 = new OctaveNoise(_random, 6);

            var i = 0;

            for (var z = 0; z < world.Size.Z; z++)
            {
                for (var x = 0; x < world.Size.X; x++)
                {
                    var heightLow = noise1.Compute(x * 1.3f, z * 1.3f) / 6 - 4;
                    var heightHigh = noise2.Compute(x * 1.3f, z * 1.3f) / 5 + 6;

                    var heightResult = noise3.Compute(x, z) / 8 > 0 ? heightLow : Math.Max(heightLow, heightHigh);

                    heightResult /= 2;

                    if (heightResult < 0)
                    {
                        heightResult *= 0.8f;
                    }

                    heightMap[i++] = (int)(heightResult + world.WaterLevel);
                }
            }

            return heightMap;
        }

        private void CreateStrata(World world, int[] heightMap)
        {
            var noise = new OctaveNoise(_random);

            var i = 0;

            for (var z = 0; z < world.Size.Z; z++)
            {
                for (var x = 0; x < world.Size.X; x++)
                {
                    var dirtThickness = noise.Compute(x, z) / 24 - 4;
                    var dirtTransition = heightMap[i++];
                    var stoneTransition = dirtTransition + dirtThickness;

                    for (var y = 0; y < world.Size.Y; y++)
                    {
                        var blockType = BlockType.Air;

                        if (y == 0)
                        {
                            blockType = BlockType.StationaryLava;
                        }
                        else if (y <= stoneTransition)
                        {
                            blockType = BlockType.Stone;
                        }
                        else if (y <= dirtTransition)
                        {
                            blockType = BlockType.Dirt;
                        }

                        world.Blocks.Set(x, y, z, blockType);
                    }
                }
            }
        }

        private void CarveCaves(World world)
        {
            var cavesCount = world.Size.X * world.Size.Y * world.Size.Z / 8192;

            for (var i = 0; i < cavesCount; i++)
            {
                var cavePosition = new Vector3<float>(_random.Next(world.Size.X), _random.Next(world.Size.Y), _random.Next(world.Size.Z));
                var caveLength = (int)(_random.NextSingle() * _random.NextSingle() * 200);

                var theta = _random.NextSingle() * MathF.PI * 2;
                var deltaTheta = 0f;
                var phi = _random.NextSingle() * MathF.PI * 2;
                var deltaPhi = 0f;

                var caveRadius = _random.NextSingle() * _random.NextSingle();

                for (var length = 0; length < caveLength; length++)
                {
                    cavePosition += new Vector3<float>(
                        MathF.Sin(theta) * MathF.Cos(phi),
                        MathF.Cos(theta) * MathF.Cos(phi),
                        MathF.Sin(phi)
                    );

                    theta += deltaTheta * 0.2f;
                    deltaTheta = deltaTheta * 0.9f + _random.NextSingle() - _random.NextSingle();
                    phi = phi / 2 + deltaPhi / 4;
                    deltaPhi = deltaPhi * 0.75f + _random.NextSingle() - _random.NextSingle();

                    if (_random.NextSingle() >= 0.25)
                    {
                        var centerPosition = new Vector3<int>(
                            (int)(cavePosition.X + (_random.Next(4) - 2) * 0.2f),
                            (int)(cavePosition.Y + (_random.Next(4) - 2) * 0.2f),
                            (int)(cavePosition.Z + (_random.Next(4) - 2) * 0.2f)
                        );

                        var radius = (world.Size.Y - centerPosition.Y) / (float)world.Size.Y;
                        radius = 1.2f + (radius * 3.5f + 1) * caveRadius;
                        radius *= MathF.Sin(length * MathF.PI / caveLength);

                        FillOblateSpheroid(world, centerPosition, radius, BlockType.Air);
                    }
                }
            }
        }

        private void FillOblateSpheroid(World world, Vector3<int> position, double radius, BlockType block)
        {
            var xBeg = (int)Math.Floor(Math.Max(position.X - radius, 0));
            var xEnd = (int)Math.Floor(Math.Min(position.X + radius, world.Size.X - 1));
            var yBeg = (int)Math.Floor(Math.Max(position.Y - radius, 0));
            var yEnd = (int)Math.Floor(Math.Min(position.Y + radius, world.Size.Y - 1));
            var zBeg = (int)Math.Floor(Math.Max(position.Z - radius, 0));
            var zEnd = (int)Math.Floor(Math.Min(position.Z + radius, world.Size.Z - 1));

            var radiusSq = radius * radius;

            for (var yy = yBeg; yy <= yEnd; yy++)
            {
                var dy = yy - position.Y;
                int zz;
                for (zz = zBeg; zz <= zEnd; zz++)
                {
                    var dz = zz - position.Z;
                    int xx;
                    for (xx = xBeg; xx <= xEnd; xx++)
                    {
                        var dx = xx - position.X;

                        if (dx * dx + 2 * dy * dy + dz * dz < radiusSq)
                        {
                            if (world.Blocks.Get(xx, yy, zz) == BlockType.Stone)
                            {
                                world.Blocks.Set(xx, yy, zz, block);
                            }
                        }
                    }
                }
            }
        }

        private void FloodFill(World world, int index, BlockType blockType)
        {
            var oneY = world.Size.X * world.Size.Z;

            var queue = new Queue<int>();
            queue.Enqueue(index);

            while (queue.TryDequeue(out index))
            {
                if (world.Blocks[index] != BlockType.Air) continue;
                world.Blocks[index] = blockType;

                var x = index % world.Size.X;
                var y = index / oneY;
                var z = index / world.Size.X % world.Size.Z;

                if (x > 0)
                {
                    queue.Enqueue(index - 1);
                }

                if (x < world.Size.X - 1)
                {
                    queue.Enqueue(index + 1);
                }

                if (z > 0)
                {
                    queue.Enqueue(index - world.Size.X);
                }

                if (z < world.Size.Z - 1)
                {
                    queue.Enqueue(index + world.Size.X);
                }

                if (y > 0)
                {
                    queue.Enqueue(index - oneY);
                }
            }
        }

        private void FloodFillWater(World world)
        {
            var waterY = world.WaterLevel;

            var index1 = world.Blocks.GetBlockIndex(0, (int)waterY, 0);
            var index2 = world.Blocks.GetBlockIndex(0, (int)waterY, world.Size.Z - 1);
            for (var x = 0; x < world.Size.X; x++)
            {
                FloodFill(world, index1, BlockType.StationaryWater);
                FloodFill(world, index2, BlockType.StationaryWater);
                index1++;
                index2++;
            }

            index1 = world.Blocks.GetBlockIndex(0, (int)waterY, 0);
            index2 = world.Blocks.GetBlockIndex(world.Size.X - 1, (int)waterY, 0);
            for (var z = 0; z < world.Size.Z; z++)
            {
                FloodFill(world, index1, BlockType.StationaryWater);
                FloodFill(world, index2, BlockType.StationaryWater);
                index1 += world.Size.X;
                index2 += world.Size.X;
            }

            var sources = world.Size.X * world.Size.Z / 800;

            for (var i = 0; i < sources; i++)
            {
                var x = _random.Next(world.Size.X);
                var z = _random.Next(world.Size.Z);
                var y = world.WaterLevel - _random.Next(1, 3);

                FloodFill(world, world.Blocks.GetBlockIndex(x, (int)y, z), BlockType.StationaryWater);
            }
        }

        private void CreateSurfaceLayer(World world, int[] heightMap)
        {
            var noise1 = new OctaveNoise(_random);
            var noise2 = new OctaveNoise(_random);

            var i = 0;

            for (var z = 0; z < world.Size.Z; z++)
            {
                for (var x = 0; x < world.Size.X; x++)
                {
                    var sandChance = noise1.Compute(x, z) > 8;
                    var gravelChance = noise2.Compute(x, z) > 12;

                    var y = heightMap[i++];

                    var blockAbove = world.Blocks.Get(x, y + 1, z);

                    if (blockAbove == BlockType.StationaryWater && gravelChance)
                    {
                        world.Blocks.Set(x, y, z, BlockType.Gravel);
                    }

                    if (blockAbove == BlockType.Air)
                    {
                        if (y <= world.WaterLevel && sandChance)
                        {
                            world.Blocks.Set(x, y, z, BlockType.Sand);
                        }
                        else
                        {
                            world.Blocks.Set(x, y, z, BlockType.Grass);
                        }
                    }
                }
            }
        }

        private void PlantTrees(World world, int[] heightMap)
        {
            var patches = world.Size.X * world.Size.Z / 4000;

            for (var i = 0; i < patches; i++)
            {
                var patchX = _random.Next(world.Size.X);
                var patchZ = _random.Next(world.Size.Z);

                for (var j = 0; j < 20; j++)
                {
                    var treeX = patchX;
                    var treeZ = patchZ;

                    for (var k = 0; k < 20; k++)
                    {
                        treeX += _random.Next(6) - _random.Next(6);
                        treeZ += _random.Next(6) - _random.Next(6);

                        if (!world.ContainsXZ(treeX, treeZ) || !(_random.NextDouble() <= 0.25))
                        {
                            continue;
                        }

                        var treeY = heightMap[treeZ * world.Size.X + treeX] + 1;

                        var treePosition = new Vector3<int>(treeX, treeY, treeZ);

                        if (treeY > 0 && world.Blocks[treePosition - Vector3<int>.UnitY] != BlockType.Grass)
                        {
                            continue;
                        }

                        var treeHeight = _random.Next(3) + 5;

                        if (CanGrow(treePosition, treeHeight))
                        {
                            Grow(treePosition, treeHeight);
                        }
                    }
                }
            }

            bool CanGrow(Vector3<int> treePosition, int treeHeight)
            {
                var baseHeight = treeHeight - 4;

                for (var y = treePosition.Y; y < treePosition.Y + treeHeight; y++)
                {
                    var size = y - treePosition.Y > baseHeight ? 2 : 1;

                    for (var z = treePosition.Z - size; z <= treePosition.Z + size; z++)
                    {
                        for (var x = treePosition.X - size; x <= treePosition.X + size; x++)
                        {
                            if (!world.Contains(x, y, z))
                            {
                                return false;
                            }

                            if (world.Blocks.Get(x, y, z) != BlockType.Air)
                            {
                                return false;
                            }
                        }
                    }
                }

                return true;
            }

            void Grow(Vector3<int> treePosition, int treeHeight)
            {
                var topStart = treePosition.Y + (treeHeight - 2);

                for (var y = treePosition.Y + (treeHeight - 4); y < topStart; y++)
                {
                    for (var zz = -2; zz <= 2; zz++)
                    {
                        for (var xx = -2; xx <= 2; xx++)
                        {
                            var x = treePosition.X + xx;
                            var z = treePosition.Z + zz;

                            if (Math.Abs(xx) == 2 && Math.Abs(zz) == 2)
                            {
                                if (_random.NextDouble() >= 0.5f)
                                {
                                    world.Blocks.Set(x, y, z, BlockType.Leaves);
                                }
                            }
                            else
                            {
                                world.Blocks.Set(x, y, z, BlockType.Leaves);
                            }
                        }
                    }
                }

                for (var y = topStart; y < treePosition.Y + treeHeight; y++)
                {
                    for (var zz = -1; zz <= 1; zz++)
                    {
                        for (var xx = -1; xx <= 1; xx++)
                        {
                            var x = xx + treePosition.X;
                            var z = zz + treePosition.Z;

                            if (xx == 0 || zz == 0)
                            {
                                world.Blocks.Set(x, y, z, BlockType.Leaves);
                            }
                            else if (y == topStart && _random.NextDouble() >= 0.5f)
                            {
                                world.Blocks.Set(x, y, z, BlockType.Leaves);
                            }
                        }
                    }
                }

                for (var y = 0; y < treeHeight - 1; y++)
                {
                    world.Blocks.Set(treePosition.X, treePosition.Y + y, treePosition.Z, BlockType.Wood);
                }
            }
        }
    }
}
