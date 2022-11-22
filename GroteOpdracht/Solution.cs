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

        private List<Order>[,] solutionMatrix; 
        // een matrix met 2 rijen (aantal vrachauto's) en 5 (aantal dagen per week) kolommen. Daarin zit
        // een List van alle Orders die langs gegaan zijn op die dag.

        public List<Order>[,] OplossingsMatrix
        {
            get { return solutionMatrix; }
        }

        public Solution(List<Order>[,] solutionMatrix, float score = -1)
        {
            this.solutionMatrix = solutionMatrix;
            if (score == -1)
            {
                score = ScoreCalculations.CalculateScore(solutionMatrix);
            }
            else
            {
                this.score = score;
            }
        }

        public static List<Order>[,] GreedySolve(List<Order>[] orders, int[,] distanceMatrix)
        {
            List<Order>[,] solutionMatrix = new List<Order>[2,5];
            // maak een geldige oplossing met een greedy algoritme
            
            return solutionMatrix;
        }
    }
}
