/*
    This file is part of NetworkAddressing library copyright (C) 2023 Michael Crino

    The NetworkAddressing library is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License v2.1 as published by
    the Free Software Foundation.

    The NetworkAddressing library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
    or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more 
    details.

    You should have received a copy of the GNU Lesser General Public License
    along with the NetworkAddressing library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

namespace Bitvantage.NetworkAddressing.Ethernet;

public record OuiDatabaseOptions
{
    public bool AutomaticUpdates { get; init; } = true;
    public string? CacheDirectory { get; init; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkAddressing");
    public TimeSpan CheckInterval { get; init; } = TimeSpan.FromHours(1);
    public Uri Location { get; init; } = new("https://standards-oui.ieee.org/");
    public TimeSpan RefreshInterval { get; init; } = TimeSpan.FromDays(30);
    public bool SynchronousUpdate { get; init; } = false;
    public bool ThrowOnUpdateFailure { get; init; } = false;

    public event EventHandler<UpdateEventArgs> DatabaseEvent;

    internal virtual void OnDatabaseEvent(UpdateEventArgs eventArgs)
    {
        DatabaseEvent?.Invoke(this, eventArgs);
    }
}

public record UpdateEventArgs
{
    public enum LogLevel
    {
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5
    }

    public DateTime DateTime { get; } = DateTime.Now;
    public long EventId { get; }
    public Exception? Exception { get; }
    public LogLevel Level { get; }
    public string Message { get; }

    public UpdateEventArgs(LogLevel level, long eventId, string message, Exception? exception = null)
    {
        Level = level;
        EventId = eventId;
        Message = message;
        Exception = exception;
    }
}