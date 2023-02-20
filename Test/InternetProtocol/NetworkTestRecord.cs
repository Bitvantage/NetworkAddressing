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

using System.Net;

namespace Test.InternetProtocol;

public record NetworkTestRecord
{
    public IPAddress Address { get; init; }
    public IPAddress Broadcast { get; init; }
    public IPAddress? FirstHost { get; init; }
    public IPAddress? LastHost { get; init; }
    public IPAddress Mask { get; init; }
    public IPAddress Network { get; init; }
    public int Prefix { get; init; }
    public IPAddress Wildcard { get; init; }


    public NetworkTestRecord(string address, int prefix, string network, string mask, string broadcast, string wildcard, string firstHost, string lastHost)
    {
        Address = IPAddress.Parse(address);
        Network = IPAddress.Parse(network);
        Prefix = prefix;
        Mask = IPAddress.Parse(mask);
        Broadcast = IPAddress.Parse(broadcast);
        Wildcard = IPAddress.Parse(wildcard);
        FirstHost = IPAddress.Parse(firstHost);
        LastHost = IPAddress.Parse(lastHost);
    }
}