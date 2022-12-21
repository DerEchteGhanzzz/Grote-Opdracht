using System.Data;
using GroteOpdracht;

namespace GroteOpdracht;

public class Trip
{
    public Node Start;
    public Node Depot;
    public Node[] Nodes = new Node[Program.Orders.Length];
    public int NodeCount = 0;
    public int truck;
    public int TotalTrashAmount;

    public Day AssignedDay;
    public Trip(Order start, int dayIndex, int truck, int tripIndex)
    {
        this.Depot = new Node(new Order(0, 0, 0, 0, Program.DepotID, 0), -1, -1, -1, -1);
        TotalTrashAmount = start.TrashVolume;
        AddOrder(start, dayIndex, truck, tripIndex, 0);
        this.truck = truck;
    }
    
    public void AddOrder(Order order, int dayIndex, int truck, int tripIndex, int nodeIndex)
    {
        Node oldNode = Nodes[nodeIndex];
        if (NodeCount == 0)
        {
            oldNode = Depot;
        }
        oldNode.InsertNew(order, dayIndex, truck, tripIndex, NodeCount);
        Nodes[NodeCount] = oldNode.Next;
        TotalTrashAmount += order.TrashVolume;
        NodeCount++;
    }
    
    // if true, we can delete the trip.
    public bool RemoveNode(int nodeIndex)
    {
        NodeCount--;

        TotalTrashAmount -= Nodes[nodeIndex].Order.TrashVolume;
        
        // node has to be uncoupled from its order regardless of the trip being empty
        Nodes[nodeIndex].Delete();
        if (NodeCount == 0)
        {
            return true;
        }
        
        (Nodes[nodeIndex], Nodes[NodeCount]) = (Nodes[NodeCount], Nodes[nodeIndex]);
        Nodes[nodeIndex].NodeIndex = nodeIndex;
        Nodes[NodeCount].NodeIndex = NodeCount;
        return false;
    }
}