using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

using System.Threading.Tasks;

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
        public bool UseRTree;


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

        // ===============================================================================================
        // Use Parallel Computation
        // ===============================================================================================
                          
        /*    
        private void ComputeAgentDesiredVelocity(FlockAgent agent)
        {
            List<FlockAgent> neighbours = FindNeighbours(agent);
            agent.ComputeDesiredVelocity(neighbours);
        }


        public void Update()
        {
            if (!UseParallel)
                foreach (FlockAgent agent in Agents)
                {
                    List<FlockAgent> neighbours = FindNeighbours(agent);
                    agent.ComputeDesiredVelocity(neighbours);
                }
            else
                Parallel.ForEach(Agents, ComputeAgentDesiredVelocity); //List of object of type T, a function/method that takes type T and returns null/void
        }
        */
             
        public void Update()
        {

            if (!UseParallel)
                foreach (FlockAgent agent in Agents)
                {
                    List<FlockAgent> neighbours = FindNeighbours(agent);
                    agent.ComputeDesiredVelocity(neighbours);
                }
            else

                Parallel.ForEach(Agents, (FlockAgent agent) =>
                {
                    List<FlockAgent> neighbours = FindNeighbours(agent);
                    agent.ComputeDesiredVelocity(neighbours);
                } );
            


            // Once the desired velocity for each agent has been computed, we update each position and velocity
            foreach (FlockAgent agent in Agents)
                agent.UpdateVelocityAndPosition();
        }


        public void UpdateUsingRTree()
        {
            //build RTree

            RTree rTree = new RTree();

            for (int i = 0; i < Agents.Count; i++)           
                rTree.Insert(Agents[i].Position, i); //insert function takes point 3d, id (in order to get point id is used to gather points

            //Then, we use the R-Tree to find the neighbours and compute the desired velocity

            foreach (FlockAgent agent in Agents)
            {
                List<FlockAgent> neighbours = new List<FlockAgent>();

                EventHandler<RTreeEventArgs> rTreeFeedback =
                (object sender, RTreeEventArgs args) =>
                {
                    if (Agents[args.Id] != agent)
                        neighbours.Add(Agents[args.Id]);
                };
                                   
                rTree.Search(new Sphere(agent.Position, NeighbourhoodRadius), rTreeFeedback);

                agent.ComputeDesiredVelocity(neighbours);
            }

            // Once the desired velocity for each agent has been computed, we update each position and velocity
            foreach (FlockAgent agent in Agents)
                agent.UpdateVelocityAndPosition();
        }
        public void UpdateUsingKDTree()
        {
            //TO DO 
            //UPDATE W KD TREE
        }
    }
}
