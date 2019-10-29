﻿using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class Shape
    {
        public enum ShapeType
        {
            POINT, //occupy only one point, one neighborhood
            SPHERE, //occupy all neighborhoods within radius
            LINE, //occupy all neighborhoods along line
            BOX,
        }
        public ShapeType type;

        public float radius = 1;
        public float length = 1;
        public Vector3Int dimensions = Vector3Int.one;

        public bool Intersects(Shape other)
        {
            return false;
        }

#if UNITY_EDITOR
        public void DrawGizmo()
        {
            switch (type)
            {
                case ShapeType.BOX:
                    Gizmos.DrawWireCube(Vector3.zero, dimensions);
                    break;
                case ShapeType.LINE:
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward * length);
                    UnityEditor.Handles.DrawWireDisc(Vector3.forward * length, Vector3.forward, 1);
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.forward, 1);

                    break;
                case ShapeType.POINT:
                    break;
                case ShapeType.SPHERE:
                    Gizmos.DrawWireSphere(Vector3.zero, radius);
                    break;

            }
        }
#endif
    }
}
