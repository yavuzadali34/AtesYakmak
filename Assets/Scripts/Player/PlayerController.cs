using UnityEngine;
using Entropy.Systems;

namespace Entropy.Player
{
    /// <summary>
    /// Oyuncu hareketlerini yönetir ve ThermodynamicsManager ile etkileşime girer.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5.0f;
        public float slowFactor = 1.0f; // Hipotermi veya karda yavaşlama
        
        [Header("References")]
        public ThermodynamicsManager thermoManager;
        
        private CharacterController _controller;
        private Vector3 _moveDirection;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (thermoManager == null) thermoManager = GetComponent<ThermodynamicsManager>();
        }

        void Update()
        {
            HandleInput();
            MoveCharacter();
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            _moveDirection = new Vector2 (horizontal, vertical).normalized;

            // ThermodynamicsManager'a hareket bilgisini gönder
            if (thermoManager != null)
            {
                thermoManager.isMoving = _moveDirection.magnitude > 0.1f;
            }
        }

        private void MoveCharacter()
        {
            // Hipotermi etkilerini simüle et (donmaya başladıkça yavaşlama)
            if (thermoManager != null && thermoManager.bodyTemp < thermoManager.hypothermiaThreshold)
            {
                slowFactor = Mathf.InverseLerp(thermoManager.minViableTemp, thermoManager.hypothermiaThreshold, thermoManager.bodyTemp);
                slowFactor = Mathf.Clamp(slowFactor, 0.3f, 1.0f);
            }
            else
            {
                slowFactor = 1.0f;
            }

            Vector3 velocity = _moveDirection * moveSpeed * slowFactor;
            
            // Yerçekimi (Top-down olsa da Grounded kalmak için)
            if (!_controller.isGrounded)
            {
                velocity.y -= 9.81f * Time.deltaTime;
            }

            _controller.Move(velocity * Time.deltaTime);

            // Karakteri hareket yönüne döndür
            if (_moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        // Çevre etkileşimi için yardımcı metodlar
        public void ApplySlowdown(float factor)
        {
            moveSpeed *= factor;
        }

        public void ResetSpeed()
        {
            moveSpeed = 5.0f;
        }
    }
}
