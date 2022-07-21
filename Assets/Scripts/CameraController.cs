using UnityEngine;

namespace Assets.Scripts
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] [Tooltip("”правл€емый игроком персонаж.")]
        private GameObject _player;

        private Vector3 _offsetFromPlayer;

        private void Start()
        {
            _offsetFromPlayer = transform.position - _player.transform.position;
        }

        private void LateUpdate()
        {
            transform.position = _player.transform.position + _offsetFromPlayer;
        }
    }
}
