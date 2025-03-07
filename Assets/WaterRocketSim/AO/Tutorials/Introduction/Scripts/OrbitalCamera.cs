using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Tutorials
{
    public class OrbitalCamera : MonoBehaviour
    {
        public Transform target;

        [Tooltip("The gain used to move the camera towards its target position. Higher gains will make the camera seem locked to the target in a positional sense. Very low gains will also affect the orbiting behaviour of the camera as it will be slow to move into the new elevation and heading positions.")]
        public float followGain = 10f;

        [Tooltip("How fast the camera stand off distance changes when scrolling the mouse.")]
        public float scrollSpeed = 1f;

        [Tooltip("Look rotation speed affects how quickly the camera rotates to look at the target. Higher values will keep the target in the centre of the camera's view.")]
        public float lookRotationSpeed = 0.1f;

        [Tooltip("How fast the elevation angle changes when dragging the mouse vertically.")]
        public float elevationSpeed = 0.25f;

        [Tooltip("How fast the heading angle changes when dragging the mouse horizontally.")]
        public float headingSpeed = 0.25f;

        Vector3 mouseDelta;

        [Tooltip("The elevation angle of the orbital camera (deg).")]
        public float elevation = 10f;
        float elevationRad;

        [Tooltip("The heading angle of the orbital camera (deg).")]
        public float heading = 0f;
        float headingRad;

        [Tooltip("How far away the camera should be from the target (m).")]
        public float distance = 1f;

        void Awake()
        {
            if (target == null)
            {
                Destroy(this);
            }
            transform.LookAt(target);
        }

        Vector3 GetMousePosition()
        {
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.position.ReadValue();
            else
                return Vector3.zero;
        }

        Vector3 GetMouseDelta()
        {
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.delta.ReadValue();
            else
                return Vector3.zero;
        }

        float GetMouseScroll()
        {
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.scroll.ReadValue()[1] / 120f;
            else
                return 0;
        }

        bool GetMouseLeftClick()
        {
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.leftButton.ReadValue() == 1f;
            else
                return false;
        }



        private void Update()
        {
            // Use left click to drag the camera around
            if (GetMouseLeftClick())
            {
                // Get mouse input
                mouseDelta = GetMouseDelta();
                elevation -= mouseDelta.y * elevationSpeed;
                heading += mouseDelta.x * headingSpeed;
            }
            distance += GetMouseScroll() * scrollSpeed;
            if (distance < 0)
                distance = 0;
        }

        Vector3 cameraTargetPosition;
        void FixedUpdate()
        {
            // Get the target position for the camera based on the elevation, heading and distance
            elevationRad = Mathf.Deg2Rad * elevation;
            headingRad = Mathf.Deg2Rad * heading;
            Vector3 direction = new Vector3(Mathf.Cos(elevationRad) * Mathf.Sin(headingRad), Mathf.Sin(elevationRad), Mathf.Cos(elevationRad) * Mathf.Cos(headingRad)).normalized;
            cameraTargetPosition = target.position + distance * direction;

            // Move the camera towards its target position using the follow gain
            Vector3 toTarget = cameraTargetPosition - transform.position;
            Vector3 movementToTarget = followGain * Time.fixedDeltaTime * toTarget;

            // If we're going to move the camera further than the total distance to the target position
            // then we'll overshoot - so just snap the camera to the target position
            Vector3 cameraNewPosition;
            if (movementToTarget.magnitude > toTarget.magnitude)
            {
                cameraNewPosition = cameraTargetPosition;
            }
            else
            {
                cameraNewPosition = transform.position + movementToTarget;
            }

            // Adding these changes so the camera actually looks at the CG...
            Vector3 lookDirection = target.position - cameraNewPosition;
            lookDirection.Normalize();
            Quaternion lookRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), lookRotationSpeed);

            transform.SetPositionAndRotation(cameraNewPosition, lookRotation);
        }

        // This allows the user to tweak the initial conditions and set up the camera how they want
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            // Get the target position for the camera based on the elevation, heading and distance
            elevationRad = Mathf.Deg2Rad * elevation;
            headingRad = Mathf.Deg2Rad * heading;
            Vector3 direction = new Vector3(Mathf.Cos(elevationRad) * Mathf.Sin(headingRad), Mathf.Sin(elevationRad), Mathf.Cos(elevationRad) * Mathf.Cos(headingRad)).normalized;
            transform.position = target.position + distance * direction;
            transform.LookAt(target);
        }
    }
}