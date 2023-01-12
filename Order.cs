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

    public Node[] NodeLookupArray;
    private int nodesAssigned;
    public float PenaltyPerVisit
    {
        get { return 3 * EmptyTime; }   // strafkosten (deze worden later vermenigvuldigd met hoe vaak we per week niet kwamen opdagen)
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
    }

    public void AssignNode(Node node)
    {
        // De nodeLoopupArray is om meerdere nodes aan een order te koppelen. Zo kan een order op meerdere dagen worden gedaan
        NodeLookupArray[nodesAssigned] = node;
        nodesAssigned++;
    }
    
    public void ClearNodes()
    {
        // reset nodes
        if (nodesAssigned == 0) return; // nodes already reset
        nodesAssigned = 0;
        NodeLookupArray = new Node[(int)Frequency]; // lengte aantal keren dat je er langs moet.
    }

    public override string ToString()
    {
        return OrderID.ToString();
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