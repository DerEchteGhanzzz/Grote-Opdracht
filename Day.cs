namespace GroteOpdracht;

public class Day
{
    public WorkDay Today;
    public Trip[][] Schedules = new Trip[][2];
    private int[] tripCounts = { 0, 0 };
    public float[] TruckTimes = { 0, 0 };

    public Day(WorkDay today)
    {
        Today = today;
        Schedules[0] = new Trip[24]; // er zitten max 24 trips in een dag
        Schedules[1] = new Trip[24];
    }

    public void RemoveFromSchedule(int truck, int scheduleIndex, float time)
    {
        tripCounts[truck]--;
        TruckTimes[truck] -= time;
        Schedules[truck][scheduleIndex] = Schedules[truck][tripCounts[truck]];
    }

    public void AddToSchedule(Trip t, int truck)
    {
        Schedules[truck][tripCounts[truck]] = t;
        tripCounts[truck]++;
        TruckTimes[truck] += t.TimeSpent;
    }

    public void SwitchBetweenTrucks()
    {
        
    }

    public void SwitchBetweenTrips()
    {
        
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