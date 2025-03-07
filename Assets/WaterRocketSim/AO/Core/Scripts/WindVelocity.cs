using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects
{
    [System.Serializable]
    public class WindVelocity
    {
        [Range(0, 10)]
        [Tooltip("Wind speed magnitude in m/s")]
        public float speed = 1f;
        [Tooltip("Horizontal direction of wind in degrees from North. A setting of 90 degrees is an Easterly wind")]
        public float azimuth;
        [Tooltip("Vertical direction of the wind in degress from horizontal. A setting of 90 degrees makes the wind vertical upwards")]
        public float elevation;

        public Vector3 GetVelocity()
        {
            return new Vector3(speed * -Mathf.Sin(Mathf.Deg2Rad * azimuth) * Mathf.Cos(Mathf.Deg2Rad * elevation),
                                   speed * Mathf.Sin(Mathf.Deg2Rad * elevation),
                                   speed * -Mathf.Cos(Mathf.Deg2Rad * azimuth) * Mathf.Cos(Mathf.Deg2Rad * elevation));
        }
    }
}