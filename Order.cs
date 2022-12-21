using System;
using System.Net.NetworkInformation;

namespace GroteOpdracht;

public class Order
{
    public readonly VisitAmount Frequency;
    public readonly float EmptyTime;
    public readonly int TrashVolume;
    public readonly int MatrixID;
    public readonly int OrderID;

    private bool added;

    public Node[] NodeLookupArray;
    private int nodesAssigned;
    public float PenaltyPerVisit
    {
        get { return 3 * EmptyTime; }
    }

    public Order(int frequency, float emptyTime, int garbagePerContainer, int containerCount, int matrixID, int orderID)
    {
        Frequency = (VisitAmount)(frequency);
        EmptyTime = emptyTime;
        TrashVolume = garbagePerContainer * containerCount;
        MatrixID = matrixID;
        OrderID = orderID;
        nodesAssigned = 0;
        NodeLookupArray = new Node[(int)Frequency]; // lengte aantal keren dat je er langs moet.
        added = false;
    }

    public void AssignNode(Node node)
    {

        try
        {
            // assign Nodes to the order.
            NodeLookupArray[nodesAssigned] = node;
            nodesAssigned++;
            added = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(nodesAssigned + " " + NodeLookupArray.Length + " " + Frequency + " " + OrderID + " " + added);
        }
    }
    
    public void ClearNodes()
    {
        // reset nodes
        nodesAssigned = 0;
        NodeLookupArray = new Node[(int)Frequency];
        added = false;
    }

    public override string ToString()
    {
        return OrderID.ToString() + " " + Frequency + " " + EmptyTime + " " + TrashVolume;
    }
}

public enum VisitAmount
{
    None,
    Once,
    Twice,
    Thrice,
    Quadruple
}