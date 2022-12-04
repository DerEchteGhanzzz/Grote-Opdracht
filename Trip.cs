using System.Data;

namespace GroteOpdracht;

public class Trip
{
    public Day Day;
    public Node Start;
    public Node Depot = new Node(new Order(0, 30, 0, 0, Program.DepotID, 0), null, null, -1);
    public Node[] Nodes = new Node[Program.Orders.Length];
    private int nodeCount = 0;
    private float timeSpent;
    public int truck;
    public int scheduleIndex;
    public float TimeSpent
    {
        get { return timeSpent + 30; }
        set { timeSpent = value; }
    }
    public int TotalTrashAmount;

    public Trip(Order start, float timeSpent, int truck, int scheduleIndex)
    {
        Start = new Node(start, Depot, Depot, nodeCount);
        this.timeSpent = timeSpent;
        TotalTrashAmount = start.TrashVolume;
        AddNode(Start, Program.TimeMatrix[Program.DepotID, start.MatrixID] + 
                       start.EmptyTime +
                       Program.TimeMatrix[start.MatrixID, Program.DepotID]);

        this.truck = truck;
        this.scheduleIndex = scheduleIndex;
    }

    private void AddNode(Node n, float time)
    {
        Nodes[nodeCount] = n;
        nodeCount++;
        timeSpent += time;
    }
    
    private void RemoveNode(int nodesIndex, float time)
    {
        nodeCount--;
        if (nodeCount == 0)
        {
            this.Delete();
            return;
        }
        Node n = Nodes[nodesIndex];
        n.Delete();

        timeSpent -= time;
        
        Nodes[nodesIndex] = Nodes[nodeCount];
    }

    public void Delete()
    {
        this.Day.RemoveFromSchedule(truck, scheduleIndex, TimeSpent);
    }
}