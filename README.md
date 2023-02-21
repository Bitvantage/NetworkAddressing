# NetworkAddressing
A library for working with network addresses

## Features
* Network class that represents IPv4 and IPv6 networks
* Network lookup class for high performance prefix matching using a variable stride trie
* MAC address class that represents an Ethernet MAC address
* OUI database to lookup manufacturer information of MAC addresses

# License
The NetworkAddressing library is licensed under LGPL v2.1

# Installing via NuGet
```sh
dotnet add package NetworkAddressing
```

# Overview
A network object represents either an IPv4 or IPv6 network.
## Network
```csharp
// create new network object
var network = Network.Parse("10.0.0.0/24");

// get the total number of host addresses in a network
var totalHosts = network.TotalHosts;

// get the total number of addresses in a network
var totalAddresses = network.TotalAddresses;

// iterate through each host address
foreach(var address in network.Hosts()
	Console.WriteLine(address);

// determin if a network is contained by a different network
var networkContainsNetwork = network.Contains("10.0.0.0/25");

// determin if a IP address is contained by a network
var networkContainsHost = network.Contains(IPAddress.Parse("10.0.0.100");
```

# Network Lookup
The NetworkLookup class uses a high performance variable stride trie to represent a large number of IPv4 or IPv6 networks in a compact tree structure. This structure allows for longest and full prefix matching in an efficient way.

```csharp
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
```

## Longest Matching Prefix
Routers typically work by finding the best route to the destination address. It does this by searching through its routing table to find the longest, or most specific, route to a particular destination.

```csharp
networkLookup.GetMatch(IPAddress.Parse("69.248.13.12");
```

## All Matching Prefixes
```csharp
networkLookup.GetMatchs(IPAddress.Parse("69.248.13.12");
```

## Tree Visualization 
A Graphviz DOT diagram can generated, and optionaly rendered. In order to render the DOT diagram into a SVG you must have [Graphviz](https://graphviz.org/) installed.
```csharp
networkLookup.ToDotSvgTree(@"c:\temp\graph.svg", IPVersion.IPv4);
```

A text representation of the tree can also be generated.
```csharp
networkLookup.ToTextTree(IPVersion.IPv4);
```

## Variable Stride Trie
Networks are organizing into a wide, shallow tree structure. The maximum depth of the tree is 33 levels for IPv4 or 129 levels for IPv6. The actual depth largely depends on how closely related the networks are; however for most application the depth is generally in the single digits. The depth of the tree is directly proportional to lookup performance.

Below is a visualization of a small tree that contains 10 networks. The orange node is the root node, green nodes are container nodes, and blue nodes are network objects and their associated values.

Network node have between zero and two children, container nodes have exactly two nodes. The child that is contained in the first half of the parents address space is on the left side, the child that is contained by the last half of the parents address space is on the right side. Container nodes are automatically added and removed to maintain the correct parent child relationship.

![Small Network Tree](https://raw.githubusercontent.com/Bitvantage/NetworkAddressing/master/Documentation/Media/NetworkLookup-Tree-Small.svg)

As more networks are added to the tree, the tree tends to grow in width far more quickly then depth. Since the depth of the tree is directly proportional to the search performance, it is possible to rapidly search a tree with millions of networks while only examining a relatively small number of the nodes.

![Wide Network Tree](https://raw.githubusercontent.com/Bitvantage/NetworkAddressing/master/Documentation/Media/NetworkLookup-Tree-Wide.png)

## Performance
Performance will vary based on many factors. On an older AMD Ryzen 5 1600X single thread lookup speed of a tree with one million networks is 1.25 million lookups per second. As a point of reference, a Dictionary used for exact matches is about to do around 10 million lookups per second.

## Thread Safety
The standard NetworkLookup class is lock free and thread safe for multiple-readers, and a single writer. A ConcurrentNetworkLookup class is provided that implements locks on write operations and should be used where there are concurrent writers. 

## MAC Address
A [MAC address](https://en.wikipedia.org/wiki/MAC_address) is a globally unique identifier assigned to Ethernet devices that consists of a bit to indicate if it is locally administrated, a bit to indicate if it is a multicast address, a manufacture identifier, and an extension identifier or serial number.
```csharp
var macAddress1 = MacAddress.Parse("dead:beef:abcd");
var macAddress2 = MacAddress.Parse("dead.beef.abcd");
var macAddress3 = MacAddress.Parse("de.ad.be.ef.ab.cd");
var macAddress4 = MacAddress.Parse("de:ad:be:ef:ab:cd");
var macAddress5 = MacAddress.Parse("deadbeefabcd");
```

## OUI Database
The first three octets of a MAC address are assigned by the IEEE. The registration data is public and can be looked up for a given MAC address. The manufacture can often give clues as to what the device is. 
```csharp
// Create a new OuiDatabase using the defaults
// The default options will use last downloaded database if it exists, and fallback to an internal database if it does not exist. 
// An asynchronous background update check is triggered upon object creation that will update the database every 30 days.
var ouiDatabase = new OuiDatabase();

var ouiRecord = ouiDatabase["64:D1:A3:01:02:03"];
var organization = ouiRecord.Organization;
var address = ouiRecord.Address;
var prefix = ouiRecord.Prefix;
```

### Options
```csharp
var options = new OuiDatabaseOptions()
{
    // Automatically preform background updates. Defaults to true
    // UpdateDatabase() can be used to manually trigger a database update.
    AutomaticUpdates = true,

    // Where to store the downloaded database. Defaults to %LOCALAPPDATA%\NetworkAddressing
    CacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NetworkAddressing"),

    // How offten to check if an update should be triggered. Defaults to once an hour.
    CheckInterval = TimeSpan.FromHours(1),

    // Where to download the OUI database from. Defaults to https://standards-oui.ieee.org/
    Location = new("https://standards-oui.ieee.org/"),

    // How old the downloaded database can be before downloading a new version. Defaults to 30 days.
    RefreshInterval = TimeSpan.FromDays(30),

    // If the initial database update that is triggered on object creation should block.  Defaults to false.
    // If set to false the last downloaded database or an internal database is initially used and a background update check is triggered.
    // If set to true the object creation is blocked while an update check is triggered.
    SynchronousUpdate = false,

    // If SynchronousUpdate both ThrowOnUpdateFailure are set to true and the initial database update fails
    // then throw an exception and prevent the object from being created.
    // defaults to false
    ThrowOnUpdateFailure = false,
};

// Events can be generated for update events, which can be logged.
options.DatabaseEvent += OptionsOnDatabaseEvent;

var ouiDatabase = new OuiDatabase(options);
```

### Overhead
There is a fair amount of overhead for each OuiDatabase instance and it is best to reuse a single instance or use the OuiDatabase.Instance property.
The OuiDatabase.Instance property returns an instance of OuiDatabase using the default options and is suitable for most purposes.
```csharp
var ouiDatabase = OuiDatabase.Instance;

var ouiRecord = ouiDatabase["64:D1:A3:01:02:03"];
var organization = ouiRecord.Organization;
var address = ouiRecord.Address;
var prefix = ouiRecord.Prefix;
```