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

using System.Diagnostics;
using Bitvantage.NetworkAddressing.Ethernet;

namespace Test.Ethernet;

internal class OuiDatabaseTester
{
    [Test]
    public void Index_01()
    {
        var ouiDatabase = new OuiDatabase();

        var ouiRecord = ouiDatabase["00:22:72:01:02:03"];

        Assert.That(ouiRecord.Organization, Is.EqualTo("American Micro-Fuel Device Corp."));
    }

    [Test]
    public void Count_01()
    {
        var ouiDatabase = new OuiDatabase();

        Assert.That(ouiDatabase.Count, Is.GreaterThanOrEqualTo(33092));
    }

    [Test]
    public void TryGetValue_01()
    {
        var ouiDatabase = new OuiDatabase();

        var success = ouiDatabase.TryGetValue("00:22:72:01:02:03", out var ouiRecord);

        Assert.That(ouiRecord.Organization, Is.EqualTo("American Micro-Fuel Device Corp."));
        Assert.That(success, Is.EqualTo(true));
    }

    [Test]
    public void Keys_01()
    {
        var ouiDatabase = new OuiDatabase();

        var keys =
            ouiDatabase
                .Keys
                .ToList();

        Assert.That(keys.Contains("00:22:72:00:00:00"), Is.EqualTo(true));
        Assert.That(keys.Contains("D8:EF:42:00:00:00"), Is.EqualTo(true));
        Assert.That(keys.Contains("1C:30:08:00:00:00"), Is.EqualTo(true));
    }

    [Test]
    public void Values_01()
    {
        var ouiDatabase = new OuiDatabase();

        var values =
            ouiDatabase
                .Values
                .ToList();

        Assert.That(values.Exists(item => item.Prefix == "002272000000"), Is.EqualTo(true));
        Assert.That(values.Exists(item => item.Prefix == "D8EF42000000"), Is.EqualTo(true));
        Assert.That(values.Exists(item => item.Prefix == "1C3008000000"), Is.EqualTo(true));
    }

    [Test]
    public void OuiRecord_01()
    {
        var ouiDatabase = new OuiDatabase();

        var ouiRecord = ouiDatabase["00:22:72:01:02:03"];

        Assert.That(ouiRecord.Organization, Is.EqualTo("American Micro-Fuel Device Corp."));
        Assert.That(ouiRecord.Address, Is.EqualTo("2181 Buchanan Loop\nFerndale  WA  98248\nUS"));
        Assert.That(ouiRecord.Prefix.ToString(), Is.EqualTo("00-22-72-00-00-00"));
    }

    [Test]
    public void OuiRecord_02()
    {
        var ouiDatabase = new OuiDatabase();

        var ouiRecord = ouiDatabase["64:D1:A3:01:02:03"];

        Assert.That(ouiRecord.Organization, Is.EqualTo("Sitecom Europe BV"));
        Assert.That(ouiRecord.Address, Is.EqualTo("Linatebaan 101\nRotterdam  Zuid Holland  3045 AH\nNL"));
        Assert.That(ouiRecord.Prefix.ToString(), Is.EqualTo("64-D1-A3-00-00-00"));
    }

    [Test]
    public void OuiRecord_03()
    {
        var options = new OuiDatabaseOptions { AutomaticUpdates = false, CacheDirectory = null };
        //var options = new OuiParserOptions { SynchronousUpdate = false, ThrowOnUpdateFailure = false };
        options.DatabaseEvent += (sender, args) => Debug.WriteLine($"{args.DateTime} {args.Level} {args.EventId} {args.Message}");

        var ouiDatabase = new OuiDatabase(options);

        var z = ouiDatabase["1C-1B-0D-EF-9F-2B"];
        var z2 = ouiDatabase[new byte[] { 0x1C, 0x1B, 0x0D, 0xEF, 0x9F, 0x2B }];
    }
}