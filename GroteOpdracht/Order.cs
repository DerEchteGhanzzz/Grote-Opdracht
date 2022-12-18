namespace GroteOpdracht;

public class Order
{
    public readonly VisitAmount Frequency;
    public readonly float EmptyTime;
    public readonly int TrashVolume;
    public readonly int MatrixID;
    public readonly int OrderID;
    private int nodesAssigned;

    public int[,] NodeLookupArray;
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
        NodeLookupArray = new int[frequency, 4];
        nodesAssigned = 0;
    }

    public void AssignNode(int dayIndex, int truck, int tripIndex, int nodeIndex)
    {
        NodeLookupArray[nodesAssigned, 0] = dayIndex;
        NodeLookupArray[nodesAssigned, 1] = truck;
        NodeLookupArray[nodesAssigned, 2] = tripIndex;
        NodeLookupArray[nodesAssigned, 3] = nodeIndex;

        nodesAssigned++;
    }
    
    public void ResetAssignedNodes()
    {
        NodeLookupArray[0, 0] = -1;
        nodesAssigned--;
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
