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

namespace Bitvantage.NetworkAddressing.InternetProtocol;

public class ConcurrenceNetworkLookup : NetworkLookup
{
    private readonly object _lock = new();

    public override void Add(Network network)
    {
        lock (_lock)
            base.Add(network);
    }

    public override void Add(NetworkKey value)
    {
        lock (_lock)
            base.Add(value);
    }

    public override void Clear()
    {
        lock (_lock)
            base.Clear();
    }

    public override void Remove(Network network)
    {
        lock (_lock)
            base.Remove(network);
    }

    public override bool TryAdd(Network network)
    {
        lock (_lock)
            return base.TryAdd(new NetworkKey(network));
    }

    public override bool TryRemove(Network network)
    {
        lock (_lock)
            return base.TryRemove(network);
    }
}

public class ConcurrenceNetworkLookup<TValue> : NetworkLookup<TValue>
{
    private readonly object _lock = new();

    public override void Add(Network network, TValue? value)
    {
        lock (_lock)
            base.Add(network, value);
    }

    public override void Add(NetworkKeyValuePair<TValue> value)
    {
        lock (_lock)
            base.Add(value);
    }

    public override void Remove(Network network)
    {
        lock (_lock)
            base.Remove(network);
    }

    public override bool TryAdd(Network network, TValue? value)
    {
        lock (_lock)
            return base.TryAdd(new NetworkKeyValuePair<TValue>(network, value));
    }

    public override bool TryRemove(Network network)
    {
        lock (_lock)
            return base.TryRemove(network);
    }
}