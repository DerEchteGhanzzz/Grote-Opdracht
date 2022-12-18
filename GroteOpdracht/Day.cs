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

    public void RemoveFromSchedule(int truck, int tripIndex, float time)
    {
        TripCount[truck]--;
        TruckTimes[truck] -= time;
        Schedules[truck, tripIndex] = Schedules[truck, TripCount[truck]];
    }

    public void CreateTrip(Order order, int truck, float timeDelta)
    {
        Schedules[truck, TripCount[truck]] = new Trip(order, (int)Today, truck, TripCount[truck]);
        TripCount[truck]++;
        TruckTimes[truck] += timeDelta;
    }

    public void AddToSchedule(int dayIndex, int truckIndex, int tripIndex, int nodeIndex, float timeDelta, int newOrderIndex)
    {
        TruckTimes[truckIndex] += timeDelta;
        Trip trip = Schedules[truckIndex, tripIndex];
        trip.AddOrder(Program.Orders[newOrderIndex], dayIndex, truckIndex, tripIndex, nodeIndex);
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