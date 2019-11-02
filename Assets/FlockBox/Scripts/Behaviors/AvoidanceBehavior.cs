﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class AvoidanceBehavior : ForecastSteeringBehavior
    {
        RaycastHit closestHit;
        RaycastHit hit;
        Agent mostImmediateObstacle;
        Vector3 edgePoint;
        Vector3 normal;
        Vector3 closestPoint;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            List<Agent> obstacles = GetFilteredAgents(surroundings, this);
            if (obstacles.Count == 0)
            {
                steer = Vector3.zero;
                return;
            }

            Ray myRay = new Ray(mine.Position, mine.Forward);
            float rayDist = surroundings.lookAheadSeconds * mine.Velocity.magnitude;
            bool foundObstacleInPath = false;
            foreach (Agent obstacle in obstacles)
            {
                if (obstacle.RaycastToShape(myRay, mine.shape.radius, rayDist, out hit))
                {
                    if (!foundObstacleInPath || hit.distance < closestHit.distance)
                    {
                        closestHit = hit;
                        mostImmediateObstacle = obstacle;
                    }
                    foundObstacleInPath = true;      
                }
            }

            if (!foundObstacleInPath)
            {
                steer = Vector3.zero;
                return;
            }
            mostImmediateObstacle.FindNormalToSteerAwayFromShape(myRay, closestHit, mine.shape.radius, ref normal);
            Debug.DrawLine(mine.Position, closestHit.point, Color.red * .5f);
            Debug.DrawRay(closestHit.point, closestHit.normal, Color.yellow);
            steer = normal;
            steer = steer.normalized * mine.activeSettings.maxForce;

        }
    }
}