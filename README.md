![MATRIX Server](http://i.imgur.com/m5wb1K0.png)

A .net server system designed to support a network of servers with easily updatable drop-in components.

## The Network

A MATRIX network consists of three parts: a single **Master**, and many **Hosts**. The master is the entrypoint to the matrix, and is responsible for commanding all of the connected **Hosts** and routing messages between them. 

**Nodes** are instances of compiled .net classes (in library format) that are both synchronized around the network in binary form, loaded at runtime into a sandboxed environment, and then launched on command by the master node running on the master server. 

The Matrix environment virtualizes the communication between nodes and the management of node instances. It allows you to seamlessly scale over an arbitrary number of **Hosts** and write code that communicates with other nodes as if they were in the same process. Hosts can drop out or be added in, and the node network will be seamlessly distributed over the matrix.

## A Node

Nodes are contained in .net libraries (generally c# dlls). You can have as many nodes as you want in a library. Nodes always derive from `INode` and their RMI interface. The RMI interface always derives from `IRMIInterface`.

The RMI interface is a C# interface that contains all of the remote method invocations you would like in your node. It is essentially the public facing interface to your node code. For example:

### `IMyNode`

```
public interface IMyNode : IRMIInterface
{
	public string DoSomething(int myInt);
}
```

###  `MyNode`

```
public class MyNode : INode, IMyNode
{
    //Override the base INode methods
    public string DoSomething(int myInt)
    {
        return "Hi: "+myInt;
    }
}
```

### Some Other Location

```
var myNode = portal.GetNodeProxy<IMyNode>();
var result = myNode.DoSomething(5);
```

Result would be populated with the result of the request, made over the network completely synchronously.
