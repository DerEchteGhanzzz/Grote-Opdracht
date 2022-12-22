using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Xml.Linq;
using GroteOpdracht;

namespace GroteOpdracht;

public interface Action
{
    public bool IsPossible(Solution s);
    public void Act(Solution s, bool changeVisitIndex = true); //TODO maak dit niet meer lelijk
    public float GetScoreDelta();
}

public class AddSingle : Action
{
    int dayIndex;
    int truckIndex;
    int tripIndex;
    int nodeIndex;
    int newOrderIndex;
    private bool makeTrip = false;

    private float scoreDelta;
    private float timeDelta;

    public float GetScoreDelta()
    {
        // the scoredelta is calculated per node
        return scoreDelta;
    }
    public AddSingle(Solution s, int newOrderIndex)
    {
        dayIndex = Program.random.Next(0, 5);
        truckIndex = Program.random.Next(0, 2);
        this.newOrderIndex = newOrderIndex;
        Day day = s.Days[dayIndex];
        GenerateTripNodeIndices(day);
    }
    
    public AddSingle(Solution s, int newOrderIndex, int dayIndex)
    {
        this.dayIndex = dayIndex;
        truckIndex = Program.random.Next(0, 2);
        this.newOrderIndex = newOrderIndex;
        Day day = s.Days[dayIndex];
        GenerateTripNodeIndices(day);
    }

    private void GenerateTripNodeIndices(Day day)
    {
        tripIndex = day.TripCount[truckIndex] > 0 ? Program.random.Next(0, day.TripCount[truckIndex]) : 0;
        if (day.TripCount[truckIndex] == 0)
        {
            nodeIndex = 0;
            return;
        }
        nodeIndex = day.Schedules[truckIndex, tripIndex].NodeCount > 0 ? Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount) : 0;
    }

    public bool IsPossible(Solution s)
    {
        Order newOrder = Program.Orders[newOrderIndex];
        
        Day day = s.Days[dayIndex];

        Trip trip = day.Schedules[truckIndex, tripIndex];
        
        if (day.TripCount[truckIndex] == 0 || trip.TotalTrashAmount + newOrder.TrashVolume > Program.TruckVolume)
        {
            //Console.WriteLine(Program.DepotID.ToString() + " " + newOrder.MatrixID.ToString());
            float newTripTime = Program.TimeMatrix[Program.DepotID, newOrder.MatrixID] +
                                newOrder.EmptyTime +
                                Program.TimeMatrix[newOrder.MatrixID, Program.DepotID] + 30 * 60;
            
            if (day.TruckTimes[truckIndex] + newTripTime > Program.TimePerDay) //check if we can make a new trip
            {
                return false;
            }
            //Console.WriteLine(Program.TimeMatrix[Program.DepotID, newOrder.MatrixID] + " :: " + newOrder.EmptyTime + " :: " + Program.TimeMatrix[newOrder.MatrixID, Program.DepotID]);
            //Console.WriteLine(newTripTime);
            
            makeTrip = true;
            timeDelta = newTripTime;
            scoreDelta = timeDelta - newOrder.PenaltyPerVisit;
            
            //Console.WriteLine(scoreDelta);
            return true;
        }
        
        Node node = trip.Nodes[nodeIndex];
        int nextOrderID = node.Next is null ? Program.DepotID : node.Next.Order.MatrixID;   // check if the next order exists
        
        float currentTime = Program.TimeMatrix[node.Order.MatrixID, nextOrderID];
        float futureTime = Program.TimeMatrix[node.Order.MatrixID, newOrder.MatrixID] +
                           newOrder.EmptyTime +
                           Program.TimeMatrix[newOrder.MatrixID, nextOrderID];
        timeDelta = futureTime - currentTime;
        scoreDelta = timeDelta - newOrder.PenaltyPerVisit;
        
        return day.TruckTimes[truckIndex] + timeDelta < Program.TimePerDay;
    }

    public void Act(Solution s, bool changeVisitIndex = true)
    {
        if (changeVisitIndex)
        {
            // Decrement the amount of orders that are not visited.
            // Swap the last of the not visited orders with the new added order.
            Program.NotVisitedAmount--;
            //Console.WriteLine((Program.Orders[Program.NotVisitedAmount], Program.Orders[newOrderIndex]));
            (Program.Orders[Program.NotVisitedAmount], Program.Orders[newOrderIndex]) = (Program.Orders[newOrderIndex], Program.Orders[Program.NotVisitedAmount]);
            //Console.WriteLine((Program.Orders[Program.NotVisitedAmount], Program.Orders[newOrderIndex]));
        }
        
        Day day = s.Days[dayIndex];
        if (makeTrip)
        {
            // because the order is now at the back of the orders list, the NotVisitedAmount will be its index
            day.CreateTrip(Program.Orders[Program.NotVisitedAmount], truckIndex, timeDelta);
        }
        else
        {
            // because the order is now at the back of the orders list, the NotVisitedAmount will be its index
            day.AddToSchedule((int)day.Today, truckIndex, tripIndex, nodeIndex, timeDelta, Program.NotVisitedAmount);
        }
    }
}

public class AddTwo : Action
{
    int dayIndex = Program.random.Next(0, 2);
    private AddSingle addDayOne;
    private AddSingle addDayTwo;
    
    public AddTwo(Solution s, int newOrderIndex)
    {
        addDayOne = new AddSingle(s, newOrderIndex, dayIndex);
        addDayTwo = new AddSingle(s, newOrderIndex, dayIndex + 3);
    }
    
    public float GetScoreDelta()
    {
        return addDayOne.GetScoreDelta() + addDayTwo.GetScoreDelta();
    }

    public bool IsPossible(Solution s)
    {
        return addDayOne.IsPossible(s) && addDayTwo.IsPossible(s);
    }

    public void Act(Solution s, bool changeVisitAmount = true)
    {
        addDayOne.Act(s, true);
        addDayTwo.Act(s, false);
    }
}

public class AddThree : Action
{
    private AddSingle addDayOne;
    private AddSingle addDayTwo;
    private AddSingle addDayThree;
    
    public AddThree(Solution s, int newOrderIndex)
    {
        addDayOne = new AddSingle(s, newOrderIndex, 0);
        addDayTwo = new AddSingle(s, newOrderIndex, 2);
        addDayThree = new AddSingle(s, newOrderIndex, 4);
    }
    
    public float GetScoreDelta()
    {
        return addDayOne.GetScoreDelta() + addDayTwo.GetScoreDelta() + addDayThree.GetScoreDelta();
    }

    public bool IsPossible(Solution s)
    {
        return addDayOne.IsPossible(s) && addDayTwo.IsPossible(s) && addDayThree.IsPossible(s);
    }

    public void Act(Solution s, bool changeVisitAmount = true)
    {
        addDayOne.Act(s, changeVisitAmount);
        addDayTwo.Act(s, false);
        addDayThree.Act(s, false);
    }
}

public class AddFour : Action
{
    private AddSingle[] ActionArray = new AddSingle[4];
    
    public AddFour(Solution s, int newOrderIndex)
    {
        int excludeDay = Program.random.Next(0, 5);
        int index = 0;
        for (int i = 0; i < 5; i++)
        {
            if (i == excludeDay)
            {
                continue;
            }
            //Console.WriteLine(index + " " + i);
            ActionArray[index] = new AddSingle(s, newOrderIndex, i);
            index++;
        }
    }
    
    public float GetScoreDelta()
    {
        float sum = 0;
        foreach (AddSingle action in ActionArray)
        {
            sum += action.GetScoreDelta();
        }
        return sum;
    }

    public bool IsPossible(Solution s)
    {
        foreach (AddSingle action in ActionArray)
        {
            if (!action.IsPossible(s))
            {
                return false;
            }
        }
        return true;
    }

    public void Act(Solution s, bool changeVisitAmount = true)
    {
        foreach (AddSingle action in ActionArray)
        {
            action.Act(s, changeVisitAmount);
            changeVisitAmount = false;
        }
    }
}

public class RemoveAction : Action
{
    
    readonly int orderIndex;
    private readonly float[] timeDeltas;
    private readonly float[] scoreDeltas;
    
    public RemoveAction(Solution s, int orderIndex)
    {
        scoreDeltas = new float[(int)Program.Orders[orderIndex].Frequency];
        timeDeltas = new float[(int)Program.Orders[orderIndex].Frequency];
        this.orderIndex = orderIndex;
    }

    public bool IsPossible(Solution s)
    {
        Node[] referenceNodeArray = Program.Orders[orderIndex].NodeLookupArray;
        if (referenceNodeArray[0] is null)
            Console.WriteLine(string.Format("IsPossible Remove on orderIndex {0}", orderIndex));
        for (int index = 0; index < scoreDeltas.Length; index++)
        {

            Day day = s.Days[referenceNodeArray[index].DayIndex];
            Trip trip = day.Schedules[referenceNodeArray[index].Truck, referenceNodeArray[index].TripIndex];
            Node orderNode = trip.Nodes[referenceNodeArray[index].NodeIndex];
            // calculate the difference in scores if the node is removed

            int nextID = orderNode.Next is null ? Program.DepotID : orderNode.Next.Order.MatrixID;
            int prevID = orderNode.Prev.Order.MatrixID;

            timeDeltas[index] = Program.TimeMatrix[prevID, nextID] -
                                (Program.TimeMatrix[prevID, orderNode.Order.MatrixID] +
                                 Program.TimeMatrix[orderNode.Order.MatrixID, nextID] +
                                 orderNode.Order.EmptyTime);

            // add penalty per visit to the score
            scoreDeltas[index] = timeDeltas[index] + orderNode.Order.PenaltyPerVisit;

        }

        return true;
    }

    public void Act(Solution s, bool changeVisitAmount = true)
    {
        Node[] reference = Program.Orders[orderIndex].NodeLookupArray;
        
        for (int index = 0; index < reference.GetLength(0); index++)
        {
            Day day = s.Days[reference[index].DayIndex];
            Trip trip = day.Schedules[reference[index].Truck, reference[index].TripIndex];
            
            // remove the node from the trip of the truck of the day
            day.RemoveNodeFromTrip(reference[index].Truck, trip, reference[index], timeDeltas[index]);
        }

        // This node now gets swapped with the last node in the list, so it now belongs to the orders that are not yet used
        (Program.Orders[orderIndex], Program.Orders[Program.NotVisitedAmount]) = (
            Program.Orders[Program.NotVisitedAmount], Program.Orders[orderIndex]);
        // this gets updated so the new node now belongs to the not visited nodes
        Program.NotVisitedAmount++;
        
    }

    public float GetScoreDelta()
    {
        return scoreDeltas.Sum();
    }
}

public class ShiftTripAction : Action
{
    int dayIndex;
    int truckIndex;
    int tripIndex;
    int nodeIndexX;
    int nodeIndexY;

    private float timeDelta;
    public bool IsPossible(Solution s) {
        dayIndex = Program.random.Next(0, 5);
        truckIndex = Program.random.Next(0, 2);
        Day day = s.Days[dayIndex];
        if (!GenerateTripNodeIndices(day))
            return false;

        Trip trip = day.Schedules[truckIndex, tripIndex];
        Node nodeX2 = trip.Nodes[nodeIndexX];
        int x3 = nodeX2.Next is null ? Program.DepotID : nodeX2.Next.Order.MatrixID;
        int x2 = nodeX2.Order.MatrixID;
        int x1 = nodeX2.Prev.Order.MatrixID;
        Node nodeY1 = trip.Nodes[nodeIndexY];
        int y1 = nodeY1.Order.MatrixID;
        int y2 = nodeY1.Next is null ? Program.DepotID : nodeY1.Next.Order.MatrixID;
        if (x1 == y1)
            return false;
        float costNow = Program.TimeMatrix[x1, x2] +
            Program.TimeMatrix[x2, x3] +
            Program.TimeMatrix[y1, y2];

        float costFuture = Program.TimeMatrix[x1, x3] +
            Program.TimeMatrix[y1, x2] +
            Program.TimeMatrix[x2, y2];

        timeDelta = costFuture - costNow;

        return (day.TruckTimes[truckIndex] + timeDelta <= Program.TimePerDay);
    }

    private bool GenerateTripNodeIndices(Day day)
    {
        tripIndex = day.TripCount[truckIndex] > 0 ? Program.random.Next(0, day.TripCount[truckIndex]) : 0;
        if (day.Schedules[truckIndex, tripIndex].NodeCount <= 1)
        {
            return false;
        }
        nodeIndexX = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount-1);
        nodeIndexY = Program.random.Next(nodeIndexX+1, day.Schedules[truckIndex, tripIndex].NodeCount);
        return true;
    }
    public void Act(Solution s, bool _)
    {
        Day day = s.Days[dayIndex];
        Node nodeX2 = day.Schedules[truckIndex, tripIndex].Nodes[nodeIndexX];
        Node nodeY1 = day.Schedules[truckIndex, tripIndex].Nodes[nodeIndexY];

        nodeX2.TwoHalfOpt(nodeY1);

        day.TruckTimes[truckIndex] += timeDelta;
    }

    public float GetScoreDelta()
    {
        return timeDelta;
    }
}

public class ShiftDayAction : Action
{

    protected int dayIndexX;
    protected int dayIndexY;

    protected int truckIndexX;
    protected int truckIndexY;

    protected int tripIndexX;
    protected int tripIndexY;

    protected int nodeIndexX;
    protected int nodeIndexY;

    protected float timeDeltaX;
    protected float timeDeltaY;

    public virtual bool IsPossible(Solution s)
    {
        dayIndexX = Program.random.Next(0, 4);
        dayIndexY = Program.random.Next(dayIndexX, 5);

        truckIndexX = Program.random.Next(0, 2);
        truckIndexY = Program.random.Next(0, 2);

        Day dayX = s.Days[dayIndexX];
        Day dayY = s.Days[dayIndexY];
        if (!GenerateTripNodeIndices(dayX, true) || !GenerateTripNodeIndices(dayY, false))
            return false;

        Trip tripX = dayX.Schedules[truckIndexX, tripIndexX];
        Node nodeX2 = tripX.Nodes[nodeIndexX];
        Trip tripY = dayY.Schedules[truckIndexY, tripIndexY];
        Node nodeY1 = tripY.Nodes[nodeIndexY];

        if(nodeX2.Order.Frequency != VisitAmount.Once || nodeY1.Order.Frequency != VisitAmount.Once)
            return false;

        int x3 = nodeX2.Next is null ? Program.DepotID : nodeX2.Next.Order.MatrixID;
        int x2 = nodeX2.Order.MatrixID;
        int x1 = nodeX2.Prev.Order.MatrixID;

        int y1 = nodeY1.Order.MatrixID;
        int y2 = nodeY1.Next is null ? Program.DepotID : nodeY1.Next.Order.MatrixID;

        float costNowX = Program.TimeMatrix[x1, x2] +
            Program.TimeMatrix[x2, x3];

        float costNowY =Program.TimeMatrix[y1, y2];


        float costFutureX = Program.TimeMatrix[x1, x3] - nodeX2.Order.EmptyTime;

        float costFutureY = Program.TimeMatrix[y1, x2] +
            Program.TimeMatrix[x2, y2] +
            nodeX2.Order.EmptyTime;

        timeDeltaX = costFutureX - costNowX;
        timeDeltaY = costFutureY - costNowY;

        return (dayX.TruckTimes[truckIndexX] + timeDeltaX <= Program.TimePerDay &&
            dayY.TruckTimes[truckIndexY] +timeDeltaY <= Program.TimePerDay);
    }

    protected bool GenerateTripNodeIndices(Day day, bool isFirst)
    {
        int truckIndex = truckIndexY;
        if (isFirst)
            truckIndex = truckIndexX;

        int tripIndex = day.TripCount[truckIndex] > 0 ? Program.random.Next(0, day.TripCount[truckIndex]) : 0;
        if (day.Schedules[truckIndex, tripIndex] is null || day.Schedules[truckIndex, tripIndex].NodeCount <= 0)
        {
            return false;
        }
        
        if(isFirst)
        {
            tripIndexX = tripIndex;
            nodeIndexX = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount);
            return true;
        }
        tripIndexY = tripIndex;
        nodeIndexY = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount);
        return true;
    }

    public void Act(Solution s, bool changeVisitIndex)
    {

        Day dayX = s.Days[dayIndexX];
        Day dayY = s.Days[dayIndexY];
        Node nodeX2 = dayX.Schedules[truckIndexX, tripIndexX].Nodes[nodeIndexX];

        dayX.RemoveNodeFromTrip(truckIndexX, dayX.Schedules[truckIndexX, tripIndexX], nodeX2, timeDeltaX);
        dayY.AddToSchedule(dayIndexY, truckIndexY, tripIndexY, nodeIndexY, timeDeltaY, nodeX2.Order);
    }

    public float GetScoreDelta()
    {
        return (timeDeltaX + timeDeltaY);
    }
}

public class ShiftTruckAction : ShiftDayAction
{
    public override bool IsPossible(Solution s)
    {
        dayIndexX = Program.random.Next(0, 5);
        dayIndexY = dayIndexX;

        truckIndexX = 0;
        truckIndexY = 1;

        Day dayX = s.Days[dayIndexX];
        Day dayY = s.Days[dayIndexY];
        if (!GenerateTripNodeIndices(dayX, true) || !GenerateTripNodeIndices(dayY, false))
            return false;

        Trip tripX = dayX.Schedules[truckIndexX, tripIndexX];
        Node nodeX2 = tripX.Nodes[nodeIndexX];
        Trip tripY = dayY.Schedules[truckIndexY, tripIndexY];
        Node nodeY1 = tripY.Nodes[nodeIndexY];

        int x3 = nodeX2.Next is null ? Program.DepotID : nodeX2.Next.Order.MatrixID;
        int x2 = nodeX2.Order.MatrixID;
        int x1 = nodeX2.Prev.Order.MatrixID;

        int y1 = nodeY1.Order.MatrixID;
        int y2 = nodeY1.Next is null ? Program.DepotID : nodeY1.Next.Order.MatrixID;

        float costNowX = Program.TimeMatrix[x1, x2] +
            Program.TimeMatrix[x2, x3];

        float costNowY = Program.TimeMatrix[y1, y2];


        float costFutureX = Program.TimeMatrix[x1, x3] - nodeX2.Order.EmptyTime;

        float costFutureY = Program.TimeMatrix[y1, x2] +
            Program.TimeMatrix[x2, y2] +
            nodeX2.Order.EmptyTime;

        timeDeltaX = costFutureX - costNowX;
        timeDeltaY = costFutureY - costNowY;

        return (dayX.TruckTimes[truckIndexX] + timeDeltaX <= Program.TimePerDay &&
            dayY.TruckTimes[truckIndexY] + timeDeltaY <= Program.TimePerDay);
    }
}