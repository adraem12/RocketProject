namespace AerodynamicObjects
{
    public static class NumericsToUnity
    {
        /// <summary>
        /// Convert this UnityEngine Vector3 into a AerodynamicObjects.Numerics Vector3
        /// </summary>
        /// <param name="vector3">Unity Vector3 to convert</param>
        /// <returns>The resulting AerodynamicObjects.Numerics Vector3</returns>
        public static Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 vector3)
        {
            return new Numerics.Vector3(vector3.x, vector3.y, vector3.z);
        }

        /// <summary>
        /// Convert this AerodynamicObjects.Numerics Vector3 into a UnityEngine Vector3
        /// </summary>
        /// <param name="vector3">AerodynamicObjects.Numerics Vector3 to convert</param>
        /// <returns>The resulting Unity Vector3</returns>
        public static UnityEngine.Vector3 ToUnity(this Numerics.Vector3 vector3)
        {
            return new UnityEngine.Vector3((float)vector3.X, (float)vector3.Y, (float)vector3.Z);
        }

        /// <summary>
        /// Convert this AerodynamicObjects.Numerics Quaternion into a UnityEngine Quaternion
        /// </summary>
        /// <param name="quaternion">AerodynamicObjects.Numerics Quaternion to convert</param>
        /// <returns>The resulting Unity Quaternion</returns>
        public static UnityEngine.Quaternion ToUnity(this Numerics.Quaternion quaternion)
        {
            return new UnityEngine.Quaternion((float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z, (float)quaternion.W);
        }

        /// <summary>
        /// Convert this UnityEngine Quaternion into a AerodynamicObjects.Numerics Quaternion
        /// </summary>
        /// <param name="quaternion">Unity Quaternion to convert</param>
        /// <returns>The resulting AerodynamicObjects.Numerics Quaternion</returns>
        public static Numerics.Quaternion ToNumerics(this UnityEngine.Quaternion quaternion)
        {
            return new Numerics.Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
    }
}