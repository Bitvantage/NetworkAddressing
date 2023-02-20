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
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace Bitvantage.NetworkAddressing.InternetProtocol;

/// <summary>
///     Organize IpNetworks into a tree structure that is wide and shallow which allows for rapid searching by using an
///     IPAddress or Network as the key.
/// </summary>
/// <typeparam name="TValue">The type of the value that is associated with the network</typeparam>
public abstract class NetworkLookupBase<TValue> where TValue : NetworkKey, new()
{
    public long Count { get; private set; }

    // TODO: the root depends on if this is IPv4 or IPv6...
    // could keep two separate trees, an IPv4 tree and a IPv6 tree and route the requests to the right place

    private Node RootV4 { get; set; } = new(new Network(IPAddress.Any, 0));
    private Node RootV6 { get; set; } = new(new Network(IPAddress.IPv6Any, 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual void Add(TValue value)
    {
        if (!TryAdd(value))
            throw new ArgumentException("An entry with the same key already exists.");
    }

    public virtual void Clear()
    {
        RootV4 = new Node(new Network(IPAddress.None, 0));
        RootV6 = new Node(new Network(IPAddress.IPv6None, 0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Node GetRoot(Network network)
    {
        if (network.Version == IPVersion.IPv4)
            return RootV4;

        return RootV6;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Node GetRoot(IPAddress ipAddress)
    {
        return ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork => RootV4,
            AddressFamily.InterNetworkV6 => RootV6,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetMatch(IPAddress ipAddress)
    {
        if (TryGetMatch(ipAddress, out var result))
            return result;

        throw new KeyNotFoundException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue GetMatch(Network address)
    {
        if (TryGetMatch(address, out var result))
            return result;

        throw new KeyNotFoundException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IList<TValue> GetMatches(IPAddress ipAddress)
    {
        if (TryGetMatches(ipAddress, out var result))
            return result;

        throw new KeyNotFoundException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IList<TValue> GetMatches(Network address)
    {
        if (TryGetMatches(address, out var result))
            return result;

        throw new KeyNotFoundException();
    }


    public virtual void Remove(Network network)
    {
        if (!TryRemove(network))
            throw new KeyNotFoundException();
    }

    public void ToDotSvgTree(string filename, IPVersion version,  Func<TValue, string>? displayText = null, string? dotPath = null)
    {
        var fullPath = Path.GetFullPath(filename);
        var workingDirectory = Path.GetDirectoryName(fullPath);

        if (workingDirectory == null)
            throw new ArgumentException($"Absolute path could not be determined: {filename}", nameof(filename));

        var fullDotPath = dotPath ?? @"C:\Program Files\Graphviz\bin\dot.exe";
        if (!File.Exists(fullDotPath))
            throw new FileNotFoundException($"Could not find dot.exe: {fullDotPath}");

        var fileName = Path.GetFileNameWithoutExtension(fullPath);
        var dotFilename = Path.Combine(workingDirectory, $"{fileName}.dot");
        var svgFilename = Path.Combine(workingDirectory, $"{fileName}.svg");

        File.WriteAllText(dotFilename, ToDotTree(version, displayText));

        var dotProcess = Process.Start(new ProcessStartInfo(dotPath ?? @"C:\Program Files\Graphviz\bin\dot.exe", $"-Tsvg {dotFilename} -o{svgFilename}") { CreateNoWindow = false, WorkingDirectory = workingDirectory });

        if (dotProcess == null)
            throw new Exception("dot failed to start");

        dotProcess.WaitForExit();

        if (dotProcess.ExitCode != 0)
            throw new Exception($"dot failed with exit code {dotProcess.ExitCode}");
    }


    private record NodeRelation(Node Node, NodeRelation? Parent)
    {
    }

    public string ToDotTree(IPVersion version, Func<TValue, string>? displayText = null)
    {
        var root = version switch
        {
            IPVersion.IPv4 => RootV4,
            IPVersion.IPv6 => RootV6,
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        // dot -Tsvg test123.dot -otest123.svg

        // initialize a stack with the root node
        var stack = new Stack<NodeRelation>();
        stack.Push(new NodeRelation(root, null));

        var stringBuilder = new StringBuilder();

        // write dot graph header
        stringBuilder.AppendLine("digraph \"Network Lookup Tree\" {");
        stringBuilder.AppendLine("\tgraph [ overlap=true ranksep=1.3 ]");
        stringBuilder.AppendLine("\tnode [shape=plaintext fontname=\"Consolas\" fontsize=\"8\"]");
        stringBuilder.AppendLine();

        while (stack.TryPop(out var currentNode))
        {
            var isRoot = root == currentNode.Node;

            stringBuilder.AppendLine($"\t\"{currentNode.Node.ValuePair.Network}\" [");

            if (isRoot)
                stringBuilder.AppendLine("\t\troot=true");

            stringBuilder.AppendLine($"\t\trank={currentNode.Node.ValuePair.Network.Prefix + 1}");
            stringBuilder.AppendLine("\t\tlabel=<");
            stringBuilder.AppendLine($"\t\t\t<table border=\"0\" cellborder=\"0\" cellspacing=\"0\" cellpadding=\"0\"{(isRoot ? " bgcolor=\"orange\"" : "")}>");

            var nodeText = string.Empty;
            if (displayText != null && currentNode.Node.HasValue)
                nodeText = $" {displayText.Invoke((TValue)currentNode.Node.ValuePair)}";

            stringBuilder.AppendLine($"\t\t\t\t<tr><td align=\"left\"{(isRoot ? " border=\"1\"" : "")}><b>{currentNode.Node.ValuePair.Network.Address}/{currentNode.Node.ValuePair.Network.Prefix}{nodeText}</b></td></tr>");

            stringBuilder.AppendLine($"\t\t\t\t\t<tr><td><table cellborder=\"0\" cellspacing=\"0\" cellpadding=\"0\" bgcolor=\"{(currentNode.Node.HasValue ? "lightblue" : "yellowgreen")}\" >");

            stringBuilder.Append("\t\t\t\t\t\t<tr><td align=\"left\">");
            WriteAddress(currentNode, true);
            stringBuilder.Append("</td></tr>");

            stringBuilder.AppendLine();

            stringBuilder.Append("\t\t\t\t\t\t<tr><td align=\"left\">");
            WriteAddress(currentNode, false);
            stringBuilder.Append("</td></tr>");

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("\t\t\t\t\t</table></td></tr>");
            stringBuilder.AppendLine("\t\t\t\t</table>>");
            stringBuilder.AppendLine("\t\t\t];");

            stringBuilder.AppendLine();

            for (var i = 0; i <= 1; i++)
                if (currentNode.Node.Children[i] != null)
                {
                    // link this nodes children to the this parent
                    stringBuilder.AppendLine($"\t\"{currentNode.Node.ValuePair.Network}\" -> \"{currentNode.Node.Children[i].ValuePair.Network}\" [ arrowhead=\"{(i == 0 ? "empty" : "normal")}\" ]");

                    // push this nodes children to the stack
                    stack.Push(new NodeRelation(currentNode.Node.Children[i], currentNode));
                }
        }

        // write dot graph footer
        stringBuilder.AppendLine();
        stringBuilder.AppendLine("}");

        return stringBuilder.ToString();

        void WriteAddress(NodeRelation currentNode, bool highlightSplitBit)
        {
            var bits = currentNode.Node.NetworkBits;

            for (var i = 0; i < currentNode.Node.AddressLength; i++)
            {
                if (i == currentNode.Node.Prefix)
                    stringBuilder.Append("<font color=\"gray\">");

                if (i % 8 == 0 && i > 0 && version == IPVersion.IPv4)
                    stringBuilder.Append(".");

                if (i % 16 == 0 && i > 0 && version == IPVersion.IPv6)
                    stringBuilder.Append(":");

                if (highlightSplitBit && currentNode.Parent != null && i == currentNode.Parent.Node.Prefix)
                    stringBuilder.Append("<U><B>");

                if ((bits & UInt128.One << currentNode.Node.AddressLength - 1 - i) > UInt128.Zero)
                    stringBuilder.Append('1');
                else
                    stringBuilder.Append('0');

                if (highlightSplitBit && currentNode.Parent != null && i == currentNode.Parent.Node.Prefix)
                    stringBuilder.Append("</B></U>");
            }

            if (currentNode.Node.Prefix != currentNode.Node.AddressLength)
                stringBuilder.Append("</font>");
        }
    }

    public string ToTextTree(IPVersion version, Func<TValue, string>? displayText = null)
    {
        // initialize a stack with the root node
        var stack = new Stack<TreeNode>();

        var root = version switch
        {
            IPVersion.IPv4 => RootV4,
            IPVersion.IPv6 => RootV6,
            _ => throw new ArgumentOutOfRangeException(nameof(version), version, null)
        };

        var rootNode = new TreeNode(root, 0, -1);
        stack.Push(rootNode);

        // in order to output the tree elements correctly, the number of remaining children for each level must be know
        // each time a node is popped off the stack record the number of child elements
        // each time a child element is printed out, decrement teh remaining child count
        var remainingChildren = new int[129];
        remainingChildren[0] = rootNode.ChildCount;

        var stringBuilder = new StringBuilder();

        while (stack.TryPop(out var treeNode))
        {
            remainingChildren[treeNode.Level] = treeNode.ChildCount;

            // node level 0 is not indented, all other nodes are
            if (treeNode.Level == 0)
            {
                stringBuilder.AppendLine($"{(treeNode.Node.HasValue ? "" : "@")}{treeNode.Node.ValuePair.Network}");
            }
            else
            {
                remainingChildren[treeNode.Level - 1]--;

                // output the branches for all parent nodes
                for (var i = 1; i < treeNode.Level; i++)
                    stringBuilder.Append(remainingChildren[i - 1] > 0 ? "│  " : "   ");

                stringBuilder.Append(remainingChildren[treeNode.Level - 1] > 0 ? "├──" : "└──");

                // output this node
                if (displayText != null)
                    stringBuilder.AppendLine($"{(treeNode.Node.HasValue ? "" : "@")}{treeNode.Node.ValuePair.Network}[{treeNode.Index}] {displayText.Invoke((TValue)treeNode.Node.ValuePair)}");
                else
                    stringBuilder.AppendLine($"{(treeNode.Node.HasValue ? "" : "@")}{treeNode.Node.ValuePair.Network}[{treeNode.Index}]");
            }

            // push this nodes children to the stack
            for (var i = 1; i >= 0; i--)
                if (treeNode.Node.Children[i] != null)
                    stack.Push(new TreeNode(treeNode.Node.Children[i], treeNode.Level + 1, i));
        }

        // remove the extra CR/LF at the end
        if (stringBuilder.Length > 0)
            stringBuilder.Remove(stringBuilder.Length - 2, 2);

        return stringBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool TryAdd(TValue valuePair)
    {
        var nodeHistory = WalkTree(valuePair.Network);
        var parent = nodeHistory[^1].Node;


        // if the new network already exists
        if (valuePair.Network == parent.ValuePair.Network)
        {
            // and it is a value node, then the requested network already exists
            if (parent.HasValue)
                return false;

            // but if the new network is a routing node
            // convert the routing node to a value node
            parent.ValuePair = valuePair;
            parent.HasValue = true;

            Count++;
            return true;
        }

        var targetSlot = parent.GetSlot(valuePair.Network);

        // if the parents target slot is empty, add the node directly
        if (parent.Children[targetSlot] == null)
        {
            parent.Children[targetSlot] = new Node(valuePair, true);
            return true;
        }

        // the new node needs to be added as a child of an existing node; but the existing node already has a child
        var newNode = new Node(valuePair, true);
        var existingNode = parent.Children[targetSlot];

        // if the new node has a smaller prefix then the existing node, the new node can be inserted directly above the existing node
        if (newNode.ValuePair.Network.Prefix < existingNode.ValuePair.Network.Prefix && newNode.ValuePair.Network.Contains(existingNode.ValuePair.Network))
        {
            newNode.Children[newNode.GetSlot(existingNode.ValuePair.Network)] = existingNode;
            parent.Children[targetSlot] = newNode;

            Count++;
            return true;
        }

        // if the new node has the same or greater prefix, then a routing node needs to be inserted directly above
        // the routing node will cover both prefixes and contain both the existing node and new node

        // calculate the smallest network that contains both the existing node and the new node 
        var routingNetwork = valuePair.Network.GetContainingNetwork(existingNode.ValuePair.Network);
        var routingNode = new Node(routingNetwork);

        // the routing node prefix is larger then the parent node
        routingNode.Children[routingNode.GetSlot(newNode.ValuePair.Network)] = newNode;
        routingNode.Children[routingNode.GetSlot(existingNode.ValuePair.Network)] = existingNode;

        // re-point the parent node from the existing node to the new routing node
        parent.Children[targetSlot] = routingNode;

        Count++;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetMatch(IPAddress ipAddress, [NotNullWhen(true)] out TValue? result)
    {
        Node? lastValueNode = null;

        var currentNode = GetRoot(ipAddress);
        var addressBits = ipAddress.ToUInt128();

        do
        {
            if (currentNode.HasValue)
                lastValueNode = currentNode;

            currentNode = currentNode.Children[currentNode.GetSlot(addressBits)];
        } while (currentNode != null && currentNode.NetworkBits == (addressBits & currentNode.NetworkMaskBits));

        if (lastValueNode != null)
        {
            result = (TValue)lastValueNode.ValuePair;
            return true;
        }

        result = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetMatch(Network address, [NotNullWhen(true)] out TValue? result)
    {
        Node? lastValueNode = null;

        var currentNode = GetRoot(address);
        var addressBits = address.NetworkBits;

        do
        {
            if (currentNode.HasValue)
                lastValueNode = currentNode;

            currentNode = currentNode.Children[currentNode.GetSlot(addressBits)];
        } while (currentNode != null && currentNode.NetworkBits == (addressBits & currentNode.NetworkMaskBits) && currentNode.Prefix <= address.Prefix);

        if (lastValueNode != null)
        {
            result = (TValue)lastValueNode.ValuePair;
            return true;
        }

        result = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetMatches(IPAddress ipAddress, out IList<TValue> result)
    {
        result = new List<TValue>();

        var currentNode = GetRoot(ipAddress);
        var addressBits = ipAddress.ToUInt128();

        do
        {
            if (currentNode.HasValue)
                result.Add((TValue)currentNode.ValuePair);

            var childSlot = currentNode.GetSlot(addressBits);
            currentNode = currentNode.Children[childSlot];
        } while (currentNode != null && currentNode.ValuePair.Network.NetworkBits == (addressBits & currentNode.ValuePair.Network.NetworkMaskBits));


        return result.Count > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetMatches(Network address, out IList<TValue> result)
    {
        result = new List<TValue>();

        var currentNode = GetRoot(address);
        var addressBits = address.NetworkBits;

        do
        {
            if (currentNode.HasValue)
                result.Add((TValue)currentNode.ValuePair);

            var childSlot = currentNode.GetSlot(addressBits);
            currentNode = currentNode.Children[childSlot];
        } while (currentNode != null && currentNode.ValuePair.Network.NetworkBits == (addressBits & currentNode.ValuePair.Network.NetworkMaskBits) && currentNode.ValuePair.Network.Prefix <= address.Prefix);

        return result.Count > 0;
    }

    public virtual bool TryRemove(Network network)
    {
        var nodeHistory = WalkTree(network);

        // check to see if the requested network exists
        var nodeToRemove = nodeHistory[^1];
        if (nodeToRemove.Node.ValuePair.Network != network)
            return false;

        // set the node to remove as routing node
        // clear the value to avoid holding a reference
        nodeToRemove.Node.HasValue = false;
        nodeToRemove.Node.ValuePair = new NetworkKey(nodeToRemove.Node.ValuePair.Network);

        // walk up the tree collapsing dead routing nodes
        for (var index = nodeHistory.Count - 1; index >= 1; index--)
        {
            var currentNode = nodeHistory[index];

            // if the node is a value node skip it
            if (currentNode.Node.HasValue)
                continue;

            var childCount = (currentNode.Node.Children[0] == null ? 0 : 1) + (currentNode.Node.Children[1] == null ? 0 : 1);

            // if the node has no children unlink it
            if (childCount == 0)
            {
                var parent = nodeHistory[index - 1];
                parent.Node.Children[currentNode.ParentSlot] = null;
                continue;
            }

            // if the node has one child, remove the node and splice its child to its parent
            if (childCount == 1)
            {
                var parent = nodeHistory[index - 1];
                var newChildSlot = currentNode.ParentSlot;
                var childSlot = currentNode.Node.Children[0] != null ? 0 : 1;
                var child = currentNode.Node.Children[childSlot];
                parent.Node.Children[newChildSlot] = child;
                continue;
            }

            // once we hit a routing node that does not require pruning, there is no need to go any further
            Count--;
            return true;
        }

        // once we hit the root, the tree has been pruned
        Count--;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<NodeHistory> WalkTree(Network network)
    {
        var networkBits = network.NetworkBits;
        var root = GetRoot(network);
        NodeHistory lastNode = new(root, root.GetSlot(networkBits));

        var nodeHistory = new List<NodeHistory> { lastNode };

        // walk the tree until...
        // the current node has a more specific prefix (larger) or
        // the current node does not have a child set in the child slot
        // record each node that is visited in a ring buffer

        do
        {
            var nextNodeSlot = lastNode.Node.GetSlot(networkBits);
            var nextNode = lastNode.Node.Children[nextNodeSlot];

            if (nextNode == null || nextNode.Prefix > network.Prefix || (networkBits & nextNode.NetworkMaskBits) != nextNode.NetworkBits)
                break;

            lastNode = new NodeHistory(nextNode, nextNodeSlot);
            nodeHistory.Add(lastNode);

            // continue until the current node has a larger mask or the current node has no children
        } while (true);

        return nodeHistory;
    }

    private readonly struct TreeNode
    {
        internal readonly Node Node;
        internal readonly int Level;
        internal readonly int Index;
        internal int ChildCount => (Node.Children[0] == null ? 0 : 1) + (Node.Children[1] == null ? 0 : 1);

        internal TreeNode(Node node, int level, int index)
        {
            Node = node;
            Level = level;
            Index = index;
        }
    }

    [DebuggerDisplay("{Node.ValuePair.Network.ToString(),nq}")]
    private struct NodeHistory
    {
        public Node Node;
        public readonly int ParentSlot;

        public NodeHistory(Node node, int parentSlot)
        {
            Node = node;
            ParentSlot = parentSlot;
        }
    }

    [DebuggerDisplay("{ValuePair.Network.ToString(),nq}")]
    private sealed class Node
    {
        private NetworkKey _valuePair;
        public ushort AddressLength;

        public UInt128 NetworkBits;
        public UInt128 NetworkMaskBits;
        public ushort Prefix;
        public UInt128 SplitMask;
        public Node[] Children { get; } = new Node[2];
        public bool HasValue { get; internal set; } // TODO: rename this

        public NetworkKey ValuePair
        {
            get => _valuePair;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                _valuePair = value;

                // copy these fields to improve performance
                NetworkBits = _valuePair.Network.NetworkBits;
                NetworkMaskBits = _valuePair.Network.NetworkMaskBits;
                Prefix = (ushort)_valuePair.Network.Prefix;
                AddressLength = _valuePair.Network.AddressLength;

                SplitMask = UInt128.One << AddressLength - Prefix - 1;
            }
        }

        public Node(TValue valuePair, bool hasValue)
        {
            ValuePair = valuePair;
            HasValue = hasValue;
        }

        public Node(Network network)
        {
            ValuePair = new NetworkKey(network);
            HasValue = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSlot(UInt128 networkBits)
        {
            // calculate the child slot
            // if the child is in the first half of the parents address space, use slot 0
            // otherwise use slot 1
            // TODO: might be faster to do (max - min) /2. Could have a static lookup table for the number of addresses for a given prefix
            // that so you would do something like networkBits + (sizeLookup[prefixSize] / 2) 
            return (networkBits & SplitMask) == UInt128.Zero ? 0 : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSlot(Network network)
        {
            return GetSlot(network.NetworkBits);
        }
    }
}