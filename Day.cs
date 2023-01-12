using System;
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
        
        Schedules = new Trip[2, 24]; // er zitten max 24 trips in een dag (want je kan niet meer dan 24 keer 30 minuten storten)
        TripCount = new int[]{ 0, 0 };  // hoeveel trips zijn er nu aanwezig voor elke truck?
        TruckTimes = new float[]{ 30 * 60, 30 * 60 };   // hoe lang doet elke truck erover om rond te rijden
    }

    public void RemoveNodeFromTrip(int truck, int trip, int nodeIndex, float timeDelta)
    {
        TruckTimes[truck] += timeDelta;
        
        // if true, the trip is empty and can be deleted.
        if (Schedules[truck, trip].RemoveNode(nodeIndex))
        {
            if (TripCount[truck] == 1)
            {
                Console.WriteLine(Schedules[truck, trip].Nodes[nodeIndex] + " :: " + Schedules[truck, trip].Nodes[nodeIndex].Order);
            }
            RemoveTripFromSchedule(truck, trip, nodeIndex);
        }
    }
    private void RemoveTripFromSchedule(int truck, int tripIndex, int nodeIndex)
    {
        TripCount[truck]--; // we remove a trip

        Schedules[truck, tripIndex] = Schedules[truck, TripCount[truck]];
        
        TruckTimes[truck] -= 30 * 60;   // remove the depot empty time.
    }

    public void CreateTrip(Order order, int truck, float timeDelta)
    {
        // maak een nieuwe trip aan
        Schedules[truck, TripCount[truck]] = new Trip(order, (int)Today, truck, TripCount[truck]);
        TripCount[truck]++;
        TruckTimes[truck] += timeDelta;
    }

    public void AddToSchedule(int dayIndex, int truck, int tripIndex, int nodeIndex, float timeDelta, int newOrderIndex)
    {
        // voeg een order toe aan een trip op de dag
        TruckTimes[truck] += timeDelta;
        Trip trip = Schedules[truck, tripIndex];
        trip.AddOrder(Program.Orders[newOrderIndex], dayIndex, tripIndex, nodeIndex);
    }
    public void AddToSchedule(int dayIndex, int truck, int tripIndex, int nodeIndex, float timeDelta, Order newOrder)
    {
        // voeg een order toe aan een trip in de dag
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