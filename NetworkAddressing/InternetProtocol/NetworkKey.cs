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

// TODO: should these be classes?
public record NetworkKey()
{
    public Network Network { get; internal set; }

    // should there be an empty network type?
    public NetworkKey(Network network) : this()
    {
        Network = network;
    }
}

public record NetworkKeyValuePair<TValue>() : NetworkKey
{
    public TValue? Value { get; }

    // should there be an empty network type?
    public NetworkKeyValuePair(Network network, TValue? value) : this()
    {
        Network = network;
        Value = value;
    }
}
