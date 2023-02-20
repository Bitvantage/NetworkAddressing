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
using System.Runtime.CompilerServices;

namespace Bitvantage.NetworkAddressing.InternetProtocol
{
    internal static class IPAddressExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static UInt128 ToUInt128(this IPAddress address)
        {
            var addressBytes = address.GetAddressBytes();

            if (addressBytes.Length == 4)
                return new UInt128(0, BitConverter.ToUInt64(new[] { addressBytes[3], addressBytes[2], addressBytes[1], addressBytes[0], (byte)0, (byte)0, (byte)0, (byte)0 }));

            if (addressBytes.Length == 16)
                return new UInt128(BitConverter.ToUInt64(new[] { addressBytes[7], addressBytes[6], addressBytes[5], addressBytes[4], addressBytes[3], addressBytes[2], addressBytes[1], addressBytes[0] }), BitConverter.ToUInt64(new[] { addressBytes[15], addressBytes[14], addressBytes[13], addressBytes[12], addressBytes[11], addressBytes[10], addressBytes[9], addressBytes[8] }));

            throw new ArgumentOutOfRangeException(nameof(address), $"IP address must be either 32 bits or 128 bits long. Specified address '{address}' is {addressBytes.Length * 8} bytes long");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IPAddress ToIpAddress(this UInt128 addressBits, IPVersion version)
        {
            var addressBytes = version switch
            {
                IPVersion.IPv4 => new byte[4],
                IPVersion.IPv6 => new byte[16],
                _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
            };

            Unsafe.As<byte, UInt128>(ref addressBytes[0]) = addressBits;
            Array.Reverse(addressBytes);

            return new IPAddress(addressBytes);
        }

    }
}
