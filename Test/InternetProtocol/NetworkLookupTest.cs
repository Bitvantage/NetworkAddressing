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
using Bitvantage.NetworkAddressing.InternetProtocol;

namespace Test.InternetProtocol;

internal class NetworkLookupTest
{
    [Test]
    public void Add_IPv4_01()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("10.20.30.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.31.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.0/20"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("200.20.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.0/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.1/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.2/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.3/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.4/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.5/32"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            ├──10.20.0.0/16[0]
            │  └──10.20.16.0/20[0]
            │     └──10.20.30.0/23[1]
            │        ├──10.20.30.0/24[0]
            │        │  └──@10.20.30.0/29[0]
            │        │     ├──@10.20.30.0/30[0]
            │        │     │  ├──@10.20.30.0/31[0]
            │        │     │  │  ├──10.20.30.0/32[0]
            │        │     │  │  └──10.20.30.1/32[1]
            │        │     │  └──@10.20.30.2/31[1]
            │        │     │     ├──10.20.30.2/32[0]
            │        │     │     └──10.20.30.3/32[1]
            │        │     └──@10.20.30.4/31[1]
            │        │        ├──10.20.30.4/32[0]
            │        │        └──10.20.30.5/32[1]
            │        └──10.20.31.0/24[1]
            └──200.20.0.0/16[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_02()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("241.104.240.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/5"), string.Empty);
        networkLookup.TryAdd(Network.Parse("131.126.152.0/21"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──@128.0.0.0/1[1]
               ├──128.0.0.0/5[0]
               │  └──131.126.152.0/21[0]
               └──241.104.240.0/21[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_03()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("241.104.240.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("131.126.152.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/5"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──@128.0.0.0/1[1]
               ├──128.0.0.0/5[0]
               │  └──131.126.152.0/21[0]
               └──241.104.240.0/21[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_04()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("241.104.240.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/5"), string.Empty);
        networkLookup.TryAdd(Network.Parse("129.51.252.0/22"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──@128.0.0.0/1[1]
               ├──128.0.0.0/5[0]
               │  └──129.51.252.0/22[0]
               └──241.104.240.0/21[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_05()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("241.104.240.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/10"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/1"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──128.0.0.0/1[1]
               ├──128.0.0.0/10[0]
               └──241.104.240.0/21[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_06()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("19.45.156.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("241.104.240.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("97.84.184.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/5"), string.Empty);
        networkLookup.TryAdd(Network.Parse("236.191.0.0/17"), string.Empty);
        networkLookup.TryAdd(Network.Parse("144.0.0.0/5"), string.Empty);
        networkLookup.TryAdd(Network.Parse("107.216.27.192/26"), string.Empty);
        networkLookup.TryAdd(Network.Parse("76.97.224.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("209.223.135.160/27"), string.Empty);
        networkLookup.TryAdd(Network.Parse("24.38.216.86/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("116.11.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("183.44.80.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("138.131.250.96/27"), string.Empty);
        networkLookup.TryAdd(Network.Parse("206.215.229.48/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("196.75.180.192/26"), string.Empty);
        networkLookup.TryAdd(Network.Parse("126.115.54.228/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("106.125.142.0/25"), string.Empty);
        networkLookup.TryAdd(Network.Parse("129.51.252.0/22"), string.Empty);
        networkLookup.TryAdd(Network.Parse("131.192.0.0/10"), string.Empty);
        networkLookup.TryAdd(Network.Parse("126.139.34.32/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("161.135.64.0/18"), string.Empty);
        networkLookup.TryAdd(Network.Parse("126.7.96.208/28"), string.Empty);
        networkLookup.TryAdd(Network.Parse("59.172.0.0/17"), string.Empty);
        networkLookup.TryAdd(Network.Parse("157.138.0.0/17"), string.Empty);
        networkLookup.TryAdd(Network.Parse("143.0.0.0/8"), string.Empty);
        networkLookup.TryAdd(Network.Parse("22.139.97.160/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("219.142.7.16/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/10"), string.Empty);
        networkLookup.TryAdd(Network.Parse("63.144.0.0/12"), string.Empty);
        networkLookup.TryAdd(Network.Parse("30.9.180.0/25"), string.Empty);
        networkLookup.TryAdd(Network.Parse("198.0.0.0/7"), string.Empty);
        networkLookup.TryAdd(Network.Parse("71.139.101.244/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("93.104.192.0/22"), string.Empty);
        networkLookup.TryAdd(Network.Parse("233.80.208.0/22"), string.Empty);
        networkLookup.TryAdd(Network.Parse("207.41.170.0/27"), string.Empty);
        networkLookup.TryAdd(Network.Parse("239.126.117.0/26"), string.Empty);
        networkLookup.TryAdd(Network.Parse("74.125.100.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("243.178.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("25.105.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("22.153.94.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("62.208.0.0/12"), string.Empty);
        networkLookup.TryAdd(Network.Parse("71.250.76.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("233.0.0.0/8"), string.Empty);
        networkLookup.TryAdd(Network.Parse("186.0.0.0/8"), string.Empty);
        networkLookup.TryAdd(Network.Parse("105.2.104.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("56.68.0.0/14"), string.Empty);
        networkLookup.TryAdd(Network.Parse("121.0.108.130/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("69.240.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("103.9.128.0/18"), string.Empty);
        networkLookup.TryAdd(Network.Parse("153.223.208.226/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("29.96.0.0/13"), string.Empty);
        networkLookup.TryAdd(Network.Parse("182.184.248.0/22"), string.Empty);
        networkLookup.TryAdd(Network.Parse("222.191.143.0/26"), string.Empty);
        networkLookup.TryAdd(Network.Parse("30.179.27.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("247.164.252.48/28"), string.Empty);
        networkLookup.TryAdd(Network.Parse("204.169.45.128/25"), string.Empty);
        networkLookup.TryAdd(Network.Parse("95.176.231.24/29"), string.Empty);
        networkLookup.TryAdd(Network.Parse("70.78.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("149.215.38.48/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("52.170.192.0/19"), string.Empty);
        networkLookup.TryAdd(Network.Parse("128.0.0.0/1"), string.Empty);
        networkLookup.TryAdd(Network.Parse("99.143.58.32/28"), string.Empty);
        networkLookup.TryAdd(Network.Parse("13.0.0.0/10"), string.Empty);
        networkLookup.TryAdd(Network.Parse("161.225.254.128/25"), string.Empty);
        networkLookup.TryAdd(Network.Parse("221.67.82.208/29"), string.Empty);
        networkLookup.TryAdd(Network.Parse("131.126.152.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("115.252.72.0/25"), string.Empty);
        networkLookup.TryAdd(Network.Parse("190.233.246.0/25"), string.Empty);
        networkLookup.TryAdd(Network.Parse("248.128.0.0/11"), string.Empty);
        networkLookup.TryAdd(Network.Parse("235.200.0.0/19"), string.Empty);
        networkLookup.TryAdd(Network.Parse("51.229.96.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("168.64.0.0/12"), string.Empty);
        networkLookup.TryAdd(Network.Parse("218.72.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("92.7.192.0/18"), string.Empty);
        networkLookup.TryAdd(Network.Parse("103.176.0.0/14"), string.Empty);
        networkLookup.TryAdd(Network.Parse("40.200.240.0/22"), string.Empty);
        networkLookup.TryAdd(Network.Parse("4.183.145.24/29"), string.Empty);
        networkLookup.TryAdd(Network.Parse("32.0.0.0/3"), string.Empty);
        networkLookup.TryAdd(Network.Parse("83.64.0.0/11"), string.Empty);
        networkLookup.TryAdd(Network.Parse("136.132.111.160/28"), string.Empty);
        networkLookup.TryAdd(Network.Parse("57.52.80.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("111.237.188.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("59.227.4.240/29"), string.Empty);
        networkLookup.TryAdd(Network.Parse("120.167.32.0/19"), string.Empty);
        networkLookup.TryAdd(Network.Parse("222.84.48.0/20"), string.Empty);
        networkLookup.TryAdd(Network.Parse("155.74.188.152/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("201.212.0.0/18"), string.Empty);
        networkLookup.TryAdd(Network.Parse("201.156.134.52/30"), string.Empty);
        networkLookup.TryAdd(Network.Parse("203.93.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("61.37.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("108.73.96.0/19"), string.Empty);
        networkLookup.TryAdd(Network.Parse("159.0.0.0/10"), string.Empty);
        networkLookup.TryAdd(Network.Parse("50.138.16.144/28"), string.Empty);
        networkLookup.TryAdd(Network.Parse("213.35.163.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("75.129.128.136/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("176.64.0.0/12"), string.Empty);
        networkLookup.TryAdd(Network.Parse("44.152.0.0/15"), string.Empty);
        networkLookup.TryAdd(Network.Parse("132.178.136.192/29"), string.Empty);
        networkLookup.TryAdd(Network.Parse("216.160.0.0/11"), string.Empty);
        networkLookup.TryAdd(Network.Parse("63.105.163.184/29"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            ├──@0.0.0.0/1[0]
            │  ├──@0.0.0.0/2[0]
            │  │  ├──@0.0.0.0/3[0]
            │  │  │  ├──@0.0.0.0/4[0]
            │  │  │  │  ├──4.183.145.24/29[0]
            │  │  │  │  └──13.0.0.0/10[1]
            │  │  │  └──@16.0.0.0/4[1]
            │  │  │     ├──@16.0.0.0/5[0]
            │  │  │     │  ├──19.45.156.0/23[0]
            │  │  │     │  └──@22.128.0.0/11[1]
            │  │  │     │     ├──22.139.97.160/30[0]
            │  │  │     │     └──22.153.94.0/23[1]
            │  │  │     └──@24.0.0.0/5[1]
            │  │  │        ├──@24.0.0.0/7[0]
            │  │  │        │  ├──24.38.216.86/31[0]
            │  │  │        │  └──25.105.0.0/16[1]
            │  │  │        └──@28.0.0.0/6[1]
            │  │  │           ├──29.96.0.0/13[0]
            │  │  │           └──@30.0.0.0/8[1]
            │  │  │              ├──30.9.180.0/25[0]
            │  │  │              └──30.179.27.0/24[1]
            │  │  └──32.0.0.0/3[1]
            │  │     ├──@40.0.0.0/5[0]
            │  │     │  ├──40.200.240.0/22[0]
            │  │     │  └──44.152.0.0/15[1]
            │  │     └──@48.0.0.0/4[1]
            │  │        ├──@48.0.0.0/5[0]
            │  │        │  ├──@50.0.0.0/7[0]
            │  │        │  │  ├──50.138.16.144/28[0]
            │  │        │  │  └──51.229.96.0/23[1]
            │  │        │  └──52.170.192.0/19[1]
            │  │        └──@56.0.0.0/5[1]
            │  │           ├──@56.0.0.0/6[0]
            │  │           │  ├──@56.0.0.0/7[0]
            │  │           │  │  ├──56.68.0.0/14[0]
            │  │           │  │  └──57.52.80.0/21[1]
            │  │           │  └──@59.128.0.0/9[1]
            │  │           │     ├──59.172.0.0/17[0]
            │  │           │     └──59.227.4.240/29[1]
            │  │           └──@60.0.0.0/6[1]
            │  │              ├──61.37.0.0/16[0]
            │  │              └──@62.0.0.0/7[1]
            │  │                 ├──62.208.0.0/12[0]
            │  │                 └──@63.0.0.0/8[1]
            │  │                    ├──63.105.163.184/29[0]
            │  │                    └──63.144.0.0/12[1]
            │  └──@64.0.0.0/2[1]
            │     ├──@64.0.0.0/3[0]
            │     │  ├──@64.0.0.0/4[0]
            │     │  │  ├──@68.0.0.0/6[0]
            │     │  │  │  ├──69.240.0.0/16[0]
            │     │  │  │  └──@70.0.0.0/7[1]
            │     │  │  │     ├──70.78.0.0/16[0]
            │     │  │  │     └──@71.128.0.0/9[1]
            │     │  │  │        ├──71.139.101.244/31[0]
            │     │  │  │        └──71.250.76.0/23[1]
            │     │  │  └──@72.0.0.0/5[1]
            │     │  │     ├──@74.0.0.0/7[0]
            │     │  │     │  ├──74.125.100.0/24[0]
            │     │  │     │  └──75.129.128.136/31[1]
            │     │  │     └──76.97.224.0/21[1]
            │     │  └──@80.0.0.0/4[1]
            │     │     ├──83.64.0.0/11[0]
            │     │     └──@92.0.0.0/6[1]
            │     │        ├──@92.0.0.0/7[0]
            │     │        │  ├──92.7.192.0/18[0]
            │     │        │  └──93.104.192.0/22[1]
            │     │        └──95.176.231.24/29[1]
            │     └──@96.0.0.0/3[1]
            │        ├──@96.0.0.0/4[0]
            │        │  ├──@96.0.0.0/5[0]
            │        │  │  ├──@96.0.0.0/6[0]
            │        │  │  │  ├──97.84.184.0/21[0]
            │        │  │  │  └──99.143.58.32/28[1]
            │        │  │  └──@103.0.0.0/8[1]
            │        │  │     ├──103.9.128.0/18[0]
            │        │  │     └──103.176.0.0/14[1]
            │        │  └──@104.0.0.0/5[1]
            │        │     ├──@104.0.0.0/6[0]
            │        │     │  ├──105.2.104.0/21[0]
            │        │     │  └──@106.0.0.0/7[1]
            │        │     │     ├──106.125.142.0/25[0]
            │        │     │     └──107.216.27.192/26[1]
            │        │     └──@108.0.0.0/6[1]
            │        │        ├──108.73.96.0/19[0]
            │        │        └──111.237.188.0/23[1]
            │        └──@112.0.0.0/4[1]
            │           ├──@112.0.0.0/5[0]
            │           │  ├──115.252.72.0/25[0]
            │           │  └──116.11.0.0/16[1]
            │           └──@120.0.0.0/5[1]
            │              ├──@120.0.0.0/7[0]
            │              │  ├──120.167.32.0/19[0]
            │              │  └──121.0.108.130/31[1]
            │              └──@126.0.0.0/8[1]
            │                 ├──@126.0.0.0/9[0]
            │                 │  ├──126.7.96.208/28[0]
            │                 │  └──126.115.54.228/30[1]
            │                 └──126.139.34.32/30[1]
            └──128.0.0.0/1[1]
               ├──@128.0.0.0/2[0]
               │  ├──@128.0.0.0/3[0]
               │  │  ├──@128.0.0.0/4[0]
               │  │  │  ├──128.0.0.0/5[0]
               │  │  │  │  ├──@128.0.0.0/6[0]
               │  │  │  │  │  ├──@128.0.0.0/7[0]
               │  │  │  │  │  │  ├──128.0.0.0/10[0]
               │  │  │  │  │  │  └──129.51.252.0/22[1]
               │  │  │  │  │  └──@131.0.0.0/8[1]
               │  │  │  │  │     ├──131.126.152.0/21[0]
               │  │  │  │  │     └──131.192.0.0/10[1]
               │  │  │  │  └──132.178.136.192/29[1]
               │  │  │  └──@136.0.0.0/5[1]
               │  │  │     ├──@136.0.0.0/6[0]
               │  │  │     │  ├──136.132.111.160/28[0]
               │  │  │     │  └──138.131.250.96/27[1]
               │  │  │     └──143.0.0.0/8[1]
               │  │  └──@144.0.0.0/4[1]
               │  │     ├──144.0.0.0/5[0]
               │  │     │  └──149.215.38.48/30[1]
               │  │     └──@152.0.0.0/5[1]
               │  │        ├──@152.0.0.0/6[0]
               │  │        │  ├──153.223.208.226/31[0]
               │  │        │  └──155.74.188.152/30[1]
               │  │        └──@156.0.0.0/6[1]
               │  │           ├──157.138.0.0/17[0]
               │  │           └──159.0.0.0/10[1]
               │  └──@160.0.0.0/3[1]
               │     ├──@160.0.0.0/4[0]
               │     │  ├──@161.128.0.0/9[0]
               │     │  │  ├──161.135.64.0/18[0]
               │     │  │  └──161.225.254.128/25[1]
               │     │  └──168.64.0.0/12[1]
               │     └──@176.0.0.0/4[1]
               │        ├──@176.0.0.0/5[0]
               │        │  ├──176.64.0.0/12[0]
               │        │  └──@182.0.0.0/7[1]
               │        │     ├──182.184.248.0/22[0]
               │        │     └──183.44.80.0/21[1]
               │        └──@184.0.0.0/5[1]
               │           ├──186.0.0.0/8[0]
               │           └──190.233.246.0/25[1]
               └──@192.0.0.0/2[1]
                  ├──@192.0.0.0/3[0]
                  │  ├──@192.0.0.0/4[0]
                  │  │  ├──@196.0.0.0/6[0]
                  │  │  │  ├──196.75.180.192/26[0]
                  │  │  │  └──198.0.0.0/7[1]
                  │  │  └──@200.0.0.0/5[1]
                  │  │     ├──@200.0.0.0/6[0]
                  │  │     │  ├──@201.128.0.0/9[0]
                  │  │     │  │  ├──201.156.134.52/30[0]
                  │  │     │  │  └──201.212.0.0/18[1]
                  │  │     │  └──203.93.0.0/16[1]
                  │  │     └──@204.0.0.0/6[1]
                  │  │        ├──204.169.45.128/25[0]
                  │  │        └──@206.0.0.0/7[1]
                  │  │           ├──206.215.229.48/30[0]
                  │  │           └──207.41.170.0/27[1]
                  │  └──@208.0.0.0/4[1]
                  │     ├──@208.0.0.0/5[0]
                  │     │  ├──209.223.135.160/27[0]
                  │     │  └──213.35.163.0/24[1]
                  │     └──@216.0.0.0/5[1]
                  │        ├──@216.0.0.0/6[0]
                  │        │  ├──216.160.0.0/11[0]
                  │        │  └──@218.0.0.0/7[1]
                  │        │     ├──218.72.0.0/16[0]
                  │        │     └──219.142.7.16/30[1]
                  │        └──@220.0.0.0/6[1]
                  │           ├──221.67.82.208/29[0]
                  │           └──@222.0.0.0/8[1]
                  │              ├──222.84.48.0/20[0]
                  │              └──222.191.143.0/26[1]
                  └──@224.0.0.0/3[1]
                     ├──@232.0.0.0/5[0]
                     │  ├──@232.0.0.0/6[0]
                     │  │  ├──233.0.0.0/8[0]
                     │  │  │  └──233.80.208.0/22[0]
                     │  │  └──235.200.0.0/19[1]
                     │  └──@236.0.0.0/6[1]
                     │     ├──236.191.0.0/17[0]
                     │     └──239.126.117.0/26[1]
                     └──@240.0.0.0/4[1]
                        ├──@240.0.0.0/5[0]
                        │  ├──@240.0.0.0/6[0]
                        │  │  ├──241.104.240.0/21[0]
                        │  │  └──243.178.0.0/16[1]
                        │  └──247.164.252.48/28[1]
                        └──248.128.0.0/11[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }


    [Test]
    public void Add_IPv4_07()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("97.84.184.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("76.97.224.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("24.38.216.86/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("116.11.0.0/16"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
                @0.0.0.0/0
                └──@0.0.0.0/1[0]
                   ├──24.38.216.86/31[0]
                   └──@64.0.0.0/2[1]
                      ├──76.97.224.0/21[0]
                      └──@96.0.0.0/3[1]
                         ├──97.84.184.0/21[0]
                         └──116.11.0.0/16[1]
                """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_08()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("97.84.184.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("76.97.224.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("24.38.216.86/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("116.11.0.0/16"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──@0.0.0.0/1[0]
               ├──24.38.216.86/31[0]
               └──@64.0.0.0/2[1]
                  ├──76.97.224.0/21[0]
                  └──@96.0.0.0/3[1]
                     ├──97.84.184.0/21[0]
                     └──116.11.0.0/16[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test(Description = "Tests that the order networks are added in results in an identically structured tree by randomizing a sequence of networks.")]
    public void Add_IPv4_09()
    {
        var addresses = GetRandomIPv4Networks(9).Take(100).ToList();
        var expectedResults = """
            @0.0.0.0/0
            ├──@0.0.0.0/1[0]
            │  ├──@0.0.0.0/2[0]
            │  │  ├──@0.0.0.0/3[0]
            │  │  │  ├──@0.0.0.0/4[0]
            │  │  │  │  ├──@0.0.0.0/5[0]
            │  │  │  │  │  ├──1.15.68.76/30[0]
            │  │  │  │  │  └──7.204.0.0/14[1]
            │  │  │  │  └──@8.0.0.0/5[1]
            │  │  │  │     ├──9.213.0.0/21[0]
            │  │  │  │     └──@12.0.0.0/6[1]
            │  │  │  │        ├──@12.0.0.0/7[0]
            │  │  │  │        │  ├──12.42.159.160/27[0]
            │  │  │  │        │  └──13.53.22.224/27[1]
            │  │  │  │        └──@14.0.0.0/7[1]
            │  │  │  │           ├──14.174.117.224/27[0]
            │  │  │  │           └──@15.64.0.0/10[1]
            │  │  │  │              ├──15.82.0.0/16[0]
            │  │  │  │              └──15.125.80.0/20[1]
            │  │  │  └──@16.0.0.0/4[1]
            │  │  │     ├──@16.0.0.0/5[0]
            │  │  │     │  ├──16.212.224.0/21[0]
            │  │  │     │  └──@20.0.0.0/6[1]
            │  │  │     │     ├──@20.0.0.0/7[0]
            │  │  │     │     │  ├──20.88.107.192/26[0]
            │  │  │     │     │  └──21.243.142.0/27[1]
            │  │  │     │     └──23.115.104.68/31[1]
            │  │  │     └──@24.0.0.0/5[1]
            │  │  │        ├──@24.0.0.0/6[0]
            │  │  │        │  ├──24.27.252.0/22[0]
            │  │  │        │  └──26.0.0.0/7[1]
            │  │  │        │     └──27.72.141.32/27[1]
            │  │  │        └──29.238.171.229/32[1]
            │  │  └──@32.0.0.0/3[1]
            │  │     ├──@32.0.0.0/4[0]
            │  │     │  ├──@32.0.0.0/5[0]
            │  │     │  │  ├──@32.0.0.0/6[0]
            │  │     │  │  │  ├──32.70.172.252/30[0]
            │  │     │  │  │  └──34.129.33.0/24[1]
            │  │     │  │  └──@36.0.0.0/8[1]
            │  │     │  │     ├──36.21.58.0/23[0]
            │  │     │  │     └──@36.128.0.0/9[1]
            │  │     │  │        ├──36.132.10.160/27[0]
            │  │     │  │        └──36.249.18.160/28[1]
            │  │     │  └──@40.0.0.0/5[1]
            │  │     │     ├──@40.0.0.0/6[0]
            │  │     │     │  ├──41.28.160.0/20[0]
            │  │     │     │  └──43.21.24.0/21[1]
            │  │     │     └──45.17.239.48/28[1]
            │  │     └──@48.0.0.0/4[1]
            │  │        ├──@48.0.0.0/5[0]
            │  │        │  ├──@48.0.0.0/6[0]
            │  │        │  │  ├──49.133.132.0/22[0]
            │  │        │  │  └──50.218.179.77/32[1]
            │  │        │  └──@52.0.0.0/7[1]
            │  │        │     ├──52.137.186.232/30[0]
            │  │        │     └──53.35.141.144/29[1]
            │  │        └──@56.0.0.0/5[1]
            │  │           ├──59.168.0.0/13[0]
            │  │           └──@60.0.0.0/6[1]
            │  │              ├──@60.0.0.0/7[0]
            │  │              │  ├──60.104.0.0/13[0]
            │  │              │  └──61.82.160.0/19[1]
            │  │              └──63.232.128.0/17[1]
            │  └──@64.0.0.0/2[1]
            │     ├──@64.0.0.0/3[0]
            │     │  ├──@64.0.0.0/4[0]
            │     │  │  ├──69.69.125.128/30[0]
            │     │  │  └──@72.0.0.0/6[1]
            │     │  │     ├──72.45.215.172/30[0]
            │     │  │     └──75.119.0.0/18[1]
            │     │  └──@80.0.0.0/4[1]
            │     │     ├──@80.0.0.0/5[0]
            │     │     │  ├──@82.0.0.0/7[0]
            │     │     │  │  ├──@82.160.0.0/11[0]
            │     │     │  │  │  ├──82.174.0.0/16[0]
            │     │     │  │  │  └──82.186.40.0/21[1]
            │     │     │  │  └──83.35.24.0/21[1]
            │     │     │  └──84.7.21.0/25[1]
            │     │     └──@88.0.0.0/5[1]
            │     │        ├──90.49.191.176/30[0]
            │     │        └──93.84.16.0/20[1]
            │     └──@96.0.0.0/3[1]
            │        ├──@108.0.0.0/7[0]
            │        │  ├──108.52.0.0/14[0]
            │        │  └──@109.0.0.0/8[1]
            │        │     ├──109.114.42.64/28[0]
            │        │     └──109.195.0.0/17[1]
            │        └──@112.0.0.0/4[1]
            │           ├──@112.0.0.0/5[0]
            │           │  ├──@112.0.0.0/6[0]
            │           │  │  ├──112.188.80.138/31[0]
            │           │  │  └──114.58.179.128/25[1]
            │           │  └──@116.0.0.0/6[1]
            │           │     ├──@116.128.0.0/9[0]
            │           │     │  ├──116.148.228.248/29[0]
            │           │     │  └──116.245.102.96/27[1]
            │           │     └──118.211.168.0/21[1]
            │           └──@120.0.0.0/5[1]
            │              ├──@122.0.0.0/8[0]
            │              │  ├──122.126.242.224/29[0]
            │              │  └──122.185.39.192/26[1]
            │              └──126.136.224.176/31[1]
            └──@128.0.0.0/1[1]
               ├──@128.0.0.0/2[0]
               │  ├──@128.0.0.0/3[0]
               │  │  ├──@128.0.0.0/4[0]
               │  │  │  ├──135.219.46.96/27[0]
               │  │  │  └──@136.0.0.0/5[1]
               │  │  │     ├──@136.0.0.0/6[0]
               │  │  │     │  ├──@136.0.0.0/7[0]
               │  │  │     │  │  ├──136.31.0.0/16[0]
               │  │  │     │  │  └──137.179.70.134/31[1]
               │  │  │     │  └──139.238.0.32/27[1]
               │  │  │     └──140.27.139.0/26[1]
               │  │  └──@144.0.0.0/4[1]
               │  │     ├──@144.0.0.0/6[0]
               │  │     │  ├──145.153.117.128/25[0]
               │  │     │  └──@146.0.0.0/7[1]
               │  │     │     ├──@146.224.0.0/11[0]
               │  │     │     │  ├──146.224.231.0/28[0]
               │  │     │     │  └──146.246.167.116/30[1]
               │  │     │     └──@147.0.0.0/8[1]
               │  │     │        ├──147.49.212.128/27[0]
               │  │     │        └──147.184.88.160/27[1]
               │  │     └──@152.0.0.0/5[1]
               │  │        ├──155.236.103.124/32[0]
               │  │        └──@156.0.0.0/6[1]
               │  │           ├──156.86.54.32/27[0]
               │  │           └──159.232.224.92/31[1]
               │  └──@160.0.0.0/3[1]
               │     ├──@160.0.0.0/4[0]
               │     │  ├──@160.0.0.0/5[0]
               │     │  │  ├──@160.0.0.0/6[0]
               │     │  │  │  ├──160.64.0.0/10[0]
               │     │  │  │  └──163.61.239.0/24[1]
               │     │  │  └──166.45.146.96/30[1]
               │     │  └──170.126.0.0/15[1]
               │     └──@176.0.0.0/4[1]
               │        ├──@176.0.0.0/6[0]
               │        │  ├──@176.0.0.0/7[0]
               │        │  │  ├──176.177.200.0/23[0]
               │        │  │  └──177.0.0.0/8[1]
               │        │  └──178.140.80.58/31[1]
               │        └──@184.0.0.0/5[1]
               │           ├──185.79.106.99/32[0]
               │           └──@189.0.0.0/8[1]
               │              ├──189.71.230.32/27[0]
               │              └──189.173.173.67/32[1]
               └──@192.0.0.0/2[1]
                  ├──@192.0.0.0/3[0]
                  │  ├──@192.0.0.0/4[0]
                  │  │  ├──@192.0.0.0/5[0]
                  │  │  │  ├──@192.0.0.0/7[0]
                  │  │  │  │  ├──192.253.199.216/29[0]
                  │  │  │  │  └──193.171.208.160/27[1]
                  │  │  │  └──@196.0.0.0/6[1]
                  │  │  │     ├──197.160.32.0/19[0]
                  │  │  │     └──199.29.0.0/17[1]
                  │  │  └──@200.0.0.0/6[1]
                  │  │     ├──200.246.67.240/32[0]
                  │  │     └──202.234.178.236/30[1]
                  │  └──@208.0.0.0/4[1]
                  │     ├──@208.0.0.0/5[0]
                  │     │  ├──208.103.128.0/18[0]
                  │     │  └──215.164.82.0/23[1]
                  │     └──@216.0.0.0/5[1]
                  │        ├──217.212.88.0/24[0]
                  │        └──@222.0.0.0/7[1]
                  │           ├──222.213.13.64/26[0]
                  │           └──223.68.0.0/14[1]
                  └──@224.0.0.0/3[1]
                     ├──@224.0.0.0/4[0]
                     │  ├──@224.0.0.0/5[0]
                     │  │  ├──@224.0.0.0/6[0]
                     │  │  │  ├──225.142.204.80/29[0]
                     │  │  │  └──227.97.190.0/24[1]
                     │  │  └──229.128.64.0/18[1]
                     │  └──@232.0.0.0/5[1]
                     │     ├──@232.0.0.0/6[0]
                     │     │  ├──233.94.168.222/31[0]
                     │     │  └──@234.0.0.0/7[1]
                     │     │     ├──234.45.0.0/18[0]
                     │     │     └──235.149.86.126/32[1]
                     │     └──@236.0.0.0/6[1]
                     │        ├──237.72.134.160/27[0]
                     │        └──239.250.182.104/30[1]
                     └──@240.0.0.0/4[1]
                        ├──@240.0.0.0/5[0]
                        │  ├──@240.0.0.0/6[0]
                        │  │  ├──241.251.248.0/22[0]
                        │  │  └──242.53.194.0/25[1]
                        │  └──245.34.199.0/24[1]
                        └──@248.0.0.0/5[1]
                           ├──251.39.79.128/28[0]
                           └──@252.0.0.0/6[1]
                              ├──252.184.223.136/30[0]
                              └──254.88.134.128/30[1]
            """;

        var random = new Random(9);

        for (var i = 0; i < 1000; i++)
        {
            var networkLookup = new NetworkLookup<string>();

            foreach (var ipNetwork in addresses.OrderBy(item => random.NextInt64()))
                networkLookup.TryAdd(ipNetwork, string.Empty);

            var result = networkLookup.ToTextTree(IPVersion.IPv4);
            Assert.That(expectedResults, Is.EqualTo(result));
        }
    }

    [Test]
    public void Add_IPv4_10()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("51.229.96.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("40.200.240.0/22"), string.Empty);
        networkLookup.TryAdd(Network.Parse("32.0.0.0/3"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──32.0.0.0/3[0]
               ├──40.200.240.0/22[0]
               └──51.229.96.0/23[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv4_11()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("0.0.0.0/0"), string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            0.0.0.0/0
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test(Description = "Concurrency test")]
    public void Add_IPv4_12()
    {
        var networkLookup = new NetworkLookup<string>();
        var lookups = 0;
        var updates = 0;

        var networks = new List<Network>();

        for (var i = 0; i <= 31; i++)
        {
            var n = new Network(IPAddress.Parse("0.0.0.0"), i);
            networks.Add(n);

            if (i > 0)
                networks.Add(n.ComplementaryNetwork);
        }

        var updateThread = new Thread(() =>
        {
            for (var i = 0; i < 10_000; i++)
            {
                var random = new Random(20);
                networks = networks.OrderBy(item => random.NextInt64()).ToList();

                foreach (var network in networks)
                {
                    networkLookup.Add(network, string.Empty);
                    updates++;
                }

                networks = networks.OrderBy(item => random.NextInt64()).ToList();
                foreach (var network in networks)
                {
                    networkLookup.Remove(network);
                    updates++;
                }
            }
        });


        var readerThread = new Thread(() =>
        {
            while (updateThread.IsAlive)
            {
                var match = networkLookup.GetMatch(IPAddress.Any);
                Assert.That(match.Value == "Success");

                lookups++;
            }
        });


        networkLookup.Add("0.0.0.0/32", "Success");

        updateThread.IsBackground = true;
        updateThread.Start();

        readerThread.IsBackground = true;
        readerThread.Start();

        updateThread.Join();
        readerThread.Join();

        TestContext.WriteLine($"{updates:N0} updates, {lookups:N0} lookups");
    }

    [Test]
    public void Add_IPv4_13()
    {
        var networkLookup = new NetworkLookup();
        networkLookup.Add("69.0.0.0/8");
        networkLookup.Add("9.213.0.0/21");
        networkLookup.Add("52.137.186.232/30");
        networkLookup.Add("59.168.0.0/13");
        networkLookup.Add("59.168.83.0/24");
        networkLookup.Add("69.169.125.128/30");
        networkLookup.Add("69.15.21.0/25");
        networkLookup.Add("69.248.13.0/26");
        networkLookup.Add("109.114.42.64/28");
        networkLookup.Add("109.195.0.0/17");

        var result = networkLookup.ToTextTree(IPVersion.IPv4);
        var expectedResults = """
            @0.0.0.0/0
            └──@0.0.0.0/1[0]
               ├──@0.0.0.0/2[0]
               │  ├──9.213.0.0/21[0]
               │  └──@48.0.0.0/4[1]
               │     ├──52.137.186.232/30[0]
               │     └──59.168.0.0/13[1]
               │        └──59.168.83.0/24[0]
               └──@64.0.0.0/2[1]
                  ├──69.0.0.0/8[0]
                  │  ├──69.15.21.0/25[0]
                  │  └──@69.128.0.0/9[1]
                  │     ├──69.169.125.128/30[0]
                  │     └──69.248.13.0/26[1]
                  └──@109.0.0.0/8[1]
                     ├──109.114.42.64/28[0]
                     └──109.195.0.0/17[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv6_01()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd("2001:db8:3333:4444:5555:6666:7777:8888", string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv6);

        var expectedResults = """
            @::/0
            └──2001:db8:3333:4444:5555:6666:7777:8888/128[0]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Add_IPv6_02()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd("2001:db8:3333:4444:5555:6666:7777:8888", string.Empty);
        networkLookup.TryAdd("2001:db8:3333:4444:CCCC:DDDD:EEEE:FFFF", string.Empty);
        networkLookup.TryAdd("2001:db8::", string.Empty);
        networkLookup.TryAdd("::", string.Empty);
        networkLookup.TryAdd("::1234:5678", string.Empty);
        networkLookup.TryAdd("2001:db8::1234:5678", string.Empty);
        networkLookup.TryAdd("2001:db8::1234:5678/32", string.Empty);
        networkLookup.TryAdd("2001:db8::1234:5678/8", string.Empty);
        networkLookup.TryAdd("2001:0db8:0001:0000:0000:0ab9:C0A8:0102", string.Empty);
        networkLookup.TryAdd("2001:db8:3333:4444:5555:6666:1.2.3.4", string.Empty);
        networkLookup.TryAdd("::11.22.33.44", string.Empty);
        networkLookup.TryAdd("2001:db8::123.123.123.123", string.Empty);

        var result = networkLookup.ToTextTree(IPVersion.IPv6);

        var expectedResults = """
            @::/0
            └──@::/2[0]
               ├──@::/99[0]
               │  ├──@::/100[0]
               │  │  ├──::/128[0]
               │  │  └──::11.22.33.44/128[1]
               │  └──::18.52.86.120/128[1]
               └──2000::/8[1]
                  └──2001:db8::/32[0]
                     └──@2001:db8::/34[0]
                        ├──@2001:db8::/47[0]
                        │  ├──@2001:db8::/97[0]
                        │  │  ├──@2001:db8::/99[0]
                        │  │  │  ├──2001:db8::/128[0]
                        │  │  │  └──2001:db8::1234:5678/128[1]
                        │  │  └──2001:db8::7b7b:7b7b/128[1]
                        │  └──2001:db8:1::ab9:c0a8:102/128[1]
                        └──@2001:db8:3333:4444::/64[1]
                           ├──@2001:db8:3333:4444:5555:6666::/97[0]
                           │  ├──2001:db8:3333:4444:5555:6666:102:304/128[0]
                           │  └──2001:db8:3333:4444:5555:6666:7777:8888/128[1]
                           └──2001:db8:3333:4444:cccc:dddd:eeee:ffff/128[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Delete_IPv4_01()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("10.20.30.0/23"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.31.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.0/20"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("200.20.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.0/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.1/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.2/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.3/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.4/32"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.20.30.5/32"), string.Empty);


        networkLookup.TryRemove(Network.Parse("10.20.0.0/16"));
        networkLookup.TryRemove(Network.Parse("10.20.30.5/32"));


        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            ├──10.20.16.0/20[0]
            │  └──10.20.30.0/23[1]
            │     ├──10.20.30.0/24[0]
            │     │  └──@10.20.30.0/29[0]
            │     │     ├──@10.20.30.0/30[0]
            │     │     │  ├──@10.20.30.0/31[0]
            │     │     │  │  ├──10.20.30.0/32[0]
            │     │     │  │  └──10.20.30.1/32[1]
            │     │     │  └──@10.20.30.2/31[1]
            │     │     │     ├──10.20.30.2/32[0]
            │     │     │     └──10.20.30.3/32[1]
            │     │     └──10.20.30.4/32[1]
            │     └──10.20.31.0/24[1]
            └──200.20.0.0/16[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Delete_IPv4_02()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("97.84.184.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("76.97.224.0/21"), string.Empty);
        networkLookup.TryAdd(Network.Parse("24.38.216.86/31"), string.Empty);
        networkLookup.TryAdd(Network.Parse("116.11.0.0/16"), string.Empty);

        networkLookup.TryRemove(Network.Parse("76.97.224.0/21"));
        var result = networkLookup.ToTextTree(IPVersion.IPv4);

        var expectedResults = """
            @0.0.0.0/0
            └──@0.0.0.0/1[0]
               ├──24.38.216.86/31[0]
               └──@96.0.0.0/3[1]
                  ├──97.84.184.0/21[0]
                  └──116.11.0.0/16[1]
            """;

        Assert.That(expectedResults, Is.EqualTo(result));
    }

    [Test]
    public void Delete_IPv4_03()
    {
        var random = new Random(55);
        var randomNetworks = GetRandomIPv4Networks(39).Distinct().Take(10000).ToList();

        var networksToAddAndKeep = randomNetworks.Take(8000).ToList();
        var networksToAddThenRemove = randomNetworks.Skip(8000).ToList();
        var allNetworks = randomNetworks.OrderBy(item => random.NextInt64()).ToList();

        var referenceLookup = new NetworkLookup<string>();
        foreach (var ipNetwork in networksToAddAndKeep)
            referenceLookup.TryAdd(ipNetwork, default);

        var expected = referenceLookup.ToTextTree(IPVersion.IPv4);

        var networkLookup = new NetworkLookup<string>();
        foreach (var ipNetwork in allNetworks)
            networkLookup.TryAdd(ipNetwork, default);

        foreach (var ipNetwork in networksToAddThenRemove)
            networkLookup.TryRemove(ipNetwork);

        var actual = networkLookup.ToTextTree(IPVersion.IPv4);

        Assert.That(expected, Is.EqualTo(actual));
    }

    public IEnumerable<IPAddress> GetRandomIPv4Addresses(int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        do
        {
            var addressBits = random.NextInt64(0, 0xff_ff_ff_ffL + 1);
            var addressBytes = BitConverter.GetBytes(addressBits).Take(4).ToArray();
            var randomIpAddress = new IPAddress(addressBytes);

            yield return randomIpAddress;
        } while (true);
    }

    public IEnumerable<Network> GetRandomIPv4Networks(int? seed = null)
    {
        var random = seed.HasValue ? new Random(seed.Value) : new Random();

        do
        {
            var addressBits = random.NextInt64(0, 0xff_ff_ff_ffL + 1);
            var addressBytes = BitConverter.GetBytes(addressBits).Take(4).ToArray();
            var randomIpAddress = new IPAddress(addressBytes);
            var randomPrefix = (ushort)Math.Cbrt(random.Next(0, 33 * 33 * 33));
            var randomNetwork = new Network(randomIpAddress, randomPrefix);

            yield return randomNetwork;
        } while (true);
    }

    //[Test]
    public void ToDotSvgTree_IPv4_01()
    {
        var ipNetworkLookup = new NetworkLookup<string>();

        var randomNetworks = GetRandomIPv4Networks(0).Distinct().Where(item => item.ContainsBy("10.0.0.0/8")).Take(3000).ToList();
        foreach (var randomNetwork in randomNetworks)
            ipNetworkLookup.TryAdd(randomNetwork, string.Empty);

        ipNetworkLookup.ToDotSvgTree(@"c:\temp\test123r.svg", IPVersion.IPv4);
    }

    //[Test]
    public void ToDotSvgTree_IPv4_02()
    {
        var ipNetworkLookup = new NetworkLookup<string>();

        var randomNetworks = GetRandomIPv4Networks(0).Distinct().Take(200).ToList();
        foreach (var randomNetwork in randomNetworks)
            ipNetworkLookup.TryAdd(randomNetwork, string.Empty);

        ipNetworkLookup.ToDotSvgTree(@"c:\temp\test123.svg", IPVersion.IPv4);
    }

    //[Test]
    public void ToDotSvgTree_IPv4_03()
    {
        var ipNetworkLookup = new NetworkLookup<string>();

        ipNetworkLookup.TryAdd(Network.Parse("10.0.0.0/8"), string.Empty);
        ipNetworkLookup.TryAdd(Network.Parse("10.0.0.0/20"), string.Empty);
        ipNetworkLookup.TryAdd(Network.Parse("10.0.0.0/24"), string.Empty);
        ipNetworkLookup.TryAdd(Network.Parse("10.0.0.0/32"), string.Empty);
        ipNetworkLookup.TryAdd(Network.Parse("10.0.0.0/16"), string.Empty);

        ipNetworkLookup.ToDotSvgTree(@"c:\temp\test123s.svg", IPVersion.IPv4);
    }

    [Test]
    public void TryGetMatch_IPv4_01()
    {
        var networkLookup = new NetworkLookup<string>();

        networkLookup.TryAdd(Network.Parse("10.0.0.0/8"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.0.0.0/16"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.0.0.0/20"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.0.0.0/24"), string.Empty);
        networkLookup.TryAdd(Network.Parse("10.0.0.0/32"), string.Empty);

        var success = networkLookup.TryGetMatch("10.0.0.0/25", out var expectedResults);

        Assert.That(success, Is.EqualTo(true));

        if (success)
            Assert.That(expectedResults.Network, Is.EqualTo(Network.Parse("10.0.0.0/24")));
    }
}