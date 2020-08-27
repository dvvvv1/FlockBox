﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{
    public interface ISteeringBehaviorComponentData
    {
        float3 GetSteering(AgentData mine, SteeringData steering, DynamicBuffer<NeighborData> neighbors);
        PerceptionData AddPerceptionRequirements(AgentData mine, PerceptionData perception);
    }
}