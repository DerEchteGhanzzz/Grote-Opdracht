using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GroteOpdrachtUtils
{
    public class DayTrip
    {
        public Destination start = new Destination(new Depot()); // start Truck 1
        public Destination middle = new Destination(new Depot()); // end Truck 1, start Truck 2
        public Destination finish = new Destination(new Depot()); // end Truck 2
        public Heatmap heatMap;
        public DayTrip()
        {
            
        }

        public void AddTo1(Order order)
        {
            //TODO: add to graph
            middle.timeLeft -= order.emptyTime;
        }

    }

    public class Destination
    {
        public Order myOrder;
        public Destination prevDest;
        public Destination nextDest;

        public int trashVol;
        public float timeLeft;

        public Destination(Destination prevDest, Order myOrder, Destination nextDest)
        {
            this.prevDest = prevDest;
            this.myOrder = myOrder;
            this.nextDest = nextDest;

            this.trashVol = prevDest.trashVol + myOrder.containerCount * myOrder.containerVol;
        }


    }
}
