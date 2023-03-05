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

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using Bitvantage.NetworkAddressing.InternetProtocol.Converters;

namespace Bitvantage.NetworkAddressing.InternetProtocol;

public enum NetworkFormat
{
    AddressAndPrefix,
    Detail,
    AddressAndMask,
    Address,
    Mask
}

public enum AddressAllocation
{
    Undefined = 0,
    Private = 1,
    Global = 2,
    AutomaticPrivateIPAddressing = 3,
    Loopback = 4,
    CurrentNetwork = 5,
    Multicast = 6,
    Experimental = 7,
    Broadcast = 8
}

public enum NetworkClass
{
    Undefined = 0,
    A = 1,
    B = 2,
    C = 3,
    D = 4,
    E = 5
}

public enum IPVersion
{
    Undefined = 0,
    IPv4 = 1,
    IPv6 = 2
}

[Serializable]
[JsonConverter(typeof(NetworkJsonConverter))]
public class Network : IComparable<Network>
{
    private static readonly UInt128[] Ipv4HostMaskBits = new UInt128[33];
    private static readonly UInt128[] Ipv6HostMaskBits = new UInt128[129];

    private static readonly UInt128[] Ipv4NetworkMaskBits = new UInt128[33];
    private static readonly UInt128[] Ipv6NetworkMaskBits = new UInt128[129];

    private static readonly IPAddress[] Ipv4HostMaskObjects = new IPAddress[33];
    private static readonly IPAddress[] Ipv6HostMaskObjects = new IPAddress[129];

    private static readonly IPAddress[] Ipv4NetworkMaskObjects = new IPAddress[33];
    private static readonly IPAddress[] Ipv6NetworkMaskObjects = new IPAddress[129];

    private static readonly Dictionary<IPAddress, int> IpHostMaskToPrefix = new();
    private static readonly Dictionary<IPAddress, int> IpNetworkMaskToPrefix = new();

    private static readonly UInt128[] Ipv4HostCount = new UInt128[33];
    private static readonly UInt128[] Ipv6HostCount = new UInt128[129];

    private static readonly BigInteger[] Ipv4AddressCount = new BigInteger[33];
    private static readonly BigInteger[] Ipv6AddressCount = new BigInteger[129];

    private static readonly NetworkLookup<AddressAllocation> Allocations;
    private static readonly NetworkLookup<NetworkClass> Classes;


    internal readonly ushort AddressLength;
    internal readonly UInt128 NetworkBits;

    public AddressAllocation Allocation => Allocations.GetMatch(this).Value;

    // BUG: Some addresses don't have broadcast addresses
    public IPAddress Broadcast => UInt128ToIpAddress(NetworkBits ^ HostMaskBits);
    public NetworkClass Class => Classes.GetMatch(this).Value;

    /// <summary>
    ///     Returns the complementary network.
    ///     A complementary network is the network that has the same mask and same address, except the least significant bit of
    ///     the address is flipped.
    ///     For example the complementary network of 10.1.2.0/25 is 10.1.2.128/25 and vice versa
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the prefix length is 0</exception>
    public Network ComplementaryNetwork
    {
        get
        {
            if (Prefix == 0)
                throw new ArgumentException($"{this} has no complementary network");

            // XOR the last bit in the network
            var complementaryBits = NetworkBits ^ (UInt128.One << (AddressLength - Prefix));

            // convert the bits to an IP address
            var complementaryAddress = UInt128ToIpAddress(complementaryBits);

            // construct a new network
            var complementaryNetwork = new Network(complementaryAddress, Prefix);

            return complementaryNetwork;
        }
    }

    /// <summary>
    ///     Returns the first host address
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Throws when the prefix is zero</exception>
    public IPAddress FirstHost
    {
        get
        {
            // special cases
            // /0's are network addresses, they have no host addresses
            // /31's and /127 have not network or broadcast address and are all host addresses
            // /32's and /128's are host addresses. Host addresses are the first and last address
            if (Prefix == 0)
                throw new ArgumentOutOfRangeException();

            if (Prefix >= AddressLength - 1)
                return Address;

            return UInt128ToIpAddress(NetworkBits + 1);
        }
    }

    /// <summary>
    ///     Returns the last host address
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Throws when the prefix is zero</exception>
    public IPAddress LastHost
    {
        get
        {
            // special cases
            // /0's are network addresses, they have no host addresses
            // /31's and /127 have not network or broadcast address and are all host addresses
            // /32's and /128's are host addresses. Host addresses are the first and last address
            if (Prefix == 0)
                throw new ArgumentOutOfRangeException();

            if (Prefix == AddressLength)
                return Address;

            if (Prefix == AddressLength - 1)
                return UInt128ToIpAddress(NetworkBits ^ HostMaskBits);

            return UInt128ToIpAddress(NetworkBits ^ (HostMaskBits - 1)); // BUG: is this correct?
        }
    }

    /// <summary>
    ///     The network mask
    /// </summary>
    public IPAddress Mask
    {
        get
        {
            return Version switch
            {
                IPVersion.IPv4 => Ipv4NetworkMaskObjects[Prefix],
                IPVersion.IPv6 => Ipv6NetworkMaskObjects[Prefix],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /// <summary>
    ///     The total count of addresses in the network
    /// </summary>
    public BigInteger TotalAddresses
    {
        get
        {
            return Version switch
            {
                IPVersion.IPv4 => Ipv4AddressCount[Prefix],
                IPVersion.IPv6 => Ipv6AddressCount[Prefix],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /// <summary>
    ///     The total number of host address in the network
    /// </summary>
    public UInt128 TotalHosts
    {
        get
        {
            return Version switch
            {
                IPVersion.IPv4 => Ipv4HostCount[Prefix],
                IPVersion.IPv6 => Ipv6HostCount[Prefix],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /// <summary>
    ///     The version of Internet Protocol represented by the network address
    /// </summary>
    public IPVersion Version { get; }

    /// <summary>
    ///     The wildcard mask of the network.
    ///     A wildcard mask is the inverted subnet mask
    /// </summary>
    public IPAddress Wildcard
    {
        get
        {
            return Version switch
            {
                IPVersion.IPv4 => Ipv4HostMaskObjects[Prefix],
                IPVersion.IPv6 => Ipv6HostMaskObjects[Prefix],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    /// <summary>
    ///     The network address
    /// </summary>
    public IPAddress Address { get; init; }

    /// <summary>
    ///     The number of bits that are used to represent the network portion of the address
    /// </summary>
    public int Prefix { get; init; }

    internal UInt128 HostMaskBits { get; }

    internal UInt128 NetworkMaskBits { get; }

    static Network()
    {
        // compute the bit values for all possible masks
        for (var i = 31; i >= 0; i--)
            Ipv4HostMaskBits[i] = (Ipv4HostMaskBits[i + 1] << 1) ^ 1;

        for (var i = 127; i >= 0; i--)
            Ipv6HostMaskBits[i] = (Ipv6HostMaskBits[i + 1] << 1) ^ 1;

        Ipv4NetworkMaskBits[32] = 0xff_ff_ff_ff;
        for (var i = 31; i >= 0; i--)
            Ipv4NetworkMaskBits[i] = (Ipv4NetworkMaskBits[i + 1] << 1) & Ipv4NetworkMaskBits[32];

        Ipv6NetworkMaskBits[128] = new UInt128(0xff_ff_ff_ff_ff_ff_ff_ff, 0xff_ff_ff_ff_ff_ff_ff_ff);
        for (var i = 127; i >= 0; i--)
            Ipv6NetworkMaskBits[i] = (Ipv6NetworkMaskBits[i + 1] << 1) & Ipv6NetworkMaskBits[128];

        // compute the IP address objects for all possible masks
        for (var i = 0; i < 33; i++)
        {
            var addressBytes =
                ((BigInteger)Ipv4HostMaskBits[i])
                .ToByteArray()
                .Concat(Enumerable.Repeat<byte>(0, 4))
                .Take(4)
                .Reverse()
                .ToArray();

            Ipv4HostMaskObjects[i] = new IPAddress(addressBytes);
        }

        for (var i = 0; i < 129; i++)
        {
            var addressBytes =
                ((BigInteger)Ipv6HostMaskBits[i])
                .ToByteArray()
                .Concat(Enumerable.Repeat<byte>(0, 16))
                .Take(16)
                .Reverse()
                .ToArray();

            Ipv6HostMaskObjects[i] = new IPAddress(addressBytes);
        }

        for (var i = 0; i < 33; i++)
        {
            var addressBytes =
                ((BigInteger)Ipv4NetworkMaskBits[i])
                .ToByteArray()
                .Concat(Enumerable.Repeat<byte>(0, 4))
                .Take(4)
                .Reverse()
                .ToArray();

            Ipv4NetworkMaskObjects[i] = new IPAddress(addressBytes);
        }

        for (var i = 0; i < 129; i++)
        {
            var addressBytes =
                ((BigInteger)Ipv6NetworkMaskBits[i])
                .ToByteArray()
                .Concat(Enumerable.Repeat<byte>(0, 16))
                .Take(16)
                .Reverse()
                .ToArray();

            Ipv6NetworkMaskObjects[i] = new IPAddress(addressBytes);
        }

        // generate a dictionary of the prefix length for each possible prefix
        for (var i = 0; i < 32; i++)
            IpHostMaskToPrefix.Add(Ipv4HostMaskObjects[i], i);

        for (var i = 0; i < 128; i++)
            IpHostMaskToPrefix.Add(Ipv6HostMaskObjects[i], i);

        for (var i = 0; i < 33; i++)
            IpNetworkMaskToPrefix.Add(Ipv4NetworkMaskObjects[i], i);

        for (var i = 0; i < 129; i++)
            IpNetworkMaskToPrefix.Add(Ipv6NetworkMaskObjects[i], i);

        // calculate the total hosts per network
        UInt128 count = 0xff_ff_ff_ff;
        for (var i = 0; i < 31; i++)
        {
            count = count >> 1;
            Ipv4HostCount[i] = count << 1;
        }

        Ipv4HostCount[31] = 2;
        Ipv4HostCount[32] = 1;

        count = new UInt128(0xff_ff_ff_ff_ff_ff_ff_ff, 0xff_ff_ff_ff_ff_ff_ff_ff);
        for (var i = 0; i < 127; i++)
        {
            count = count >> 1;
            Ipv6HostCount[i] = count << 1;
        }

        Ipv6HostCount[127] = 2;
        Ipv6HostCount[128] = 1;

        // calculate the total addresses per network
        for (var i = 0; i < 32; i++)
            Ipv4AddressCount[i] = BigInteger.One << (32 - i);

        Ipv4AddressCount[32] = 1;


        for (var i = 0; i < 128; i++)
            Ipv6AddressCount[i] = BigInteger.One << (128 - i);

        Ipv6AddressCount[128] = 1;

        // populate the allocation lookup
        Allocations = new NetworkLookup<AddressAllocation>
        {
            { "::/0", AddressAllocation.Undefined },
            { "0.0.0.0/0", AddressAllocation.Undefined },

            { "10.0.0.0/8", AddressAllocation.Private },
            { "172.16.0.0/12", AddressAllocation.Private },
            { "192.168.0.0/16", AddressAllocation.Private },

            { "0.0.0.0/5", AddressAllocation.Global },
            { "8.0.0.0/7", AddressAllocation.Global },
            { "11.0.0.0/8", AddressAllocation.Global },
            { "12.0.0.0/6", AddressAllocation.Global },
            { "16.0.0.0/4", AddressAllocation.Global },
            { "32.0.0.0/3", AddressAllocation.Global },
            { "64.0.0.0/2", AddressAllocation.Global },
            { "128.0.0.0/3", AddressAllocation.Global },
            { "160.0.0.0/5", AddressAllocation.Global },
            { "168.0.0.0/6", AddressAllocation.Global },
            { "172.0.0.0/12", AddressAllocation.Global },
            { "172.32.0.0/11", AddressAllocation.Global },
            { "172.64.0.0/10", AddressAllocation.Global },
            { "172.128.0.0/9", AddressAllocation.Global },
            { "173.0.0.0/8", AddressAllocation.Global },
            { "174.0.0.0/7", AddressAllocation.Global },
            { "176.0.0.0/4", AddressAllocation.Global },
            { "192.0.0.0/9", AddressAllocation.Global },
            { "192.128.0.0/11", AddressAllocation.Global },
            { "192.160.0.0/13", AddressAllocation.Global },
            { "192.169.0.0/16", AddressAllocation.Global },
            { "192.170.0.0/15", AddressAllocation.Global },
            { "192.172.0.0/14", AddressAllocation.Global },
            { "192.176.0.0/12", AddressAllocation.Global },
            { "192.192.0.0/10", AddressAllocation.Global },
            { "193.0.0.0/8", AddressAllocation.Global },
            { "194.0.0.0/7", AddressAllocation.Global },
            { "196.0.0.0/6", AddressAllocation.Global },
            { "200.0.0.0/5", AddressAllocation.Global },
            { "208.0.0.0/4", AddressAllocation.Global },

            { "169.254.0.0/16", AddressAllocation.AutomaticPrivateIPAddressing },
            { "127.0.0.0/8", AddressAllocation.Loopback },
            { "0.0.0.0/8", AddressAllocation.CurrentNetwork },
            { "240.0.0.0/4", AddressAllocation.Experimental },
            { "224.0.0.0/4", AddressAllocation.Multicast },
            { "255.255.255.255/32", AddressAllocation.Broadcast }
        };

        // populate the class lookup
        Classes = new NetworkLookup<NetworkClass>
        {
            { "::/0", NetworkClass.Undefined },
            { "0.0.0.0/0", NetworkClass.Undefined },
            { "0.0.0.0/1", NetworkClass.A },
            { "128.0.0.0/2", NetworkClass.B },
            { "192.0.0.0/3", NetworkClass.C },
            { "224.0.0.0/4", NetworkClass.D },
            { "240.0.0.0/4", NetworkClass.E }
        };
    }

    public Network(IPAddress ipAddress, int prefix)
    {
        if (ipAddress.AddressFamily != AddressFamily.InterNetwork && ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
            throw new NotSupportedException($"Non-IPv4 or IPv6 addresses are not supported. The specified address family is {ipAddress.AddressFamily}");

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork && prefix > 32)
            throw new ArgumentOutOfRangeException(nameof(prefix), "Maximum mask length of a IPv4 address is 32");

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6 && prefix > 128)
            throw new ArgumentOutOfRangeException(nameof(prefix), "Maximum mask length of a IPv6 address is 128");

        Address = ipAddress;
        Prefix = prefix;

        Version = ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => IPVersion.IPv4,
            AddressFamily.InterNetworkV6 => IPVersion.IPv6,
            _ => throw new ArgumentOutOfRangeException()
        };


        NetworkMaskBits = Version switch
        {
            IPVersion.IPv4 => Ipv4NetworkMaskBits[Prefix],
            IPVersion.IPv6 => Ipv6NetworkMaskBits[Prefix],
            _ => throw new ArgumentOutOfRangeException()
        };

        HostMaskBits = Version switch
        {
            IPVersion.IPv4 => Ipv4HostMaskBits[Prefix],
            IPVersion.IPv6 => Ipv6HostMaskBits[Prefix],
            _ => throw new ArgumentOutOfRangeException()
        };

        // truncate the network address
        // for example 10.1.2.3/8 becomes 10.0.0.0/8
        NetworkBits = ipAddress.ToUInt128() & NetworkMaskBits;
        Address = UInt128ToIpAddress(NetworkBits);

        AddressLength = Version switch
        {
            IPVersion.IPv4 => 32,
            IPVersion.IPv6 => 128,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Network(IPAddress ipAddress) : this(ipAddress, GetHostAddressPrefix(ipAddress))
    {
    }


    public Network(IPAddress ipAddress, IPAddress mask) : this(ipAddress, GetPrefix(mask))
    {
    }

    public IEnumerable<IPAddress> Addresses()
    {
        var startAddress = NetworkBits;
        var endAddress = NetworkBits ^ HostMaskBits;

        for (var i = startAddress; i <= endAddress; i++)
            yield return UInt128ToIpAddress(i);
    }

    public bool ContainedByOrEqual(Network network)
    {
        return network.ContainsOrEqual(this);
    }

    public bool Contains(IPAddress address)
    {
        var hostNetwork = new Network(address);

        return Contains(hostNetwork);
    }

    /// <summary>
    ///     Determines if the
    ///     <param name="network"></param>
    ///     is contained within this network
    /// </summary>
    /// <param name="network"></param>
    /// <returns></returns>
    public bool Contains(Network network)
    {
        // BUG: should we throw an exception for this? hmm...
        if (Version != network.Version)
            return false;

        // a small network can't contain a bigger one...
        if (Prefix >= network.Prefix)
            return false;

        // the smaller network must have the same network prefix as the larger network

        // AND the network mask of this address to the network bits of the other network
        // if the network is contained by this network then the resulting network bits should be the same as this networks bits.
        return (network.NetworkBits & NetworkMaskBits) == NetworkBits;
    }

    public bool ContainsBy(Network network)
    {
        return network.Contains(this);
    }

    /// <summary>
    ///     Determines if the <paramref name="network"></paramref> is contained within this network or equal to it
    /// </summary>
    /// <param name="network"></param>
    /// <returns></returns>
    public bool ContainsOrEqual(Network network)
    {
        // BUG: should we throw an exception for this? hmm...
        if (Version != network.Version)
            return false;

        // a small network can't contain a bigger one...
        if (Prefix > network.Prefix)
            return false;

        // the smaller network must have the same network prefix as the larger network

        // AND the network mask of this address to the network bits of the other network
        // if the network is contained by this network then the resulting network bits should be the same as this networks bits.
        return (network.NetworkBits & NetworkMaskBits) == NetworkBits;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (GetType() != obj.GetType())
            return false;

        var network = (Network)obj;
        return network.Address.Equals(Address) && network.Prefix == Prefix;
    }


    /// <summary>
    ///     Returns a new network that covers both this network and the <paramref name="networkrk" />
    /// </summary>
    /// <param name="network"></param>
    /// <returns></returns>
    public Network GetContainingNetwork(Network network)
    {
        return GetContainingNetwork(this, network);
    }


    /// <summary>
    ///     Computes the smallest network that contains both networks
    /// </summary>
    /// <param name="network1"></param>
    /// <param name="network2"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if the networks are not from the same address family</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Network GetContainingNetwork(Network network1, Network network2)
    {
        // ensure the network types are the same
        if (network1.Version != network2.Version)
            throw new ArgumentException("Networks must be from the same address family");

        // special case: if either network has a prefix of 0, then the only network that can contain it is 0.0.0.0/0
        if (network1.Prefix == 0 || network2.Prefix == 0)
            return new Network(IPAddress.None, 0);

        // figure out the most specific prefix that includes both networks

        // first XOR the two networks together
        // the parts that are different will be set to '1', for example
        // 10.20.30.1 00001010.00010100.00011110.00000001
        // 10.20.30.2 00001010.00010100.00011110.00000010
        //            00000000.00000000.00000000.00000011
        var mostSignificantBit = network1.NetworkBits ^ network2.NetworkBits;

        // if there are no significant bits then the network is completely contained by the network with the smaller prefix
        if (mostSignificantBit == 0)
            return network1.Prefix < network2.Prefix ? network1 : network2;

        // in the above example the split needs to occur at prefix 31
        // which is the most specific set bit
        // the position of the set bit dictates the new prefix

        // start checking for set bits at the larger of the smaller of the two networks prefix
        var bitPosition = 0; //network1.Prefix > network2.Prefix ? network1.Prefix : network2.Prefix;
        while (mostSignificantBit >> bitPosition > UInt128.One)
            bitPosition++;

        var newPrefix = network2.AddressLength - bitPosition - 1;

        return new Network(network1.Address, newPrefix);
    }

    /// <summary>
    ///     Computes the smallest network that contains all of the networks. The resulting network may contain additional
    ///     address space.
    /// </summary>
    /// <param name="ipNetworks">List of networks</param>
    /// <returns>The network which contains all of the supplied networks.</returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static Network GetContainingNetwork(IEnumerable<Network> ipNetworks)
    {
        Network? containingNetwork = null;
        foreach (var ipNetwork in ipNetworks)
        {
            if (containingNetwork == null)
            {
                containingNetwork = ipNetwork;
                continue;
            }

            containingNetwork = GetContainingNetwork(containingNetwork, ipNetwork);
        }

        if (containingNetwork == null)
            throw new ArgumentNullException();

        return containingNetwork;
    }


    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Prefix);
        hashCode.AddBytes(Address.GetAddressBytes());

        return hashCode.ToHashCode();
    }

    /// <summary>
    ///     Returns an enumerator for all host addresses
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IPAddress> Hosts()
    {
        UInt128 startAddress;
        UInt128 endAddress;

        if (Prefix == AddressLength) // /32 and /128 are host addresses
        {
            startAddress = NetworkBits;
            endAddress = NetworkBits;
        }
        else if (Prefix == AddressLength - 1) // /31 and /127 are point to point addresses
        {
            startAddress = NetworkBits;
            endAddress = NetworkBits + UInt128.One;
        }
        else
        {
            startAddress = NetworkBits + 1;
            endAddress = NetworkBits ^ (HostMaskBits - 1); // BUG: is this correct?
        }

        for (var i = startAddress; i <= endAddress; i++)
            yield return UInt128ToIpAddress(i);
    }

    /// <summary>
    ///     XXXXXXXXXXXXXXXXXXXXXXXXX Fix this... the
    ///     <param name="valueToAdd" />
    ///     to the
    /// </summary>
    /// <param name="network">The network to add to</param>
    /// <param name="valueToAdd">The number of networks to add</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when it is not possible to add the <paramref name="valueToAdd" />
    ///     to the <paramref name="network" />
    /// </exception>
    public static Network operator +(Network network, UInt128 valueToAdd)
    {
        try
        {
            checked
            {
                var addressesPerNetwork = UInt128.One << (network.AddressLength - network.Prefix);
                var addressesToAdd = addressesPerNetwork * valueToAdd;
                var networkBits = network.NetworkBits + addressesToAdd;

                if (network.Version == IPVersion.IPv4 && networkBits > Ipv4NetworkMaskBits[network.Prefix])
                    throw new ArgumentOutOfRangeException();

                if (network.Version == IPVersion.IPv6 && networkBits > Ipv6NetworkMaskBits[network.Prefix])
                    throw new ArgumentOutOfRangeException();

                var ipAddress = networkBits.ToIpAddress(network.Version);
                var newNetwork = new Network(ipAddress, network.Prefix);

                return newNetwork;
            }
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public static bool operator ==(Network? network1, Network? network2)
    {
        if (ReferenceEquals(network1, null) && ReferenceEquals(network2, null))
            return true;

        if (ReferenceEquals(network1, null) || ReferenceEquals(network2, null))
            return false;

        return network1.Address.Equals(network2.Address) && network1.Prefix == network2.Prefix;
    }

    public static bool operator >(Network network1, Network network2)
    {
        // ipv6 is greater then ipv4
        if (network1.Version != network2.Version)
            return network2.Version == IPVersion.IPv4;

        // if both side have the same network bits, then the one with the biggest mask is greater 
        if (network1.NetworkBits == network2.NetworkBits)
            return network1.Prefix > network2.Prefix;

        // otherwise the one with the biggest network is biggest
        return network1.NetworkBits > network2.NetworkBits;
    }

    public static implicit operator Network(string value)
    {
        return Parse(value);
    }

    public static bool operator !=(Network network1, Network network2)
    {
        return !(network1 == network2);
    }

    public static bool operator <(Network network1, Network network2)
    {
        // ipv4 is less then ipv6
        if (network1.Version != network2.Version)
            return network1.Version == IPVersion.IPv4;

        // if both side have the same network bits, then the one with the smallest mask is smaller 
        if (network1.NetworkBits == network2.NetworkBits)
            return network1.Prefix < network2.Prefix;

        // otherwise the one with the smallest network is smallest
        return network1.NetworkBits < network2.NetworkBits;
    }

    public static Network operator -(Network network, UInt128 valueToSubtract)
    {
        try
        {
            checked
            {
                var addressesPerNetwork = UInt128.One << (network.AddressLength - network.Prefix);
                var addressToSubtract = addressesPerNetwork * valueToSubtract;
                var networkBits = network.NetworkBits - addressToSubtract;

                if (network.Version == IPVersion.IPv4 && networkBits > Ipv4NetworkMaskBits[network.Prefix])
                    throw new ArgumentOutOfRangeException();

                if (network.Version == IPVersion.IPv6 && networkBits > Ipv6NetworkMaskBits[network.Prefix])
                    throw new ArgumentOutOfRangeException();

                var ipAddress = networkBits.ToIpAddress(network.Version);
                var newNetwork = new Network(ipAddress, network.Prefix);

                return newNetwork;
            }
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Parses a string to a <c>Network</c> object
    /// </summary>
    /// <param name="ipNetworkString">
    ///     A string that represents a network The following formats are supported:10.0.0.0/24, myhost/24, 10.0.0.10, 10.0.0.0
    ///     255.255.255.0, myhost.mydomain.org 255.255.255.0, myhost
    /// </param>
    /// <returns>The parsed network object</returns>
    public static Network Parse(string ipNetworkString)
    {
        if (TryParse(ipNetworkString, out var network))
            return network;

        throw new FormatException("Address is malformed");
    }


    public List<Network> RemoveNetwork(Network networkToRemove)
    {
        if (networkToRemove.Contains(this))
            throw new ArgumentException("The network to remove contains the network");

        if (this == networkToRemove)
            throw new ArgumentException("The network to remove and the network are the same");

        if (!Contains(networkToRemove))
            throw new ArgumentException("The network to remove is not contained by this network");

        var results = RemoveNetwork(new List<Network>(new[] { this }), networkToRemove);
        return results;
    }

    /// <summary>
    ///     Splits this Network into two equal Network objects.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Insufficient number network bits to perform the split</exception>
    public IEnumerable<Network> Split()
    {
        return Split(Prefix + 1);
    }

    /// <summary>
    ///     Splits this Network into two equal Network objects.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Insufficient number network bits to perform the split</exception>
    public IEnumerable<Network> Split(int prefixLength)
    {
        if (prefixLength >= AddressLength)
            throw new InvalidOperationException($"The split target for {this} of {Address}/{prefixLength} is not valid");

        var sizeOfNewNetworks = UInt128.One << (AddressLength - prefixLength);
        var numberOfNewNetworks = UInt128.One << (prefixLength - Prefix);

        var currentNetworkBits = NetworkBits;
        for (UInt128 i = 0; i < numberOfNewNetworks; i++)
        {
            var networkAddress = UInt128ToIpAddress(currentNetworkBits);
            var network = new Network(networkAddress, prefixLength);

            yield return network;

            currentNetworkBits += sizeOfNewNetworks;
        }
    }

    /// <summary>
    ///     Summarizes a list of networks such that the returned networks represents a compact and equivalent representation of
    ///     the original networks.
    /// </summary>
    /// <param name="networks">List of networks to summarize</param>
    /// <returns>A summarized, compact, and equivalent form of the original networks</returns>
    public static IEnumerable<Network> Summarize(IEnumerable<Network> networks)
    {
        // create a lookup dictionary by prefix
        var networksByPrefix = new Dictionary<int, Dictionary<IPAddress, Network>>();

        // create a new, empty, bucket for each possible prefix
        for (var i = 0; i <= 128; i++)
            networksByPrefix.Add(i, new Dictionary<IPAddress, Network>());

        // add each network to the lookup by prefix
        foreach (var network in networks)
            networksByPrefix[network.Prefix].Add(network.Address, network);

        // go through each possible prefix, starting with the largest prefix
        for (var prefixLength = 128; prefixLength > 0; prefixLength--)

            // iterate over each network at a given prefix
            foreach (var network in networksByPrefix[prefixLength].ToArray())
            {
                // network could have been previously removed
                // if the network no longer exists, skip this network
                if (!networksByPrefix[prefixLength].Contains(network))
                    continue;

                // check to see if the complementary network exists
                // if the complementary network does not exist, then this network cannot be summarized
                var complementaryNetwork = network.Value.ComplementaryNetwork;
                if (!networksByPrefix[prefixLength].TryGetValue(complementaryNetwork.Address, out var complementaryX))
                    continue;

                // both halves of the less specific network exist
                // merge the two more specific halves into the more specific version
                // by replacing the two more specific networks with a new, less specific network
                // BUG: this assumes that there is no overlap...
                var summarizedNetwork = new Network(network.Key, prefixLength - 1);

                // BUG: Index out of bounds?
                // add the new summarized network
                networksByPrefix[prefixLength - 1].Add(summarizedNetwork.Address, summarizedNetwork);

                // remove the two more specific haves of the summarized network that are now covered by the new summarized network
                networksByPrefix[prefixLength].Remove(network.Key);
                networksByPrefix[prefixLength].Remove(complementaryNetwork.Address);
            }

        // return each network in the network lookup table
        foreach (var ipNetworks in networksByPrefix.Values)
        foreach (var ipNetwork in ipNetworks)
            yield return ipNetwork.Value;
    }

    public override string ToString()
    {
        return ToString(NetworkFormat.AddressAndPrefix);
    }

    public string ToString(NetworkFormat format)
    {
        switch (format)
        {
            case NetworkFormat.AddressAndPrefix:
                return $"{Address}/{Prefix}";

            case NetworkFormat.AddressAndMask:
                return $"{Address} {Mask}";

            case NetworkFormat.Mask:
                return $"{Mask}";

            case NetworkFormat.Address:
                return $"{Address}";

            case NetworkFormat.Detail:
            {
                var sb = new StringBuilder();

                var networkProperty = new List<(string Field, string Value, string Bits)>();
                networkProperty.Add(new ValueTuple<string, string, string>("Address:", Address.ToString(), BitsToString(NetworkBits)));
                networkProperty.Add(new ValueTuple<string, string, string>("Mask:", Mask.ToString(), BitsToString(NetworkMaskBits)));
                networkProperty.Add(new ValueTuple<string, string, string>("Wildcard:", Wildcard.ToString(), BitsToString(Wildcard)));
                networkProperty.Add(new ValueTuple<string, string, string>("Broadcast:", Broadcast.ToString(), BitsToString(Broadcast)));

                if (Prefix > 0)
                {
                    networkProperty.Add(new ValueTuple<string, string, string>("First Host:", FirstHost.ToString(), BitsToString(FirstHost)));
                    networkProperty.Add(new ValueTuple<string, string, string>("Last Host:", LastHost.ToString(), BitsToString(LastHost)));
                }

                networkProperty.Add(new ValueTuple<string, string, string>("Hosts/Net:", TotalHosts.ToString("0,0", CultureInfo.InvariantCulture), ""));

                var column = new int[3];
                foreach (var (field, value, bits) in networkProperty)
                {
                    if (field.Length > column[0])
                        column[0] = field.Length;

                    if (value.Length > column[1])
                        column[1] = value.Length;

                    if (bits.Length > column[2])
                        column[2] = bits.Length;
                }

                foreach (var (field, value, bits) in networkProperty)
                    sb.AppendLine($"{field.PadRight(column[0])} {value.PadRight(column[1])}  {bits.PadRight(column[2])}".TrimEnd());

                return sb.ToString();
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    ///     Attempts to parse a string to a <c>Network</c> object
    /// </summary>
    /// <param name="ipNetworkString">
    ///     A string that represents a network. The following formats are supported:10.0.0.0/24, myhost/24, 10.0.0.10, 10.0.0.0
    ///     255.255.255.0, myhost.mydomain.org 255.255.255.0, myhost
    /// </param>
    /// <param name="network"></param>
    /// <returns>True if the network was successfully parsed, otherwise false</returns>
    public static bool TryParse(string ipNetworkString, out Network network)
    {
        network = null!;

        var networkParts = ipNetworkString.Split(' ', '/');

        if (networkParts.Length is 0 or > 3)
            return false;

        if (!IPAddress.TryParse(networkParts[0], out var networkPart))
        {
            // BUG: this can throw if it fails to resolve the name...
            IPAddress[] hostAddresses;

            try
            {
                // Dns.GetHostAddresses will throw on failure and there is no TryGetHostAddresses version
                hostAddresses = Dns.GetHostAddresses(networkParts[0]);
                if (hostAddresses.Length == 0)
                    return false;
            }
            catch
            {
                return false;
            }

            networkPart = hostAddresses[0];
        }

        var prefixLength = 0;
        IPAddress? maskAddress = null;

        switch (networkParts.Length)
        {
            case 1:
                switch (networkPart.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        prefixLength = 32;
                        break;
                    case AddressFamily.InterNetworkV6:
                        prefixLength = 128;
                        break;
                    default:
                        return false;
                }

                break;
            case 2:
                if (!int.TryParse(networkParts[1], out prefixLength))
                    if (!IPAddress.TryParse(networkParts[1], out maskAddress))
                        return false;

                break;
        }

        if (maskAddress != null)
            network = new Network(networkPart, maskAddress);
        else
            network = new Network(networkPart, prefixLength);

        return true;
    }

    // should these helper functions be somewhere else
    private string BitsToString(IPAddress value)
    {
        return BitsToString(value.ToUInt128());
    }

    private string BitsToString(UInt128 value)
    {
        var sb = new StringBuilder();

        for (var i = AddressLength - 1; i >= 0; i--)
        {
            if ((value & (UInt128.One << i)) > UInt128.Zero)
                sb.Append('1');
            else
                sb.Append('0');

            if (Version == IPVersion.IPv4 && i % 8 == 0 && i > 0)
                sb.Append('.');
            else if (Version == IPVersion.IPv6 && i % 16 == 0 && i > 0)
                sb.Append(':'); // BUG: Does this work correctly with IPv6?
        }

        return sb.ToString();
    }

    private static int GetHostAddressPrefix(IPAddress ipAddress)
    {
        return ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => 32,
            AddressFamily.InterNetworkV6 => 128,
            _ => -1
        };
    }

    private static int GetPrefix(IPAddress mask)
    {
        if (IpNetworkMaskToPrefix.TryGetValue(mask, out var maskLength))
            return maskLength;

        throw new ArgumentOutOfRangeException(nameof(mask), $"Invalid subnet mask: {mask}");
    }

    private static List<Network> RemoveNetwork(List<Network> listOfNetworks, Network networkToRemove)
    {
        // TODO: Handle if the networktoremove overlaps a network on the listofnetworks.
        // TODO: could handle it by disallowing overlapping networks
        // BUG: If you try to remove a network that is not contained, but has the same mask, you get a stack overflow (172.16.0.0/16 and 172.1.0.0/16 for example...

        if (listOfNetworks.Contains(networkToRemove))
        {
            listOfNetworks.Remove(networkToRemove);
            return listOfNetworks;
        }

        foreach (var network in listOfNetworks)
        {
            if (!network.Contains(networkToRemove))
                continue;

            listOfNetworks.AddRange(network.Split());
            listOfNetworks.Remove(network);
            break;
        }

        return RemoveNetwork(listOfNetworks, networkToRemove);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IPAddress UInt128ToIpAddress(UInt128 addressBits)
    {
        return addressBits.ToIpAddress(Version);
    }

    public int CompareTo(Network? other)
    {
        if (ReferenceEquals(other, null))
            return -1;

        if (other == this)
            return 0;

        if (other > this)
            return -1;

        return 1;
    }
}