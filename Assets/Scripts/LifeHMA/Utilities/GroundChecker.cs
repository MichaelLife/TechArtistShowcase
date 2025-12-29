using UnityEngine;
namespace LifeHMA.Utilities
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] private float _groundDistance = 1f;
        [SerializeField] private float _headDistance = 1f;
        [SerializeField] private float _radius = 1f;
        [SerializeField] private float _headRadius = 1f;
        [SerializeField] private LayerMask _groundLayers;

        public bool showGizmo;

        public bool IsGrounded { get; private set; }

        private void Update()
        {
            IsGrounded = Physics.CheckSphere(transform.position + Vector3.down * _groundDistance, _radius, _groundLayers);
        }

        public bool HeadChecker()
        {
            return Physics.CheckSphere(transform.position + Vector3.up * _headDistance, _headRadius, _groundLayers);
        }

        private void OnDrawGizmos()
        {
            if (showGizmo)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position + Vector3.down * _groundDistance, _radius);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + Vector3.up * _headDistance, _headRadius);
            }
        }
    }
}
