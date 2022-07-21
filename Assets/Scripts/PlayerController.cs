using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] [Tooltip("Кость, вокруг которой вращается верхняя часть «тела».")]
        private Transform _torsoBone;

        private Vector3 _torsoBoneRotation;

        [SerializeField] [Range(4f, 10f)] [Tooltip("Скорость ходьбы.")]
        private float _movementSpeed = 5f;

        [SerializeField] [Tooltip("Слот для оружия.")]
        private MeshFilter _weaponSocket;

        [SerializeField] [Tooltip("Модель автомата.")]
        private Mesh _assaultRifle;

        [SerializeField] [Tooltip("Модель меча.")]
        private Mesh _sword;

        [SerializeField] [Tooltip("Капсула для обнаружения врага.")]
        private CapsuleCollider _collider;

        [SerializeField] [Range(0.8f, 7f)] [Tooltip("На каком расстоянии можно начать добивание.")]
        private float _enemyDetectionDistance = 5f;

        private EnemyController _enemy;
        private Vector3 _positionBehindTheEnemy;
        private bool _isWaiting;

        [SerializeField]
        [Range(1.5f, 2.5f)]
        [Tooltip("Во сколько раз ускоряется движение при беге ко врагу во время добивания.")]
        private float _acceleration = 2f;

        private float _runningSpeed;

        [SerializeField] [Tooltip("Интерфейс с информацией о добивании врага.")]
        private GameObject _finishingUI;

        [SerializeField]
        [Range(1.2f, 1.7f)]
        [Tooltip("Коэффициент для регулирования расстояния между персонажем и игроком во время добивания.")]
        private float _distanceToEnemyCoefficient = 1.5f;

        [SerializeField] private Animator _animator;
        private float _finishingAnimationDuration;
        private float _killingTime;
        private bool _isKilling;

        private Camera _camera;

        private void Awake()
        {
            AnimationClip[] clips = _animator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (!clip.name.Equals("Finishing"))
                {
                    continue;
                }

                _finishingAnimationDuration = clip.length;
                _killingTime = _finishingAnimationDuration * 0.5f;
            }

            _runningSpeed = _movementSpeed * _acceleration;

            _collider.radius = _enemyDetectionDistance;
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void Update()
        {
            if (_isKilling)
            {
                Finish();
            }
            else if (!_isWaiting)
            {
                Move();
            }
        }

        private void Finish()
        {
            transform.position = Vector3.MoveTowards(
                transform.position, _positionBehindTheEnemy, _runningSpeed * Time.deltaTime);

            Vector3 direction = _enemy.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;

            if (Vector3.Distance(transform.position, _positionBehindTheEnemy) > 0.001f)
            {
                return;
            }

            _isKilling = false;
            _animator.SetBool("IsFinishing", true);
            _isWaiting = true;

            Invoke(nameof(DeathNotification), _killingTime);
            Invoke(nameof(Stop), _finishingAnimationDuration);
        }

        private void DeathNotification()
        {
            _enemy.OnDeath();
        }

        private void Stop()
        {
            _animator.SetBool("IsFinishing", false);
            _isWaiting = false;

            _weaponSocket.mesh = _assaultRifle;

            _enemy = null;
        }

        private void Move()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            _animator.SetFloat("Horizontal", horizontal);
            _animator.SetFloat("Vertical", vertical);

            if (horizontal != 0f || vertical != 0f)
            {
                Vector3 movement = new(horizontal, 0.0f, vertical);
                Vector3 movementRotated = Quaternion.AngleAxis(_camera.transform.eulerAngles.y, Vector3.up) * movement;
                if (movement.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(movementRotated);
                }

                transform.Translate(_movementSpeed * Time.deltaTime * movementRotated, Space.World);
            }

            if (!_finishingUI.activeInHierarchy || !Input.GetKeyDown(KeyCode.Space) || _enemy == null)
            {
                return;
            }

            _animator.SetFloat("Horizontal", (_positionBehindTheEnemy - transform.position).x);
            _animator.SetFloat("Vertical", (_positionBehindTheEnemy - transform.position).y);

            _finishingUI.SetActive(false);

            _isKilling = true;

            _weaponSocket.mesh = _sword;
        }

        private void LateUpdate()
        {
            if (_isKilling || _isWaiting || _animator.GetCurrentAnimatorStateInfo(0).IsName("Finishing"))
            {
                return;
            }

            RotateToCursor();
        }

        private void RotateToCursor()
        {
            Vector3 directionToCursor = Input.mousePosition - _camera.WorldToScreenPoint(_torsoBone.position);
            float angle = -Mathf.Atan2(directionToCursor.y, directionToCursor.x) * Mathf.Rad2Deg - 45f;
            _torsoBone.rotation =
                Quaternion.AngleAxis(angle, Vector3.up) * Quaternion.Euler(0f, 0f, -90f) *
                Quaternion.Euler(90f, 0f, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Enemy
            if (other.gameObject.layer != 6)
            {
                return;
            }

            EnemyController newEnemy = other.gameObject.GetComponent<EnemyController>();
            if (newEnemy == null || newEnemy.IsDead)
            {
                return;
            }

            _enemy = newEnemy;
            _finishingUI.SetActive(true);
            _positionBehindTheEnemy =
                _enemy.transform.position - _distanceToEnemyCoefficient * _enemy.transform.forward;
        }

        private void OnTriggerExit(Collider other)
        {
            // Enemy
            if (other.gameObject.layer != 6)
            {
                return;
            }

            _finishingUI.SetActive(false);
            _enemy = null;
        }
    }
}
