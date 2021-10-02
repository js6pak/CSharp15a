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

using System.Collections.Concurrent;
using System.Linq;
using CSharp15a.Configuration;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;

namespace CSharp15a.Services
{
    public class PlayerManager
    {
        public PlayerManager(IOptions<ServerOptions> options)
        {
            Max = options.Value.MaxPlayers;
        }

        public int Max { get; }

        public ConcurrentDictionary<ConnectionContext, Player> Players { get; } = new ConcurrentDictionary<ConnectionContext, Player>();

        public byte GetNextId()
        {
            byte id;

            for (id = 0; Players.Any(x => x.Value.Id == id); id++)
            {
            }

            return id;
        }
    }
}
