using UnityEngine;
using UnityEngine.InputSystem;

namespace AerodynamicObjects.Tutorials
{
    public class ChaseCamera : MonoBehaviour
    {
        public Transform target;

        [Tooltip("The gain used to move the camera towards its target position. Higher gains will make the camera seem locked to the target in a positional sense. Very low gains will also affect the orbiting behaviour of the camera as it will be slow to move into the new elevation and heading positions.")]
        public float followGain = 10f;

        [Tooltip("How fast the camera stand off distance changes when scrolling the mouse.")]
        public float scrollSpeed = 1f;

        [Tooltip("Look rotation speed affects how quickly the camera rotates to look at the target. Higher values will keep the target in the centre of the camera's view.")]
        public float lookRotationSpeed = 0.1f;

        Vector3 directionForOffset;
        float distance = 0f;
        float initialHeadingAngle;
        Vector3 cameraTargetPosition;

        void Awake()
        {
            if (target == null)
            {
                Destroy(this);
            }

            // The position of the camera relative to the target, used to keep an offset between the two
            directionForOffset = Vector3.Normalize(transform.position - target.position);
            distance = Vector3.Distance(transform.position, target.position);
            initialHeadingAngle = target.eulerAngles.y;
            transform.LookAt(target);
        }

        private void Update()
        {
            // Allow the user to scroll the distance
            distance += GetMouseScroll() * scrollSpeed;

            if (distance < 0)
                distance = 0;
        }

        float GetMouseScroll()
        {
            var mouse = Mouse.current;
            if (mouse != null)
                return mouse.scroll.ReadValue()[1] / 120f;
            else
                return 0;
        }

        void FixedUpdate()
        {
            // Get the target position for the camera by rotating the offset by the heading angle of the target
            cameraTargetPosition = target.position + Quaternion.Euler(0, target.eulerAngles.y - initialHeadingAngle, 0) * (distance * directionForOffset);

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

            // Get the direction we should be looking to see the target
            Vector3 targetLookDirection = target.position - cameraNewPosition;
            targetLookDirection.Normalize();

            // Slerp the current camera rotation towards the rotation it should be at to see the target
            Quaternion lookRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetLookDirection), lookRotationSpeed);

            // Set position and rotation of the camera
            transform.SetPositionAndRotation(cameraNewPosition, lookRotation);

        }
    }
}