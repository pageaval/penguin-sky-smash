using UnityEngine;

namespace PSS
{
    /// Follows the penguin horizontally (and a little vertically) with smoothing.
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 startPos = new Vector3(0f, 0f, -10f);
        public float smooth = 6f;
        public float xLead = 2.2f;
        public float yFollow = 0.35f;
        public float minY = 0f, maxY = 6f;

        bool active;

        public void SnapToStart()
        {
            active = false;
            transform.position = startPos;
        }

        public void FollowTarget(Transform t)
        {
            target = t;
            active = true;
        }

        void LateUpdate()
        {
            if (!active || target == null) return;
            float tx = Mathf.Max(startPos.x, target.position.x + xLead); // never scroll left of the launch view
            float ty = Mathf.Clamp(startPos.y + (target.position.y - GameConfig.GroundY) * yFollow, minY, maxY);
            var goal = new Vector3(tx, ty, startPos.z);
            transform.position = Vector3.Lerp(transform.position, goal, smooth * Time.deltaTime);
        }
    }
}
