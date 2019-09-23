﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AvoidanceBehavior : SteeringBehavior {


    public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
    {
        LinkedList<AgentWrapped> obstacles = GetFilteredAgents(surroundings, this);
        if (obstacles.First == null)
        {
            steer = Vector3.zero;
            return;
        }
        bool foundObstacleInPath = false;
        float closestHitDistance = float.MaxValue;
        Vector3 closestHitPoint = Vector3.zero;
        Agent mostThreateningObstacle = obstacles.First.Value.agent;

        foreach (AgentWrapped obs_wrapped in obstacles)
        {
            Vector3 closestPoint = ClosestPointPathToObstacle(mine, obs_wrapped);
            if (Vector3.Distance(closestPoint, obs_wrapped.wrappedPosition) < obs_wrapped.agent.radius)
            {
                //found obstacle directly in path
                foundObstacleInPath = true;

                float distanceToClosestPoint = Vector3.Distance(closestPoint, mine.position);
                if (distanceToClosestPoint < closestHitDistance)
                {
                    closestHitDistance = distanceToClosestPoint;
                    closestHitPoint = closestPoint;
                    mostThreateningObstacle = obs_wrapped.agent;
                }

            }
        }

        if (!foundObstacleInPath)
        {
            steer = Vector3.zero;
            return;
        }
        float distanceToObstacleEdge = Mathf.Max(Vector3.Distance(mine.position, mostThreateningObstacle.position) - mostThreateningObstacle.radius, 1);
        if (distanceToObstacleEdge > effectiveRadius)
        {
            steer = Vector3.zero;
            return;
        }
        steer = closestHitPoint - mostThreateningObstacle.position;
        steer = steer.normalized * mine.activeSettings.maxForce;
    }

    

    Vector3 ClosestPointPathToObstacle(SteeringAgent mine, AgentWrapped obstacle)
    {
        Vector3 agentPos = mine.position;
        Vector3 agentToObstacle = obstacle.wrappedPosition - agentPos;
        Vector3 projection = Vector3.Project(agentToObstacle, mine.velocity.normalized);
        if (projection.normalized == mine.velocity.normalized)
            return agentPos + projection;
        else return agentPos;
    }
}
