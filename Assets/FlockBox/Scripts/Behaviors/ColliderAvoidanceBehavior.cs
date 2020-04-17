﻿/*
MIT License

Copyright (c) 2019 Sebastian Lague

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    [System.Serializable]
    public class ColliderAvoidanceBehavior : ForecastSteeringBehavior
    {

        public float clearance;
        public LayerMask mask = -1;
        RaycastHit hit;
        public bool drawVisionRays = false;

        public override bool CanUseTagFilter { get { return false; } }

        const int numViewDirections = 360;
        private static Vector3[] Directions
        {
            get
            {
                if (_directions == null)
                {
                    _directions = new Vector3[numViewDirections];

                    float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
                    float angleIncrement = Mathf.PI * 2 * goldenRatio;

                    for (int i = 0; i < numViewDirections; i++)
                    {
                        float t = (float)i / numViewDirections;
                        float inclination = Mathf.Acos(1 - 2 * t);
                        float azimuth = angleIncrement * i;

                        float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                        float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                        float z = Mathf.Cos(inclination);
                        _directions[i] = new Vector3(x, y, z);
                    }
                }
                return _directions;
            }
        }
        private static Vector3[] _directions;

        private const string lastClearDirectionKey = "lastClearDirection";

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            float rayDist = lookAheadSeconds * mine.Velocity.magnitude;      
            Ray myRay = new Ray(mine.WorldPosition, mine.WorldForward);
            if (!ObstacleInPath(myRay, mine.shape.radius + clearance, rayDist, ref hit, mask))
            {
                steer = Vector3.zero;
                mine.SetAttribute(lastClearDirectionKey, steer);

                return;
            }
            float hitDist = hit.distance;

            Vector3 lastClearWorldDirection = Vector3.zero;
            if (mine.HasAttribute(lastClearDirectionKey))
            {
                lastClearWorldDirection = (Vector3)mine.GetAttribute(lastClearDirectionKey);
            }
            if (lastClearWorldDirection == Vector3.zero)
            {
                lastClearWorldDirection = myRay.direction;
            }

            myRay.direction = lastClearWorldDirection;

            steer = ObstacleRays(myRay, mine.shape.radius + clearance, rayDist, ref hit, mask);
            mine.SetAttribute(lastClearDirectionKey, steer.normalized);
            float smooth = (1f - (hitDist / rayDist));
            steer = mine.WorldToFlockBoxDirection(steer);
            steer = steer.normalized * mine.activeSettings.maxForce - mine.Velocity;
            steer *= smooth;
        }

        bool ObstacleInPath(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit, LayerMask mask)
        {
            if (Physics.SphereCast(ray.origin, rayRadius, ray.direction, out hit, perceptionDistance, mask))
            {
                return true;
            }
            return false;
        }


        Vector3 forward;
        Quaternion rot;
        Vector3[] rayDirections;

        Vector3 ObstacleRays(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit, LayerMask mask)
        {
            forward = ray.direction;
            rot = Quaternion.LookRotation(forward);

            for (int i = 0; i < Directions.Length; i++)
            {
                Vector3 dir = rot * (Directions[i]);
                ray.direction = dir;
                if (!Physics.SphereCast(ray, rayRadius, out hit, perceptionDistance, mask))
                {
                    if(drawVisionRays) Debug.DrawLine(ray.origin, ray.origin + ray.direction.normalized * perceptionDistance, Color.yellow);
                    return dir;
                }
                if(drawVisionRays)Debug.DrawLine(ray.origin, ray.origin + ray.direction.normalized * hit.distance, new Color(1,1,1,.3f));

            }

            return forward;
        }

#if UNITY_EDITOR
        protected override void DrawForecastPerceptionGizmo(SteeringAgent agent, float distance)
        {
            DrawCylinderGizmo(agent.shape.radius + clearance, distance);
            Handles.DrawWireDisc(Vector3.forward * distance, Vector3.forward, agent.shape.radius);
            Handles.Label(Vector3.forward * distance + Vector3.up * agent.shape.radius, new GUIContent("Agent Radius"));
            Handles.Label(Vector3.forward * distance + Vector3.up * (agent.shape.radius + clearance), new GUIContent("Clearance"));


        }
#endif
    }
}
