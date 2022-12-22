using System.Security.Cryptography.X509Certificates;

namespace GroteOpdracht;

public class Node
{
    public Node Prev;
    public Node? Next;
    public Order Order;

    public int DayIndex;
    public int Truck;
    public int TripIndex;
    public int NodeIndex;
    public Node(Order order, int dayIndex, int truck, int tripIndex, int nodeIndex)
    {
        this.Order = order;
        NodeIndex = nodeIndex;
        DayIndex = dayIndex;
        Truck = truck;
        TripIndex = tripIndex;
        if (order.OrderID != 0)
        {
            order.AssignNode(this);
        }
    }
    public void Delete()
    {
        
        Prev.Next = Next;
        if (Next is not null) Next.Prev = Prev; // check if the next or next node even exists
        
        Order.ClearNodes(); // order resets the nodeCount because all nodes will get removed
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

    public void TwoHalfOpt(Node Y1)
    {
        this.Prev.Next = this.Next;
        this.Next = this.Prev;
        if(Y1.Next is not null)
            Y1.Next.Prev = this;
        this.Next = Y1.Next;

        Y1.Next = this;
        this.Prev = Y1;
    }
}