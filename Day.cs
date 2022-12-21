using GroteOpdracht;

namespace GroteOpdracht;

public class Day
{
    public WorkDay Today;
    public Trip[,] Schedules = new Trip[2, 24]; // er zitten max 24 trips in een dag
    public int[] TripCount = { 0, 0 };
    public float[] TruckTimes = { 0, 0 };

    public Day(WorkDay today)
    {
        Today = today;
    }

    public void RemoveNodeFromTrip(int truck, Trip trip, Node node, float timeDelta)
    {
        TruckTimes[node.Truck] += timeDelta;
        
        // if true, the trip is empty and can be deleted.
        if (trip.RemoveNode(node.NodeIndex))
        {
            RemoveTripFromSchedule(trip.truck, node.TripIndex);
        }
    }
    public void RemoveTripFromSchedule(int truck, int tripIndex)
    {
        TripCount[truck]--; // we remove a trip
        Schedules[truck, tripIndex] = Schedules[truck, TripCount[truck]];
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
        trip.AddOrder(Program.Orders[newOrderIndex], dayIndex, truck, tripIndex, nodeIndex);
    }

    public void SwitchBetweenTrucks()
    {
        
    }

    public void SwitchBetweenTrips()
    {
        
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