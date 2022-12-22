using System;
using System.Linq;
using System.Security.Cryptography;
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
        if(referenceNodeArray[0] is null)
            Console.WriteLine(string.Format("IsPossible Remove on orderIndex {0}", orderIndex));
        for (int index = 0; index < scoreDeltas.Length; index++)
        {
            //Console.WriteLine(Program.NotVisitedAmount + " " + orderIndex + " " + Program.Orders.Length);
            try
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
                //Console.WriteLine(scoreDeltas[index] + " " + timeDeltas[index] + "  " +  orderNode.Order.PenaltyPerVisit);
            }
            catch (Exception e)
            {
                Console.WriteLine("errored");
                /*Console.WriteLine(e);
                Console.WriteLine(orderIndex);
                Console.WriteLine(Program.Orders[orderIndex]);
                Console.WriteLine(index);
                Console.WriteLine();*/
            }            
            
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

/*public class ShiftAction : Action
{
    public bool IsPossible(Solution s);
    public void Act(Solution s);
    public float GetScoreDelta();
}*/