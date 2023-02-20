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

using System.Collections;
using System.Runtime.CompilerServices;

namespace Bitvantage.NetworkAddressing.InternetProtocol;

public class NetworkLookup : NetworkLookupBase<NetworkKey>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(Network network)
    {
        Add(new NetworkKey(network));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool TryAdd(Network network)
    {
        return TryAdd(new NetworkKey(network));
    }
}

public class NetworkLookup<TValue> : NetworkLookupBase<NetworkKeyValuePair<TValue>>, IEnumerable<NetworkKeyValuePair<TValue>>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(Network network, TValue? value)
    {
        Add(new NetworkKeyValuePair<TValue>(network, value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual NetworkKeyValuePair<TValue> GetOrAdd(Network address, Func<Network, TValue> valueFactoryFunc)
    {
        if (TryGetMatch(address, out var networkValuePair))
            return networkValuePair;

        networkValuePair = new NetworkKeyValuePair<TValue>(address, valueFactoryFunc.Invoke(address));
        TryAdd(networkValuePair);

        return networkValuePair;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool TryAdd(Network network, TValue? value)
    {
        return TryAdd(new NetworkKeyValuePair<TValue>(network, value));
    }

    IEnumerator<NetworkKeyValuePair<TValue>> IEnumerable<NetworkKeyValuePair<TValue>>.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}