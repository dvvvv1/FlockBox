﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public class ExternalAgent : Agent
    {
        [SerializeField]
        private FlockBox _autoJoinFlockBox;
        private Vector3 _lastPosition;

        protected override FlockBox AutoFindFlockBox()
        {
            return _autoJoinFlockBox;
        }

        protected override void JoinFlockBox(FlockBox flockBox)
        {
            FlockBox = flockBox;
        }

        protected override void LateUpdate()
        {
            if (isAlive && transform.hasChanged)
            {
                if (_autoJoinFlockBox != null)
                {
                    Position = FlockBox.transform.InverseTransformPoint(transform.position);
                    Velocity = FlockBox.transform.InverseTransformDirection((transform.position - _lastPosition)/Time.deltaTime);
                    ValidateVelocity();
                    if(Velocity != Vector3.zero)
                    {
                        Forward = Velocity.normalized;
                    }
                }
                if (ValidatePosition())
                {
                    FindOccupyingCells();
                }
                else
                {
                    RemoveFromAllCells();
                }
                transform.hasChanged = false;
            }
            _lastPosition = transform.position;

        }
    }
}
