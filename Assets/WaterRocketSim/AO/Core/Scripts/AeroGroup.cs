using UnityEngine;

namespace AerodynamicObjects
{
    [System.Serializable]
    public class AeroGroup : MonoBehaviour
    {
        public AerodynamicGroup group = new AerodynamicGroup();

        // This feels so wrong, but I'm too lazy to figure out a better way to keep
        // track of them. Why can't unity just let me put non-unity references in
        // their object field :(
        public AeroObject[] aeroObjects = new AeroObject[0];
        public AeroObject[] previousAeroObjects = new AeroObject[0];

        private void Awake()
        {
            UpdateGroup();
        }

        public void UpdateGroup()
        {

            // Make sure the real group isn't null
            if (group == null)
            {
                group = new AerodynamicGroup();
            }

            // We need to check if there are any differences between the previous group of objects
            // and the updated group of objects
            for (int i = 0; i < previousAeroObjects.Length; i++)
            {
                if (previousAeroObjects[i] == null)
                {
                    continue;
                }

                if (NotInGroup(previousAeroObjects[i]))
                {
                    // Make sure the aero object is removed from the group
                    previousAeroObjects[i].ao.UnGroup();
                }
            }

            // Handle the case where the array became null
            if (aeroObjects == null)
            {
                aeroObjects = new AeroObject[0];
                // Clunky to delete these but it's for the best
                group.objectPlanformAreas = new double[0];

                return;
            }

            // Copy the updated array so we can do the check again next time
            previousAeroObjects = new AeroObject[aeroObjects.Length];
            for (int i = 0; i < previousAeroObjects.Length; i++)
            {
                previousAeroObjects[i] = aeroObjects[i];
            }

            // We could try to be lazy here and check the lengths of the two arrays,
            // however - if the references have been messed up somewhere along the way
            // there's still a chance the arrays would be the same length. So just do this anyway
            AerodynamicObject[] aerodynamicObjects = new AerodynamicObject[aeroObjects.Length];
            for (int i = 0; i < aeroObjects.Length; i++)
            {
                if (aeroObjects[i] == null)
                {
                    continue;
                }

                // Make sure the dimensions are up to date
                aeroObjects[i].UpdateDimensions();

                aerodynamicObjects[i] = aeroObjects[i].ao;
            }

            group.SetObjectsAsGroup(aerodynamicObjects);
        }

        private bool NotInGroup(AeroObject aeroObject)
        {
            if (aeroObjects == null || aeroObjects.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < aeroObjects.Length; i++)
            {
                if (aeroObject == aeroObjects[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void GetAllChildObjects()
        {
            // First remove existing objects from the group
            if (aeroObjects != null)
            {
                for (int i = 0; i < aeroObjects.Length; i++)
                {
                    aeroObjects[i].ao.UnGroup();
                }
            }


            aeroObjects = GetComponentsInChildren<AeroObject>();

            if (aeroObjects.Length == 0)
            {
                Debug.LogWarning("No aerodynamic objects were found in children of " + gameObject.name);
            }
        }
    }
}