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

using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Bitvantage.NetworkAddressing.InternetProtocol;

namespace Test.Ethernet
{
    internal record AsnRecord
    {
        [TypeConverter(typeof(IpNetworkConverter))]
        [Name("network")]
        public Network? Network { get; set; }
        [Name("autonomous_system_number")]
        public uint AutonomousSystemNumber { get; set; }
        [Name("autonomous_system_organization")]
        public string? Organization { get; set; }

        private class IpNetworkConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
            {

                return Network.Parse(text!);
            }
        }
    }


}
