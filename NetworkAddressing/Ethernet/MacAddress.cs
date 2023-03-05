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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Bitvantage.NetworkAddressing.Ethernet.Converters;

namespace Bitvantage.NetworkAddressing.Ethernet;

[Serializable]
[JsonConverter(typeof(MacAddressJsonConverter))]
public class MacAddress : IComparable<MacAddress>, IXmlSerializable
{
    public enum MacAddressFormat
    {
        /// <summary>
        ///     01-02-03-AB-CD-EF
        /// </summary>
        Ieee,

        /// <summary>
        ///     01.02.03.ab.cd.ef
        /// </summary>
        Ietf,

        /// <summary>
        ///     01-02-03-ab-cd-ef
        /// </summary>
        DoubleDash,

        /// <summary>
        ///     01:02:03:ab:cd:ef
        /// </summary>
        DoubleColon,

        /// <summary>
        ///     01.02.03.ab.cd.ef
        /// </summary>
        DoubleDot,

        /// <summary>
        ///     0102:03ab:cdef
        /// </summary>
        QuadColon,

        /// <summary>
        ///     0102.03ab.cdef
        /// </summary>
        QuadDot,

        /// <summary>
        ///     010203abcdef
        /// </summary>
        Full // TODO: Better name?
    }

    private const long OrganizationalUniqueIdentifierMask = 0x00_00_ff_ff_ff_00_00_00;
    private const long ExtensionIdentifierMask = 0x00_00_00_00_00_ff_ff_ff;

    // Match the following formats:
    // aa:aa:aa:aa:aa:aa
    // aa-aa-aa-aa-aa-aa
    // aa.aa.aa.aa.aa.aa
    // aaaa:aaaa:aaaa
    // aaaa.aaaa.aaaa
    // aaaaaaaaaaaa
    // leading and trailing spaces are ignored
    private static readonly Regex MacAddressRegex = new("""
            # match any number of leading white spaces
            (?>^\s*)
            (?>
                # aa:aa:aa:aa:aa:aa
                (?<octet1>[0-9a-fA-F]{2}):(?<octet2>[0-9a-fA-F]{2}):(?<octet3>[0-9a-fA-F]{2}):(?<octet4>[0-9a-fA-F]{2}):(?<octet5>[0-9a-fA-F]{2}):(?<octet6>[0-9a-fA-F]{2})|
                # aa-aa-aa-aa-aa-aa
                (?<octet1>[0-9a-fA-F]{2})-(?<octet2>[0-9a-fA-F]{2})-(?<octet3>[0-9a-fA-F]{2})-(?<octet4>[0-9a-fA-F]{2})-(?<octet5>[0-9a-fA-F]{2})-(?<octet6>[0-9a-fA-F]{2})|
                # aa.aa.aa.aa.aa.aa
                (?<octet1>[0-9a-fA-F]{2})\.(?<octet2>[0-9a-fA-F]{2})\.(?<octet3>[0-9a-fA-F]{2})\.(?<octet4>[0-9a-fA-F]{2})\.(?<octet5>[0-9a-fA-F]{2})\.(?<octet6>[0-9a-fA-F]{2})|
                # aaaa:aaaa:aaaa
                (?<octet1>[0-9a-fA-F]{2})(?<octet2>[0-9a-fA-F]{2}):(?<octet3>[0-9a-fA-F]{2})(?<octet4>[0-9a-fA-F]{2}):(?<octet5>[0-9a-fA-F]{2})(?<octet6>[0-9a-fA-F]{2})|
                # aaaa.aaaa.aaaa.aaaa
                (?<octet1>[0-9a-fA-F]{2})(?<octet2>[0-9a-fA-F]{2}).(?<octet3>[0-9a-fA-F]{2})(?<octet4>[0-9a-fA-F]{2}).(?<octet5>[0-9a-fA-F]{2})(?<octet6>[0-9a-fA-F]{2})|
                # aaaaaaaaaaaa
                (?<octet1>[0-9a-fA-F]{2})(?<octet2>[0-9a-fA-F]{2})(?<octet3>[0-9a-fA-F]{2})(?<octet4>[0-9a-fA-F]{2})(?<octet5>[0-9a-fA-F]{2})(?<octet6>[0-9a-fA-F]{2})
            )
            # match any number of trailing white spaces
            (?>\s*$)
            """, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);

    internal ulong MacAddressBits;

    /// <summary>
    ///     Returns a new MAC address using the last three octets of the MAC address
    /// </summary>
    public MacAddress ExtensionIdentifier => new(MacAddressBits & ExtensionIdentifierMask);

    public bool IsBroadcast =>
        // a broadcast address is an address with all bits set to '1'
        MacAddressBits == 0xFF_FF_FF_FF_FF_FF;

    public bool IsInvalid => MacAddressBits == 0; // zero is not a valid mac address

    public bool IsLocallyAdministered
    {
        // second least-significant bit of the first octet is set to one for locally administered addresses
        // the broadcast address is not locally administered

        get
        {
            if (IsBroadcast)
                return false;

            return (MacAddressBits & ((ulong)1 << 41)) == (ulong)1 << 41;
        }
    }

    public bool IsMulticast
    {
        // a multicast address is an address with a '1' in the least-significant bit of the first octet
        // the broadcast address is not a multicast address

        get
        {
            if (IsBroadcast)
                return false;

            return (MacAddressBits & ((ulong)1 << 40)) == (ulong)1 << 40;
        }
    }

    /// <summary>
    ///     Returns a new MAC address using the first three octets of the MAC address
    /// </summary>
    public MacAddress OrganizationalUniqueIdentifier => new(MacAddressBits & OrganizationalUniqueIdentifierMask);

    private MacAddress()
    {
    }

    private MacAddress(MacAddress macAddress)
    {
        MacAddressBits = macAddress.MacAddressBits;
    }

    public MacAddress(byte octet1, byte octet2, byte octet3, byte octet4, byte octet5, byte octet6)
    {
        MacAddressBits = BitConverter.ToUInt64(new[] { octet6, octet5, octet4, octet3, octet2, octet1, (byte)0x00, (byte)0x00 }, 0);
    }

    public MacAddress(params byte[] octets)
    {
        if (octets.Length != 6)
            throw new ArgumentException($"Array size must be exactly 6 bytes long, array size is {octets.Length} bytes", nameof(octets));

        MacAddressBits = BitConverter.ToUInt64(new[] { octets[5], octets[4], octets[3], octets[2], octets[1], octets[0], (byte)0x00, (byte)0x00 }, 0);
    }

    public MacAddress(ulong value)
    {
        MacAddressBits = value;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (GetType() != obj.GetType())
            return false;

        var mac = (MacAddress)obj;

        return mac.MacAddressBits == MacAddressBits;
    }

    public override int GetHashCode()
    {
        return MacAddressBits.GetHashCode();
    }

    public static bool operator ==(MacAddress? mac1, MacAddress? mac2)
    {
        if (ReferenceEquals(mac1, null) && ReferenceEquals(mac2, null))
            return true;

        if (ReferenceEquals(mac1, null) || ReferenceEquals(mac2, null))
            return false;

        return mac1.MacAddressBits == mac2.MacAddressBits;
    }

    public static bool operator >(MacAddress mac1, MacAddress mac2)
    {
        return mac1.MacAddressBits > mac2.MacAddressBits;
    }

    public static bool operator >=(MacAddress mac1, MacAddress mac2)
    {
        return mac1.MacAddressBits >= mac2.MacAddressBits;
    }

    public static implicit operator MacAddress(string value)
    {
        return Parse(value);
    }

    public static implicit operator MacAddress(byte[] value)
    {
        return new MacAddress(value);
    }

    public static bool operator !=(MacAddress mac1, MacAddress mac2)
    {
        return !(mac1 == mac2);
    }

    public static bool operator <(MacAddress mac1, MacAddress mac2)
    {
        return mac1.MacAddressBits < mac2.MacAddressBits;
    }

    public static bool operator <=(MacAddress mac1, MacAddress mac2)
    {
        return mac1.MacAddressBits <= mac2.MacAddressBits;
    }

    public static MacAddress Parse(string macAddress)
    {
        return new MacAddress(Parse(macAddress, false).Value);
    }

    public override string ToString()
    {
        return ToString(MacAddressFormat.Ieee);
    }

    /// <summary>
    ///     Returns a string, replacing each occurrence of 'x' or 'X' with the respective part of the MAC address
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public string ToString(string format)
    {
        var hexValues =
            BitConverter
                .GetBytes(MacAddressBits)
                .Take(6)
                .Reverse()
                .Select(item => item.ToString("x"));

        var hexValue = string.Join(null, hexValues);

        var results = new StringBuilder();
        var currentIndex = 0;

        foreach (var formatChar in format)
            if (formatChar is 'x' or 'X' && currentIndex < hexValue.Length)
                if (formatChar is 'x')
                    results.Append(hexValue[currentIndex++]);
                else
                    results.Append(hexValue[currentIndex++].ToString().ToUpper());
            else
                results.Append(formatChar);

        return results.ToString();
    }

    public string ToString(MacAddressFormat macAddressFormat)
    {
        string separator;
        int width;
        var upperCase = false;
        switch (macAddressFormat)
        {
            case MacAddressFormat.Ieee:
                separator = "-";
                width = 1;
                upperCase = true;
                break;

            case MacAddressFormat.Ietf:
                separator = ":";
                width = 1;
                break;

            case MacAddressFormat.DoubleDash:
                separator = "-";
                width = 1;
                break;
            case MacAddressFormat.DoubleColon:
                separator = ":";
                width = 1;
                break;
            case MacAddressFormat.DoubleDot:
                separator = ".";
                width = 1;
                break;
            case MacAddressFormat.QuadColon:
                separator = ":";
                width = 2;
                break;
            case MacAddressFormat.QuadDot:
                separator = ".";
                width = 2;
                break;
            case MacAddressFormat.Full:
                separator = "";
                width = 6;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(macAddressFormat), macAddressFormat, null);
        }

        var byteArray = BitConverter.GetBytes(MacAddressBits);
        var stringBuilder = new StringBuilder();
        for (var index = 5; index >= 0; index--)
        {
            stringBuilder.Append(byteArray[index].ToString("x2"));
            if (index % width == 0 && index > 0)
                stringBuilder.Append(separator);
        }

        if (upperCase)
            return stringBuilder.ToString().ToUpper();

        return stringBuilder.ToString();
    }

    public ulong ToUInt64()
    {
        return MacAddressBits;
    }

    public static bool TryParse(string text, [NotNullWhen(true)] out MacAddress parsedMacAddress)
    {
        // Match the following formats:
        // aa:aa:aa:aa:aa:aa
        // aa-aa-aa-aa-aa-aa
        // aa.aa.aa.aa.aa.aa
        // aaaa:aaaa:aaaa
        // aaaa.aaaa.aaaa
        // aaaaaaaaaaaa
        // leading and trailing spaces are ignored

        var macAddress = Parse(text, true);

        if (macAddress == null)
        {
            parsedMacAddress = null;
            return false;
        }

        parsedMacAddress = new MacAddress(macAddress.Value);
        return true;
    }

    private static ulong? Parse(string macAddress, [DoesNotReturnIf(false)] bool suppressException)
    {
        var macAddressMatch = MacAddressRegex.Match(macAddress);

        if (macAddressMatch.Success)
        {
            var macAddressInt = BitConverter.ToUInt64(
                new byte[]
                {
                    byte.Parse(macAddressMatch.Groups["octet6"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(macAddressMatch.Groups["octet5"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(macAddressMatch.Groups["octet4"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(macAddressMatch.Groups["octet3"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(macAddressMatch.Groups["octet2"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    byte.Parse(macAddressMatch.Groups["octet1"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    0x00,
                    0x00
                }
            );

            return macAddressInt;
        }

        if (suppressException)
            return null;

        throw new ArgumentException("Unsupported format", nameof(macAddress));
    }

    public int CompareTo(MacAddress? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (ReferenceEquals(null, other))
            return 1;

        return MacAddressBits.CompareTo(other.MacAddressBits);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public XmlSchema? GetSchema()
    {
        return null;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void ReadXml(XmlReader reader)
    {
        reader.MoveToContent();

        if (reader.IsEmptyElement)
            throw new NullReferenceException();

        reader.ReadStartElement();
        var macAddressText = reader.ReadString();
        MacAddressBits = Parse(macAddressText, false).Value;

        reader.ReadEndElement();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public void WriteXml(XmlWriter writer)
    {
        writer.WriteString(ToString());
    }
}