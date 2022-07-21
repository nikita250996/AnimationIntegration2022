using UnityEngine;

namespace Assets.Scripts
{
    public class EnemyController : MonoBehaviour
    {
        [HideInInspector] public bool IsDead { get; private set; }

        [SerializeField] [Range(-49f, 49f)] [Tooltip("Границы по x для точки возрождения.")]
        private float[] _respawnAreaBordersX = {-49f, 49f};

        [SerializeField] [Range(-49f, 49f)] [Tooltip("Границы по z для точки возрождения.")]
        private float[] _respawnAreaBordersZ = {-49f, 49f};

        [SerializeField] [Range(1f, 20f)] [Tooltip("Время возрождения.")]
        private float _respawnTime = 5f;

        [SerializeField] [Tooltip("Капсула для обнаружения врага игроком.")]
        private CapsuleCollider _collider;

        private Rigidbody[] _rigidBodies;

        [SerializeField] private Animator _animator;

        private void Awake()
        {
            _rigidBodies = gameObject.GetComponentsInChildren<Rigidbody>();
        }

        public void OnDeath()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            _animator.enabled = false;
            _collider.enabled = false;

            foreach (Rigidbody rigidBody in _rigidBodies)
            {
                rigidBody.velocity = Vector3.zero;
            }

            Invoke(nameof(Respawn), _respawnTime);
        }

        private void Respawn()
        {
            IsDead = false;
            _animator.enabled = true;
            _collider.enabled = true;

            float x = Random.Range(_respawnAreaBordersX[0], _respawnAreaBordersX[1]);
            float z = Random.Range(_respawnAreaBordersZ[0], _respawnAreaBordersZ[1]);
            transform.SetPositionAndRotation(new Vector3(x, 0f, z), Quaternion.identity);
        }
    }
}
