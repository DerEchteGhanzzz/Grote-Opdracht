namespace GroteOpdracht;

public class Order
{
    public readonly VisitAmount Frequency;
    public readonly float EmptyTime;
    public readonly int TrashVolume;
    public readonly int MatrixID;
    public readonly int OrderID;

    public float Penalty
    {
        get { return (int)Frequency * 3 * EmptyTime; }
    }

    public Order(int frequency, float emptyTime, int garbagePerContainer, int containerCount, int matrixID, int orderID)
    {
        Frequency = (VisitAmount)(frequency);
        EmptyTime = emptyTime;
        TrashVolume = garbagePerContainer * containerCount;
        MatrixID = matrixID;
        OrderID = orderID;
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