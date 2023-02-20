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

using System.IO.Compression;
using System.Net;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Bitvantage.NetworkAddressing.InternetProtocol;

namespace PerformanceTest;

public class Program
{
    public class MyTests
    {
        private static readonly NetworkLookup<string> NetworkLookup = new();
        private static readonly List<Network> IPNetworks = GetRandomIpv4Networks(11).Take(100000).Distinct().ToList();
        private static readonly List<IPAddress> IPAddresses = GetRandomIpv4Addresses(12).Take(1000000).ToList();

        static MyTests()
        {
            foreach (var ipNetwork in IPNetworks)
                NetworkLookup.TryAdd(ipNetwork, string.Empty);

        }

        [IterationCount(30)]
        [Benchmark]
        public void IpNetworkLookupX1M()
        {
            foreach (var ipAddress in IPAddresses)
                if (NetworkLookup.GetMatch(ipAddress).Value != String.Empty)
                    throw new Exception();
        }

        [IterationCount(30)]
        [Benchmark]
        public void AddX100K()
        {
            var networkLookup = new NetworkLookup<string?>();
            foreach (var network in IPNetworks)
                networkLookup.TryAdd(network, default);
        }

        [IterationCount(30)]
        [Benchmark]
        public void ConcurrenceAddX100K()
        {
            var networkLookup = new ConcurrenceNetworkLookup<string?>();
            foreach (var network in IPNetworks)
                networkLookup.TryAdd(network, default);
        }
    }

    static void Compress(string inputFileName, string outputFileName)
    {
        using (var input = File.OpenRead(inputFileName))
        using (var output = File.Create(outputFileName))
        using (var compressor = new BrotliStream(output, CompressionLevel.SmallestSize))
        {
            input.CopyTo(compressor);
        }
    }

    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<MyTests>();
        Console.WriteLine(summary);
    }

    public static IEnumerable<Network> GetRandomIpv4Networks(int? seed = null)
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

    public static IEnumerable<IPAddress> GetRandomIpv4Addresses(int? seed = null)
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
}