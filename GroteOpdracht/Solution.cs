using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroteOpdrachtUtils;

namespace GroteOpdracht
{
    public class Solution
    {
        private float score; // elke solution heeft een score
        public float Score
        {
            get { return score; }
        }

        private List<Node>[,] solutionMatrix; 
        // een matrix met 2 rijen (aantal vrachauto's) en 5 (aantal dagen per week) kolommen. Daarin zit
        // een List van alle Orders die langs gegaan zijn op die dag.

        public List<Node>[,] OplossingsMatrix
        {
            get { return solutionMatrix; }
        }

        public Solution(List<Node>[,] solutionMatrix, float score = -1)
        {
            this.solutionMatrix = solutionMatrix;
            if (score == -1)
            {
                this.score = ScoreCalculations.CalculateScore(solutionMatrix);
            }
            else
            {
                this.score = score;
            }
        }

        public static List<Node>[,] GreedySolve(List<Node>[] orders, int[,] distanceMatrix)
        {
            List<Node>[,] solutionMatrix = new List<Node>[2,5];
            // maak een geldige oplossing met een greedy algoritme
            
            return solutionMatrix;
        }
    }

    public class Destination
    {
        private Node order;
        private float currentScore;
        private int currentLoad;

        private Destination next;
        private Destination prev;
        public Destination Next
        {
            get { return next; }
            set { next = value; }
        }
        public Destination Prev
        {
            get { return prev; }
            set { prev = value; }
        }

        public Destination(Node order)
        {   // een bestemming is een node met een order erin. Hij weet wat de score is op deze plek en hoe vol de truck zit.
            this.order = order;
        }

        public void AddPrevious(Destination prev)
        {   // dit maakt de Destinations een doubly linked list
            this.prev = prev;
            this.prev.Next = this;
            UpdateScores(); // dit gaat te lang duren om te updaten voor elke node
        }

        public void UpdateScores()
        {
            this.currentLoad = prev.GetGarbageAmt() + this.GetGarbageAmt();
            this.currentScore = prev.currentScore + Program.timeMatrix[prev.order.matrixID,order.matrixID] + order.emptyTime;
        }

        public int GetGarbageAmt()
        {   // hoeveel afval moet er opgehaald worden?
            return this.order.containerCount * this.order.containerVol;
        }

        public override bool Equals(object? obj)
        {
            return this.order.Equals(obj);
        }
    }
}
