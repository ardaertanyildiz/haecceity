﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;


namespace Haecceity.Flock
{
    public class FlockAgent
    {
        public Point3d Position;
        public Vector3d Velocity;

        public FlockSystem FlockSystem;

        private Vector3d desiredVelocity;

        public FlockAgent(Point3d position, Vector3d velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        //update vel and position (range 4.0 - 8.0)
        public void UpdateVelocityAndPosition()
        {
            Velocity = 0.97 * Velocity + 0.03 * desiredVelocity;

            if (Velocity.Length > 8.0)
            {
                Velocity *= 8.0 / Velocity.Length;
            }
            else if (Velocity.Length < 4.0)
            {
                Velocity *= 4.0 / Velocity.Length;
            }           

            Position += Velocity * FlockSystem.Timestep;
        }



        public void ComputeDesiredVelocity(List<FlockAgent> neighbours)
        {
            // First, reset the desired velocity to 0
            desiredVelocity = new Vector3d(0.0, 0.0, 0.0);

            // ===============================================================================
            // Inverse the vector if agent exceeds bounding box
            // ===============================================================================

            //double boundingBoxX = Rhino.Geometry.BoundingBox()
            double boundingBoxSize = 30.0;
            

            if (Position.X < 0.0)
                desiredVelocity += new Vector3d(-Position.X, 0.0, 0.0);
            else if (Position.X > boundingBoxSize)
                desiredVelocity += new Vector3d(boundingBoxSize - Position.X, 0.0, 0.0);

            if (Position.Y < 0.0)
                desiredVelocity += new Vector3d(0.0, -Position.Y, 0.0);
            else if (Position.Y > boundingBoxSize)
                desiredVelocity += new Vector3d(0.0, boundingBoxSize - Position.Y, 0.0);

            if (Position.Z < 0.0)
                desiredVelocity += new Vector3d(0.0, 0.0, -Position.Z);
            else if (Position.Z > boundingBoxSize)
                desiredVelocity += new Vector3d(0.0, 0.0, boundingBoxSize - Position.Z);


            // ===============================================================================
            // If there are no neighbours nearby, the agent will maintain its velocity,
            // else it will perform the "alignment", "cohension" and "separation" behaviours
            // ===============================================================================

            if (neighbours.Count == 0)
                desiredVelocity += Velocity; // maintain the current velocity
            else
            {
                // -------------------------------------------------------------------------------
                // "Alignment" behavior 
                // -------------------------------------------------------------------------------

                Vector3d alignment = Vector3d.Zero;

                foreach (FlockAgent neighbour in neighbours)
                    alignment += neighbour.Velocity;

                // We divide by the number of neighbours to actually get their average velocity
                alignment /= neighbours.Count;

                desiredVelocity += FlockSystem.AlignmentStrength * alignment;


                // -------------------------------------------------------------------------------
                // "Cohesion" behavior 
                // -------------------------------------------------------------------------------

                Point3d centre = Point3d.Origin;

                foreach (FlockAgent neighbour in neighbours)
                    centre += neighbour.Position;

                // We divide by the number of neighbours to actually get their centre of mass
                centre /= neighbours.Count;

                Vector3d cohesion = centre - Position;

                desiredVelocity += FlockSystem.CohesionStrength * cohesion;


                // -------------------------------------------------------------------------------
                // "Separation" behavior 
                // -------------------------------------------------------------------------------

                Vector3d separation = Vector3d.Zero;

                foreach (FlockAgent neighbour in neighbours)
                {
                    double distanceToNeighbour = Position.DistanceTo(neighbour.Position);

                    //if (distanceToNeighbour < FlockSystem.SeparationDistance)
                    if (distanceToNeighbour <= FlockSystem.SeparationDistance)
                    {
                        Vector3d getAway = Position - neighbour.Position;

                        /* We scale the getAway vector by inverse of distanceToNeighbour to make 
                           the getAway vector bigger as the agent gets closer to its neighbour */
                        separation += getAway / (getAway.Length * distanceToNeighbour); //divide getaway.lenght to normalize 
                    }


                }

                desiredVelocity += FlockSystem.SeparationStrength * separation;
            }


            // ===============================================================================
            // Avoiding the obstacles (repellers)
            // ===============================================================================

            foreach (Circle repeller in FlockSystem.Repellers)
            {
                double distanceToRepeller = Position.DistanceTo(repeller.Center);

                Vector3d repulsion = Position - repeller.Center;

                // Repulstion gets stronger as the agent gets closer to the repeller
                repulsion /= (repulsion.Length * distanceToRepeller); //unititze by dividing repulsion length

                // Repulsion strength is also proportional to the radius of the repeller circle/sphere
                // This allows the user to tweak the repulsion strength by tweaking the radius
                repulsion *= 30.0 * repeller.Radius;

                desiredVelocity += repulsion;

            }

            //TO DO:
            //SET REPULSION STRENTH AS A SEPERATE PARAMETER
            //SET REPULSION MESH
        }   
    }
}
