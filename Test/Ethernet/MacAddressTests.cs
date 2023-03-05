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

using Bitvantage.NetworkAddressing.Ethernet;

namespace Test.Ethernet;

internal class MacAddressTests
{
    [Test]
    public void CompareTo_01()
    {
        var unsortedMacAddresses = new List<MacAddress?>
        {
            "20-20-30-40-50-60",
            "10-20-30-40-50-60",
            "10-20-30-40-50-61",
            "00-00-00-00-00-01",
            "00-00-00-00-00-00",
            null,
        };

        var expectedResults = new List<MacAddress?>
        {
            null,
            "00-00-00-00-00-00",
            "00-00-00-00-00-01",
            "10-20-30-40-50-60",
            "10-20-30-40-50-61",
            "20-20-30-40-50-60",

        };

        var orderedMacAddresses =
            unsortedMacAddresses
                .Order()
                .ToList();

        Assert.That(orderedMacAddresses, Is.EquivalentTo(expectedResults));
    }

    [Test]
    public void ExtensionIdentifier_01()
    {
        var macAddress = MacAddress.Parse("de:ad:be:ef:ab:cd");
        Assert.That(macAddress.ExtensionIdentifier, Is.EqualTo(new MacAddress(new byte[] { 0x00, 0x00, 0x00, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void OrganizationalUniqueIdentifier_01()
    {
        var macAddress = MacAddress.Parse("de:ad:be:ef:ab:cd");
        Assert.That(macAddress.OrganizationalUniqueIdentifier, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0x00, 0x00, 0x00 })));
    }


    [Test]
    public void Parse_01()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void Parse_02()
    {
        var macAddress = MacAddress.Parse("dead.beef.abcd");
        Assert.That(macAddress, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void Parse_03()
    {
        var macAddress = MacAddress.Parse("dead:beef:abcd");
        Assert.That(macAddress, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void Parse_04()
    {
        var macAddress = MacAddress.Parse("de-ad-be-ef-ab-cd");
        Assert.That(macAddress, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void Parse_05()
    {
        var macAddress = MacAddress.Parse("de.ad.be.ef.ab.cd");
        Assert.That(macAddress, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void Parse_06()
    {
        var macAddress = MacAddress.Parse("de:ad:be:ef:ab:cd");
        Assert.That(macAddress, Is.EqualTo(new MacAddress(new byte[] { 0xde, 0xad, 0xbe, 0xef, 0xab, 0xcd })));
    }

    [Test]
    public void Parse_07()
    {
        Assert.Throws<ArgumentException>(() => MacAddress.Parse("de ad be ef ab cd"));
    }

    [Test]
    public void ToString_01()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(), Is.EqualTo("DE-AD-BE-EF-AB-CD"));
    }

    [Test]
    public void ToString_02()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.Ieee), Is.EqualTo("DE-AD-BE-EF-AB-CD"));
    }

    [Test]
    public void ToString_03()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.Ietf), Is.EqualTo("de:ad:be:ef:ab:cd"));
    }

    [Test]
    public void ToString_04()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.DoubleColon), Is.EqualTo("de:ad:be:ef:ab:cd"));
    }

    [Test]
    public void ToString_05()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.DoubleDash), Is.EqualTo("de-ad-be-ef-ab-cd"));
    }

    [Test]
    public void ToString_06()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.DoubleDot), Is.EqualTo("de.ad.be.ef.ab.cd"));
    }

    [Test]
    public void ToString_07()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.QuadColon), Is.EqualTo("dead:beef:abcd"));
    }

    [Test]
    public void ToString_08()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(MacAddress.MacAddressFormat.QuadDot), Is.EqualTo("dead.beef.abcd"));
    }

    [Test]
    public void ToString_09()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString("XX:XX:XX:XX:XX:XX"), Is.EqualTo("DE:AD:BE:EF:AB:CD"));
    }

    [Test]
    public void ToString_10()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString("xx.xx.xx.xx.xx.xx"), Is.EqualTo("de.ad.be.ef.ab.cd"));
    }

    [Test]
    public void ToString_11()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString("xxxx.xxxx.xxxx"), Is.EqualTo("dead.beef.abcd"));
    }

    [Test]
    public void ToString_12()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString("xx"), Is.EqualTo("de"));
    }

    [Test]
    public void ToString_13()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString("xxxx.xxxx.xxxx.xxxx"), Is.EqualTo("dead.beef.abcd.xxxx"));
    }

    [Test]
    public void ToString_14()
    {
        var macAddress = MacAddress.Parse("deadbeefabcd");
        Assert.That(macAddress.ToString(""), Is.EqualTo(""));
    }
}

