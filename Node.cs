namespace GroteOpdracht;

public class Node
{
    public Node Prev;
    public Node Next;
    public Order Value;
    public int NodeIndex;

    public Node(Order value, Node prev, Node next, int nodeCount)
    {
        this.Value = value;
        if (value.OrderID == Program.DepotID) return;
        this.Prev = prev;
        this.Next = next;
           
        Prev.Next = this;
        Next.Prev = this;
        
        this.NodeIndex = nodeCount;
    }

    public void Delete()
    {
        Prev.Next = Next;
        Next.Prev = Prev;
    }

    public static void TwoHalfOpt(Node X1, Node Y1)
    {
        Node X2 = X1.Next;
        Node Y2 = Y1.Next;

        X1.Next = X2.Next;
        X1.Next.Prev = X1;
        
        Y1.Next = X2;
        X2.Prev = Y1;
        
        X2.Next = Y2; 
        Y2.Prev = X2;
    }
}