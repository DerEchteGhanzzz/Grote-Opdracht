namespace GroteOpdracht;

public class Node
{
    public Node Prev;
    public Node Next;
    public Order Order;
    
    public int NodeIndex;
    public Node(Order order, int dayIndex, int truck, int tripIndex, int nodeIndex)
    {
        this.Order = order;
        if (nodeIndex != -1)
            order.AssignNode(dayIndex, truck, tripIndex, nodeIndex);
    }
    public void Delete()
    {
        
        Prev.Next = Next;
        if (Next is not null) Next.Prev = Prev; // check if the next or next node even exists
        
        Order.ResetAssignedNodes(); // order only lowers the nodeCount because all nodes will get removed
    }

    public void InsertNew(Order newOrder, int dayIndex, int truck, int tripIndex, int nodeIndex)
    {
        // the new node needs to know its position
        Node newNode = new Node(newOrder, dayIndex, truck, tripIndex, nodeIndex);

        if (this.Next is not null)
        {
            newNode.Next = this.Next;
            newNode.Next.Prev = newNode;
        }
        
        this.Next = newNode;
        newNode.Prev = this;
        
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