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
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CSharp15a.Services
{
    // TODO convert to a plugin
    public class BetacraftManager
    {
        public async Task<Guid?> HasJoinedAsync(EndPoint serverAddress, string playerName)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(serverAddress.ToString()!));
            var hashText = Convert.ToHexString(hash).ToLower();

            var httpResponseMessage = await new HttpClient().GetAsync($"https://sessionserver.mojang.com/session/minecraft/hasJoined?username={playerName}&serverId={hashText}");

            if (httpResponseMessage.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var response = JsonSerializer.Deserialize<HasJoinedResponse>(await httpResponseMessage.Content.ReadAsStreamAsync());
            return response?.Id;
        }

        public class HasJoinedResponse
        {
            [JsonPropertyName("id")]
            [JsonConverter(typeof(GuidJsonConverter))]
            public Guid Id { get; }

            [JsonPropertyName("name")]
            public string Name { get; }

            [JsonConstructor]
            public HasJoinedResponse(Guid id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        public class GuidJsonConverter : JsonConverter<Guid>
        {
            public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return Guid.Parse(reader.GetString()!);
            }

            public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
            {
                throw new NotSupportedException();
            }
        }
    }
}
