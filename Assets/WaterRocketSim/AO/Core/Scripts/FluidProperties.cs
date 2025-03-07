using UnityEngine;

namespace AerodynamicObjects
{
    [CreateAssetMenu(fileName = "Fluid Properties", menuName = "Aerodynamic Objects/Fluid Properties", order = 100)]
    public class FluidProperties : ScriptableObject
    {
        public float density = 1.23f;
        public float pressure = 101325f;
        public float dynamicViscosity = 1.8e-5f;
    }
}