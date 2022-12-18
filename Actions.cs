using System.Security.Cryptography;
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

    public float GetScoreDelta()
    {
        return scoreDelta;
    }
    public AddSingle(Solution s, int newOrderIndex)
    {
        dayIndex = RandomNumberGenerator.GetInt32(0, 5);
        truckIndex = RandomNumberGenerator.GetInt32(0, 2);
        this.newOrderIndex = newOrderIndex;
        Day day = s.Days[dayIndex];
        GenerateTripNodeIndices(day);
    }
    
    public AddSingle(Solution s, int newOrderIndex, int dayIndex)
    {
        this.dayIndex = dayIndex;
        truckIndex = RandomNumberGenerator.GetInt32(0, 2);
        this.newOrderIndex = newOrderIndex;
        Day day = s.Days[dayIndex];
        GenerateTripNodeIndices(day);
    }

    private void GenerateTripNodeIndices(Day day)
    {
        tripIndex = day.TripCount[truckIndex] > 0 ? RandomNumberGenerator.GetInt32(0, day.TripCount[truckIndex]) : 0;
        if (day.TripCount[truckIndex] == 0)
        {
            nodeIndex = 0;
            return;
        }
        nodeIndex = day.Schedules[truckIndex, tripIndex].NodeCount > 0 ? RandomNumberGenerator.GetInt32(0, day.Schedules[truckIndex, tripIndex].NodeCount) : 0;
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
                                Program.TimeMatrix[newOrder.MatrixID, Program.DepotID] + 30;
            
            if (day.TruckTimes[truckIndex] + newTripTime > Program.TimePerDay) //check if we can make a new trip
            {
                return false;
            }

            makeTrip = true;
            scoreDelta = newTripTime;
            return true;
        }
        
        Node node = trip.Nodes[nodeIndex];
        int nextOrderID = node.Next is null ? Program.DepotID : node.Next.Order.MatrixID;   // check if the next order exists
        float currentTime = Program.TimeMatrix[node.Order.MatrixID, nextOrderID];
        float futureTime = Program.TimeMatrix[node.Order.MatrixID, newOrder.MatrixID] +
                           newOrder.EmptyTime +
                            Program.TimeMatrix[newOrder.MatrixID, nextOrderID];
        if (day.TruckTimes[truckIndex] + futureTime - currentTime > Program.TimePerDay)
        {
            return false;
        }

        scoreDelta = futureTime - currentTime - newOrder.PenaltyPerVisit;
        return true;
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
            day.CreateTrip(Program.Orders[Program.NotVisitedAmount], truckIndex, scoreDelta);
        }
        else
        {
            // because the order is now at the back of the orders list, the NotVisitedAmount will be its index
            day.AddToSchedule((int)day.Today, truckIndex, tripIndex, nodeIndex, scoreDelta, Program.NotVisitedAmount);
        }
        
        // change the score
        s.Score += scoreDelta;
    }
}

public class AddTwo : Action
{
    int dayIndex = RandomNumberGenerator.GetInt32(0, 2);
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
        for (int i = 0; i < ActionArray.Length; i++)
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
    
    int orderIndex;

    private float[] scoreDeltas;
    
    public RemoveAction(Solution s, int orderIndex)
    {
        scoreDeltas = new float[(int)Program.Orders[orderIndex].Frequency];
        this.orderIndex = orderIndex;
    }
    
    public bool IsPossible(Solution s)
    {
        for (int index = 0; index < scoreDeltas.Length; index++)
        {
            Console.WriteLine(Program.NotVisitedAmount + " " + orderIndex + " " + Program.Orders.Length);
            int[,] reference = Program.Orders[orderIndex].NodeLookupArray;
            
            Day day = s.Days[reference[index, 0]];
            Trip trip = day.Schedules[reference[index, 1], reference[index, 2]];
            Node orderNode = trip.Nodes[reference[index, 3]];
            // calculate the difference in scores if the node is removed

            int nextID = orderNode.Next is null ? Program.DepotID : orderNode.Next.Order.MatrixID;
            int prevID = orderNode.Prev.Order.MatrixID;
            
            scoreDeltas[index] = Program.Orders[orderIndex].PenaltyPerVisit +
                                 Program.TimeMatrix[prevID, nextID] -
                                 (Program.TimeMatrix[prevID, orderNode.Order.MatrixID] +
                                  Program.TimeMatrix[orderNode.Order.MatrixID, nextID] +
                                  orderNode.Order.EmptyTime);
        }
        return true;
    }

    public void Act(Solution s, bool changeVisitAmount = true)
    {
        // This node now gets swapped with the last node in the list, so it now belongs to the orders that are not yet used
        (Program.Orders[orderIndex], Program.Orders[Program.NotVisitedAmount]) = (
            Program.Orders[Program.NotVisitedAmount], Program.Orders[orderIndex]);
        
        int[,] reference = Program.Orders[Program.NotVisitedAmount].NodeLookupArray;
        Console.WriteLine(reference.GetLength(0) + "<= amount of nodes to be deleted");
        for (int index = 0; index < reference.GetLength(0); index++)
        {
            Day day = s.Days[reference[index, 0]];
            Trip trip = day.Schedules[reference[index, 1], reference[index, 2]];
            if (trip.RemoveNode(reference[index, 3]))
            {
                day.RemoveFromSchedule(trip.truck, reference[index, 2], scoreDeltas[index]);
            }
        }
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
    int nodeIndexA;
    int nodeIndexB;

    public bool IsPossible(Solution s)
    {
        dayIndex = RandomNumberGenerator.GetInt32(0, 5);
        truckIndex = RandomNumberGenerator.GetInt32(0, 2);
    }

    public void Act(Solution s)
    {

    }

    public float GetScoreDelta()
    {

    }
}