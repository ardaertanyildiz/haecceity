using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using Haecceity.Flock;
using Grasshopper.Kernel.Types;

namespace Haecceity
{
    public class GhcFlock : GH_Component
    {

        private FlockSystem flockSystem;

        public GhcFlock()
            : base(
                  "Flock",
                  "Flock",
                  "Flock",
                  "Haecceity",
                  "Simulation")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Play", "Play", "Play", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("3D", "3D", "3D", GH_ParamAccess.item, true);
            pManager.AddIntegerParameter("Count", "Count", "Number of Agents", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Timestep", "Timestep", "Timestep", GH_ParamAccess.item, 0.02);
            pManager.AddNumberParameter("Neighbourhood Radius", "Neighbourhood Radius", "Neighbourhood Radius", GH_ParamAccess.item, 3.5);
            pManager.AddNumberParameter("Alignment", "Alignment", "Alignment", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Cohesion", "Cohesion", "Cohesion", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation", "Separation", "Separation", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Separation Distance", "Separation Distance", "Separation Distance", GH_ParamAccess.item, 1.5);
            pManager.AddCircleParameter("Repellers", "Repellers", "Repellers", GH_ParamAccess.list);
            pManager[10].Optional = true;
            pManager.AddBooleanParameter("Use Parallel", "Use Parallel", "Use Parallel", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Use R-Tree", "Use R-Tree", "Use R-Tree", GH_ParamAccess.item, false);

            //pManager.AddBrepParameter("BoundingBox", "BoundingBox", "BoundingBox", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("Use KD-Tree", "Use KD-Tree", "Use KD-Tree", GH_ParamAccess.item, false);

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Information", GH_ParamAccess.item);
            pManager.AddPointParameter("Positions", "Positions", "The agent positions", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Velocities", "The agent velocities", GH_ParamAccess.list);
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================

            bool iReset = true;
            bool iPlay = false;
            bool i3D = false;
            int iCount = 0;
            double iTimestep = 0.0;
            double iNeighbourhoodRadius = 0.0;
            double iAlignment = 0.0;
            double iCohesion = 0.0;
            double iSeparation = 0.0;
            double iSeparationDistance = 0.0;
            List<Circle> iRepellers = new List<Circle>();
            bool iUseParallel = false;
            bool iUseRTree = false;
            //bool iUseKDTree = false;
            //Brep AgentBox = new Brep();


            if (!DA.GetData("Reset", ref iReset))return;
            if (!DA.GetData("Play", ref iPlay)) return;
            if (!DA.GetData("3D", ref i3D)) return;
            if (!DA.GetData("Count", ref iCount)) return;
            if (!DA.GetData("Timestep", ref iTimestep)) return;
            if (!DA.GetData("Neighbourhood Radius", ref iNeighbourhoodRadius)) return;
            if (!DA.GetData("Alignment", ref iAlignment)) return;
            if (!DA.GetData("Cohesion", ref iCohesion)) return;
            if (!DA.GetData("Separation", ref iSeparation)) return;
            if (!DA.GetData("Separation Distance", ref iSeparationDistance)) return;
            if (!DA.GetDataList("Repellers", iRepellers)) return;
            if (!DA.GetData("Use Parallel", ref iUseParallel)) return;
            if (!DA.GetData("Use R-Tree", ref iUseRTree)) return;
            //if (!DA.GetData("Use KD-Tree", ref iUseKDTree)) return;
            //if (!DA.GetData("BoundingBox", ref AgentBox)) return;

            // ===============================================================================================
            // Validate Data
            // ===============================================================================================

            if (iAlignment < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Cohesion>Seperation for flocking");
                return;
            }

            if (iCohesion < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Cohesion>Seperation for flocking");
                return;
            }

            if (iSeparation < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Cohesion>Seperation for flocking");
                return;
            }


            // ===============================================================================================
            // Read input parameters
            // ===============================================================================================

            if (iReset || flockSystem == null)
            {
                flockSystem = new FlockSystem(iCount, i3D);
            }
            else
            {
                // ===============================================================================================
                // Assign the input parameters to the corresponding variables in the  "flockSystem" object
                // ===============================================================================================

                flockSystem.Timestep = iTimestep;
                flockSystem.NeighbourhoodRadius = iNeighbourhoodRadius;
                flockSystem.AlignmentStrength = iAlignment;
                flockSystem.CohesionStrength = iCohesion;
                flockSystem.SeparationStrength = iSeparation;
                flockSystem.SeparationDistance = iSeparationDistance;
                flockSystem.Repellers = iRepellers;
                flockSystem.UseParallel = iUseParallel;
                flockSystem.UseRTree = iUseRTree;


                // ===============================================================================
                // Update the flock
                // ===============================================================================

                if (iUseRTree)
                    flockSystem.UpdateUsingRTree();
                else
                    flockSystem.Update();

                if (iPlay) ExpireSolution(true);
            }

            // ===============================================================================
            // Output the agent positions and velocities so we can see them on display
            // ===============================================================================

            List<GH_Point> positions = new List<GH_Point>();
            List<GH_Vector> velocities = new List<GH_Vector>();

            foreach (FlockAgent agent in flockSystem.Agents)
            {
                positions.Add(new GH_Point(agent.Position));
                velocities.Add(new GH_Vector(agent.Velocity));
            }

            DA.SetDataList("Positions", positions);
            DA.SetDataList("Velocities", velocities);
        }


        protected override System.Drawing.Bitmap Icon { get { return null; } }


        public override Guid ComponentGuid
        {
            get { return new Guid("2de1b48c-b3d7-4bd3-92e5-76b88686d251"); }
        }
    }
}