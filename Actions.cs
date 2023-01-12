using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Xml.Linq;
using GroteOpdracht;

namespace GroteOpdracht;

public interface Action
{
    public bool IsPossible(Solution s);
    public void Act(Solution s, bool changeVisitIndex = true); //TODO maak dit niet meer lelijk
    public float GetScoreDelta();
    public float GetOvertimeReduction(Solution s);
}

public class AddSingle : Action // kijk of een order past in de oplossing en voeg hem toe of niet als hij past
{
    int dayIndex;
    int truckIndex;
    int tripIndex;
    int nodeIndex;
    int newOrderIndex;
    private bool makeTrip = false;

    private float scoreDelta;
    private float timeDelta;

    public bool IsOvertime(Solution s)
    {
        return s.Days[dayIndex].TruckTimes[truckIndex] - Program.TimePerDay + timeDelta > 0;
    }
    
    public float GetOvertimeReduction(Solution s)
    {
        float x = s.Days[dayIndex].TruckTimes[truckIndex];
        return Math.Max(Program.TimePerDay, x + timeDelta) - Math.Max(Program.TimePerDay, x);
    }

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

    private void GenerateTripNodeIndices(Day day)   // pak een node uit een trip
    {
        tripIndex = day.TripCount[truckIndex] > 0 ? Program.random.Next(0, day.TripCount[truckIndex]) : 0;
        if (day.TripCount[truckIndex] == 0)
        {
            nodeIndex = 0;
            return;
        }
        nodeIndex = day.Schedules[truckIndex, tripIndex].NodeCount > 0 ? Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount) : 0;
    }

    public bool IsPossible(Solution s)  // kijk of de actie mogelijk is
    {
        Order newOrder = Program.Orders[newOrderIndex]; // zoek de order op
        
        Day day = s.Days[dayIndex]; // aan welke dag voegen we hem toe

        Trip trip = day.Schedules[truckIndex, tripIndex];   // en aan welke trip
        
        // zit de trip vol? of is er nog geen trip? voeg er een toe
        if (day.TripCount[truckIndex] == 0 || trip.TotalTrashAmount + newOrder.TrashVolume > Program.TruckVolume)
        {
            // hoeveel extra tijd kost dit?
            float newTripTime = Program.TimeMatrix[Program.DepotID, newOrder.MatrixID] +
                                newOrder.EmptyTime +
                                Program.TimeMatrix[newOrder.MatrixID, Program.DepotID] + 30 * 60;
            
            if (day.TruckTimes[truckIndex] + newTripTime > Program.TimePerDay) //check if we can make a new trip
            {
                return false; // niet mogelijk!
            }

            makeTrip = true;
            timeDelta = newTripTime;
            scoreDelta = timeDelta - newOrder.PenaltyPerVisit;
            
            return true;    // mogelijk!
        }
        
        Node node = trip.Nodes[nodeIndex];
        int nextOrderID = node.Next is null ? Program.DepotID : node.Next.Order.MatrixID;   // check if the next order exists
        
        // hoe lang doen we erover? het is het nieuwe pad - het huidige pad + de nieuwestorttijd
        float currentTime = Program.TimeMatrix[node.Order.MatrixID, nextOrderID];
        float futureTime = Program.TimeMatrix[node.Order.MatrixID, newOrder.MatrixID] +
                           newOrder.EmptyTime +
                           Program.TimeMatrix[newOrder.MatrixID, nextOrderID];
        timeDelta = futureTime - currentTime;
        scoreDelta = timeDelta - newOrder.PenaltyPerVisit;
        
        
        
        return true; // return of het wel of niet in de dag past
    }

    public void Act(Solution s, bool changeVisitIndex = true)
    {
        if (changeVisitIndex)
        {
            // Decrement the amount of orders that are not visited.
            // Swap the last of the not visited orders with the new added order.
            Program.NotVisitedAmount--;
            (Program.Orders[Program.NotVisitedAmount], Program.Orders[newOrderIndex]) = (Program.Orders[newOrderIndex], Program.Orders[Program.NotVisitedAmount]);
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

public class AddTwo : Action    // maak 2 add single actions aan voor 2 dagen en plan ze allemaal in
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
    
    public bool IsOvertime(Solution s)
    {
        return addDayOne.IsOvertime(s) || addDayTwo.IsOvertime(s);
    }

    public float GetOvertimeReduction(Solution s)
    {
        return addDayOne.GetOvertimeReduction(s) + addDayTwo.GetOvertimeReduction(s);
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

public class AddThree : Action  // maak 3 add single actions aan voor 3 dagen en plan ze allemaal in
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
    
    public bool IsOvertime(Solution s)
    {
        return addDayOne.IsOvertime(s) || addDayTwo.IsOvertime(s) || addDayThree.IsOvertime(s);
    }

    public float GetOvertimeReduction(Solution s)
    {
        return addDayOne.GetOvertimeReduction(s) + addDayTwo.GetOvertimeReduction(s) + addDayThree.GetOvertimeReduction(s);
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

public class AddFour : Action   // maak 4 add single actions aan voor 4 dagen en plan ze allemaal in
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
    
    public bool IsOvertime(Solution s)
    {
        foreach (AddSingle action in ActionArray)
        {
            if (action.IsOvertime(s))
            {
                return true;
            }
        }

        return false;
    }

    public float GetOvertimeReduction(Solution s)
    {
        float max = 0;
        foreach (AddSingle action in ActionArray)
        {
            max += action.GetOvertimeReduction(s);
            
        }
        return max;
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
public class RemoveAction : Action // TODO bugfixen
{
    
    readonly int orderIndex;
    private readonly float[] timeDeltas;
    private readonly float[] scoreDeltas;
    
    public float GetOvertimeReduction(Solution s)
    {
        return overtime;
    }

    public bool IsOvertime(Solution s)
    {
        return false;   //fix
    }
    
    private float overtime = 0;
    
    public RemoveAction(Solution s, int orderIndex)
    {
        scoreDeltas = new float[(int)Program.Orders[orderIndex].Frequency];
        timeDeltas = new float[(int)Program.Orders[orderIndex].Frequency];
        this.orderIndex = orderIndex;
    }

    public bool IsPossible(Solution s)
    {
        Node[] referenceNodeArray = Program.Orders[orderIndex].NodeLookupArray;
        if (referenceNodeArray is null)
            Console.WriteLine(string.Format("IsPossible Remove on orderIndex {0}", orderIndex));
        for (int index = 0; index < scoreDeltas.Length; index++)
        {

            int dayIndex = referenceNodeArray[index].DayIndex;
            Day day = s.Days[dayIndex];
            int truckIndex = referenceNodeArray[index].Truck;
            Trip trip = day.Schedules[truckIndex, referenceNodeArray[index].TripIndex];
            Node orderNode = trip.Nodes[referenceNodeArray[index].NodeIndex];
            // calculate the difference in scores if the node is removed
            
            int nextID = orderNode.Next is null ? Program.DepotID : orderNode.Next.Order.MatrixID;
            int prevID = orderNode.Prev.Order.MatrixID;
            
            timeDeltas[index] = Program.TimeMatrix[prevID, nextID] -
                                (Program.TimeMatrix[prevID, orderNode.Order.MatrixID] +
                                 Program.TimeMatrix[orderNode.Order.MatrixID, nextID] +
                                 orderNode.Order.EmptyTime);
            
            float x = s.Days[dayIndex].TruckTimes[truckIndex];
            overtime += Math.Max(Program.TimePerDay, x + timeDeltas[index]) - Math.Max(Program.TimePerDay, x);
            
            // add penalty per visit to the score
            scoreDeltas[index] = timeDeltas[index] + orderNode.Order.PenaltyPerVisit;

        }
        
        return true;
    }

    public void Act(Solution s, bool changeVisitAmount = true)
    {
        Node[] reference = Program.Orders[orderIndex].NodeLookupArray;
        
        for (int index = 0; index < reference.Length; index++)
        {
            Day day = s.Days[reference[index].DayIndex];
            int truckIndex = reference[index].Truck;
            int tripIndex = reference[index].TripIndex;
            
            // remove the node from the trip of the truck of the day
            day.RemoveNodeFromTrip(truckIndex, tripIndex, reference[index].NodeIndex, timeDeltas[index]);
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
    
    public float GetOvertimeReduction(Solution s)
    {
        float x = s.Days[dayIndex].TruckTimes[truckIndex];
        return Math.Max(Program.TimePerDay, x + timeDelta) - Math.Max(Program.TimePerDay, x);
    }

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
        
        return true;
    }
    private bool GenerateTripNodeIndices(Day day)
    {
        tripIndex = day.TripCount[truckIndex] > 0 ? Program.random.Next(0, day.TripCount[truckIndex]) : 0;
        if (day.Schedules[truckIndex, tripIndex] is null || day.Schedules[truckIndex, tripIndex].NodeCount < 2)
        {
            return false;
        }
        nodeIndexX = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount-1);
        
        nodeIndexY = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount);
        while (nodeIndexY == nodeIndexX)
        {
            nodeIndexY = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount);
        }
        
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

public class SwapNeighbourNodes : Action
{
    int dayIndex;
    int truckIndex;
    int tripIndex;
    int nodeIndexX;
    int nodeIndexY;

    private float timeDelta;

    public float GetOvertimeReduction(Solution s)
    {
        float x = s.Days[dayIndex].TruckTimes[truckIndex];
        return Math.Max(Program.TimePerDay, x + timeDelta) - Math.Max(Program.TimePerDay, x);
    }
    
    public float GetScoreDelta()
    {
        return timeDelta;
    }
    
    public bool IsPossible(Solution s) {
        dayIndex = Program.random.Next(0, 5);
        truckIndex = Program.random.Next(0, 2);
        Day day = s.Days[dayIndex];
        
        tripIndex = day.TripCount[truckIndex] > 0 ? Program.random.Next(0, day.TripCount[truckIndex]) : 0;
        if (day.Schedules[truckIndex, tripIndex] is null || day.Schedules[truckIndex, tripIndex].NodeCount < 2)
        {
            return false;
        }

        Trip trip = day.Schedules[truckIndex, tripIndex];
        
        nodeIndexX = Program.random.Next(0, day.Schedules[truckIndex, tripIndex].NodeCount-1);
        
        Node X = trip.Nodes[nodeIndexX];
        Node Y;
        if (X.Next is null)
        {
            Y = X;
            X = X.Prev;
        }
        else
        {
            Y = X.Next;
        }

        nodeIndexX = X.NodeIndex;
        nodeIndexY = Y.NodeIndex;
        
        int x = X.Order.MatrixID;
        int xP = X.Prev.Order.MatrixID;
        
        int y = Y.Order.MatrixID;
        int yN = Y.Next is null ? Program.DepotID : Y.Next.Order.MatrixID;

        float costNow = Program.TimeMatrix[xP, x] +
                        Program.TimeMatrix[x, y] +
                        Program.TimeMatrix[y, yN];
        
        float costFuture = Program.TimeMatrix[xP, y] +
                            Program.TimeMatrix[y, x] +
                            Program.TimeMatrix[x, yN];
        
        timeDelta = costFuture - costNow;
        
        return true;
    }
    
    public void Act(Solution s, bool _)
    {
        Day day = s.Days[dayIndex];
        Node nodeX = day.Schedules[truckIndex, tripIndex].Nodes[nodeIndexX];
        Node nodeY = day.Schedules[truckIndex, tripIndex].Nodes[nodeIndexY];

        nodeX.Next = nodeY.Next;
        if (nodeY.Next is not null)
            nodeY.Next.Prev = nodeX;
        nodeY.Next = nodeX;
        
        nodeY.Prev = nodeX.Prev;
        nodeX.Prev.Next = nodeY;
        nodeX.Prev = nodeY;
        
        day.TruckTimes[truckIndex] += timeDelta;
    }
}

/*
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

    public float GetMaxOvertime(Solution s)
    {
        throw new NotImplementedException();
    }

    public virtual bool IsPossible(Solution s)
    {
        Init(s);
        // er zijn geen nodes
        if (nodeIndexY == -1) 
            return false;
        // kijk of deze shift wel kan
        if (NonValid(s))
            return false;
        
        Day dayX = s.Days[dayIndexX];
        Day dayY = s.Days[dayIndexY];
        
        Node nodeX2 = dayX.Schedules[truckIndexX, tripIndexX].Nodes[nodeIndexX];
        Node nodeY1 = dayY.Schedules[truckIndexY, tripIndexY].Nodes[nodeIndexY];
        
        // als de volgende node null is, is de huidige node het einde van de trip, dus dan moeten we de afstand tot het depot nemen
        int x3 = nodeX2.Next is null ? Program.DepotID : nodeX2.Next.Order.MatrixID;
        int x2 = nodeX2.Order.MatrixID;
        int x1 = nodeX2.Prev.Order.MatrixID;
        
        int y1 = nodeY1.Order.MatrixID;
        int y2 = nodeY1.Next is null ? Program.DepotID : nodeY1.Next.Order.MatrixID;
        
        // bereken wat de nieuwe kosten zijn
        float costNowX = Program.TimeMatrix[x1, x2] +
            Program.TimeMatrix[x2, x3];

        float costNowY = Program.TimeMatrix[y1, y2];
        
        float costFutureX = Program.TimeMatrix[x1, x3] - nodeX2.Order.EmptyTime;
        
        float costFutureY = Program.TimeMatrix[y1, x2] +
            Program.TimeMatrix[x2, y2] +
            nodeX2.Order.EmptyTime;
        
        timeDeltaX = costFutureX - costNowX;
        timeDeltaY = costFutureY - costNowY;
        
        // return of het wel in de dagen zou passen
        return true;
    }
    private void Init(Solution s)
    {
        // twee random dagen
        dayIndexX = Program.random.Next(0, 4);
        dayIndexY = Program.random.Next(dayIndexX, 5);

        truckIndexX = Program.random.Next(0, 2);
        truckIndexY = Program.random.Next(0, 2);
        
        Day dayX = s.Days[dayIndexX];
        
        // kijken of de trucks wel genoeg trips hebben en of die trips wel genoeg nodes hebben
        tripIndexX = dayX.TripCount[truckIndexX] > 0 ? Program.random.Next(0, dayX.TripCount[truckIndexX]) : 0;
        if (dayX.Schedules[truckIndexX, tripIndexX] is null)
        {
            nodeIndexY = -1;
            return;
        }
        nodeIndexX = Program.random.Next(0, dayX.Schedules[truckIndexX, tripIndexX].NodeCount);

        Day dayY = s.Days[dayIndexY];

        tripIndexY = dayY.TripCount[truckIndexY] > 0 ? Program.random.Next(0, dayY.TripCount[truckIndexY]) : 0;
        if (dayY.Schedules[truckIndexY, tripIndexY] is null)
        {
            nodeIndexY = -1;
            return;
        }
        nodeIndexY = Program.random.Next(0, dayY.Schedules[truckIndexY, tripIndexY].NodeCount);
    }
    public bool NonValid(Solution s)
    {
        // verschillende condities van waarom de shift niet mogelijk zou kunnen zijn
        bool cond1 = dayIndexX != dayIndexY ||
            truckIndexX != truckIndexY ||
            tripIndexX != tripIndexY  ||
            nodeIndexX != nodeIndexY;

        Node nodeX2 = s.Days[dayIndexX].Schedules[truckIndexX, tripIndexX].Nodes[nodeIndexX];
        Node nodeY1 = s.Days[dayIndexY].Schedules[truckIndexY, tripIndexY].Nodes[nodeIndexY];
        
        // de nodes mogen niet gelijk zijn aan elkaar
        bool cond2 = nodeX2.Prev != nodeY1;
        
        // de nodes moeten een frequency van 1 maal per week hebben
        bool cond3 = dayIndexX != dayIndexY || nodeX2.Order.Frequency == VisitAmount.Once;

        return cond1 && cond2 && cond3; 
    }

    public void Act(Solution s, bool changeVisitIndex)
    {

        Day dayX = s.Days[dayIndexX];
        Day dayY = s.Days[dayIndexY];
        
        Node nodeX2 = dayX.Schedules[truckIndexX, tripIndexX].Nodes[nodeIndexX];
        // haal de node X2 weg uit de ene trip
        dayX.RemoveNodeFromTrip(truckIndexX, tripIndexX, nodeIndexX, timeDeltaX);
        // voeg hem in de andere trip toe achter de andere node
        dayY.AddToSchedule(dayIndexY, truckIndexY, tripIndexY, nodeIndexY, timeDeltaY, nodeX2.Order);
    }

    public float GetScoreDelta()
    {
        return (timeDeltaX + timeDeltaY);
    }
}
*/