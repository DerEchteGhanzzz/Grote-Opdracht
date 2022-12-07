namespace GroteOpdracht;

public class Order
{
    public readonly VisitAmount Frequency;
    public readonly float EmptyTime;
    public readonly int TrashVolume;
    public readonly int MatrixID;
    public readonly int OrderID;

    public int[,] NodeLookupArray;
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
        NodeLookupArray = new int[frequency, 4];
    }

    public void AssignNode(int dayIndex, int truck, int tripIndex, int nodeIndex)
    {
        if (nodesAssigned == NodeLookupArray.GetLength(0))
        {
            Console.WriteLine(NodeLookupArray[nodesAssigned-1, 0]);
        }
        
        NodeLookupArray[nodesAssigned, 0] = dayIndex;
        NodeLookupArray[nodesAssigned, 1] = truck;
        NodeLookupArray[nodesAssigned, 2] = tripIndex;
        NodeLookupArray[nodesAssigned, 3] = nodeIndex;
        nodesAssigned++;
    }
    
    public void ResetAssignedNodes()
    {
        nodesAssigned = 0;
        NodeLookupArray[nodesAssigned, 0] = -1;
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