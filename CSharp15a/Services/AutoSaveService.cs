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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CSharp15a.Services
{
    public class AutoSaveService : IHostedService, IDisposable
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

        private readonly ILogger<AutoSaveService> _logger;
        private readonly ServerService _server;
        private Timer? _timer;
        private Task? _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public AutoSaveService(ILogger<AutoSaveService> logger, ServerService server)
        {
            _logger = logger;
            _server = server;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(_ =>
            {
                _timer?.Change(Timeout.Infinite, 0);
                _executingTask = SaveAsync();
            }, null, Interval, Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        private async Task SaveAsync()
        {
            foreach (var world in _server.Worlds)
            {
                _logger.LogInformation("Saving {World}", world.Name);

                try
                {
                    await world.SaveAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to save {World}", world.Name);
                }
            }

            _timer?.Change(Interval, TimeSpan.FromMilliseconds(-1));
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            if (_executingTask == null)
            {
                await SaveAsync();
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, stoppingToken));
            }

            await SaveAsync();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
