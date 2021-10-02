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

using Microsoft.Extensions.Configuration;

namespace CSharp15a.Configuration
{
    public class ServerOptions
    {
        public const string Key = "server";

        public string Name { get; init; } = null!;

        [ConfigurationKeyName("max-players")]
        public int MaxPlayers { get; init; }

        [ConfigurationKeyName("online-mode")]
        public bool OnlineMode { get; init; }
    }
}
