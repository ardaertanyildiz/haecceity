using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;



namespace Haecceity.Flock
{
    public class FlockSystem
    {
        public List<FlockAgent> Agents;

        public double Timestep;
        public double NeighbourhoodRadius;
        public double AlignmentStrength;
        public double CohesionStrength;
        public double SeparationStrength;
        public double SeparationDistance;
        public List<Circle> Repellers;
        public bool UseParallel;


        public FlockSystem(int agentCount, bool flock3D)
        {
            Agents = new List<FlockAgent>();

            //flock3d
            if (flock3D)
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        Util.GetRandomPoint(0.0, 30.0, 0.0, 30.0, 0.0, 30.0),
                        Util.GetRandomUnitVector() * 4.0);

                    agent.FlockSystem = this;

                    Agents.Add(agent);
                }
            //flock2d
            else
                for (int i = 0; i < agentCount; i++)
                {
                    FlockAgent agent = new FlockAgent(
                        Util.GetRandomPoint(0.0, 30.0, 0.0, 30.0, 0.0, 0.0),
                        Util.GetRandomUnitVectorXY() * 4.0);

                    agent.FlockSystem = this;

                    Agents.Add(agent);
                }
        }


        private List<FlockAgent> FindNeighbours(FlockAgent agent)
        {
            List<FlockAgent> neighbours = new List<FlockAgent>();

            foreach (FlockAgent neighbour in Agents)
                if (neighbour != agent && neighbour.Position.DistanceTo(agent.Position) < NeighbourhoodRadius)
                    neighbours.Add(neighbour);

            return neighbours;
        }


        public void Update()
        {
            foreach (FlockAgent agent in Agents)
            {
                List<FlockAgent> neighbours = FindNeighbours(agent);
                agent.ComputeDesiredVelocity(neighbours);
            }

            // Once the desired velocity for each agent has been computed, we update each position and velocity
            foreach (FlockAgent agent in Agents)
                agent.UpdateVelocityAndPosition();
        }
    }
}
