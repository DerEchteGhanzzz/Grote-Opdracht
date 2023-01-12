using System;
using System.Data;
using GroteOpdracht;

namespace GroteOpdracht;

public class Trip
{
    public readonly Node Depot;
    public Node[] Nodes = new Node[Program.Orders.Length];
    public int NodeCount { get; private set; }
    public readonly int truck;
    public int TotalTrashAmount;

    public Day AssignedDay;
    public Trip(Order start, int dayIndex, int truck, int tripIndex)
    {
        NodeCount = 0;
        // dit is het depot. Het depot is er zodat er een eerste node kan worden toegevoegd aan de trip
        Depot = new Node(new Order(0, 0, 0, 0, Program.DepotID, 0), -1, -1, -1, -1);
        this.truck = truck;
        AddOrder(start, dayIndex, tripIndex, 0);
    }
    
    public void AddOrder(Order order, int dayIndex, int tripIndex, int nodeIndex)
    {
        // pak de juiste node
        Node oldNode = Nodes[nodeIndex];
        if (NodeCount == 0)
        {
            // we beginnen met het depot, want er zijn nog geen andere nodes
            oldNode = Depot;
        }
        // insert de nieuwe order
        oldNode.InsertNew(order, dayIndex, truck, tripIndex, NodeCount);
        if (oldNode.Next is null)
        {
            Console.WriteLine("AddOrder failed in Trip"); // error
            return;
        }
        Nodes[NodeCount] = oldNode.Next;
        TotalTrashAmount += order.TrashVolume;
        NodeCount++;
    }
    
    // if true, we can delete the trip.
    public bool RemoveNode(int nodeIndex)
    {
        // we halen een node weg
        NodeCount--;
        // we halen het afval weg
        TotalTrashAmount -= Nodes[nodeIndex].Order.TrashVolume;
        
        // node has to be uncoupled from its order regardless of the trip being empty
        Nodes[nodeIndex].Delete();
        if (NodeCount == 0)
        {
            // return true van: we gaan deze trip nu verwijderen, omdat hij leeg is = true
            return true;
        }
        
        // wissen de nodes, zodat de node die achteraan stond nu weer goed staat
        (Nodes[nodeIndex], Nodes[NodeCount]) = (Nodes[NodeCount], Nodes[nodeIndex]);
        Nodes[nodeIndex].NodeIndex = nodeIndex; // update de nodeIndices die de nodes bijhouden
        Nodes[NodeCount].NodeIndex = NodeCount;
        return false;   // we hoeven de trip niet te verwijderen
    }
}