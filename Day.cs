using GroteOpdracht;

namespace GroteOpdracht;

public class Day
{
    public readonly WorkDay Today;
    public Trip[,] Schedules { get; private set; } // [truck, trip]
    public int[] TripCount { get; private set; }
    public float[] TruckTimes { get; private set; }

    public Day(WorkDay today)
    {
        Today = today;
        Schedules = new Trip[2, 24]; // er zitten max 24 trips in een dag
        TripCount = new int[]{ 0, 0 };
        TruckTimes = new float[]{ 30 * 60, 30 * 60 };
    }

    public void RemoveNodeFromTrip(int truck, int trip, int nodeIndex, float timeDelta)
    {
        TruckTimes[truck] += timeDelta;
        
        // if true, the trip is empty and can be deleted.
        if (Schedules[truck,trip].RemoveNode(nodeIndex))
        {
            RemoveTripFromSchedule(truck, trip);
        }
    }
    private void RemoveTripFromSchedule(int truck, int tripIndex)
    {
        TripCount[truck]--; // we remove a trip
        try
        {
            Schedules[truck, tripIndex] = Schedules[truck, TripCount[truck]];
        }
        catch (System.IndexOutOfRangeException e)
        {
            Console.WriteLine(string.Format("errorDay: {0}\nTruck: {1}\ntripcount:{2}",Today, truck, TripCount[truck]));
        }
        TruckTimes[truck] -= 30 * 60;   // remove the depot empty time.
    }

    public void CreateTrip(Order order, int truck, float timeDelta)
    {
        Schedules[truck, TripCount[truck]] = new Trip(order, (int)Today, truck, TripCount[truck]);
        TripCount[truck]++;
        TruckTimes[truck] += timeDelta;
    }

    public void AddToSchedule(int dayIndex, int truck, int tripIndex, int nodeIndex, float timeDelta, int newOrderIndex)
    {
        TruckTimes[truck] += timeDelta;
        Trip trip = Schedules[truck, tripIndex];
        trip.AddOrder(Program.Orders[newOrderIndex], dayIndex, tripIndex, nodeIndex);
    }
    public void AddToSchedule(int dayIndex, int truck, int tripIndex, int nodeIndex, float timeDelta, Order newOrder)
    {
        TruckTimes[truck] += timeDelta;
        Trip trip = Schedules[truck, tripIndex];
        trip.AddOrder(newOrder, dayIndex, tripIndex, nodeIndex);
    }

    public override string ToString()
    {
        return ((int)Today + 1).ToString();
    }
}

public enum WorkDay
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday
}