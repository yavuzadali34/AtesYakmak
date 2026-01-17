using UnityEngine;
using UnityEngine.AI;

namespace Entropy.AI
{
    public enum DogState { Idle, Follow, Warn, Flee }

    /// <summary>
    /// Oyuncunun yoldaşı olan köpek. Tehlikeleri (ince buz) önceden sezer.
    /// FSM (Finite State Machine) ile kontrol edilir.
    /// </summary>
    public class DogAI : MonoBehaviour
    {
        [Header("Settings")]
        public DogState currentState = DogState.Follow;
        public float followDistance = 3.0f;
        public float detectionRange = 4.0f;
        public LayerMask dangerLayer; // İnce buz (Thin Ice) katmanı

        [Header("References")]
        public Transform player;
        private NavMeshAgent _agent;
        private Animator _animator; // Opsiyonel animasyon kontrolü

        void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            HandleState();
        }

        private void HandleState()
        {
            switch (currentState)
            {
                case DogState.Idle:
                    _agent.isStopped = true;
                    // Idle animasyonu
                    break;

                case DogState.Follow:
                    _agent.isStopped = false;
                    UpdateMovement();
                    DetectDanger();
                    break;

                case DogState.Warn:
                    _agent.isStopped = true;
                    BarkAndWarn();
                    // Tehlike geçti mi kontrol et
                    if (!IsDangerInFront()) currentState = DogState.Follow;
                    break;

                case DogState.Flee:
                    // Kitaptaki çaresizlik anında köpeğin kaçışı
                    RunAwayFromPlayer();
                    break;
            }
        }

        private void UpdateMovement()
        {
            if (player == null) return;

            float dist = Vector3.Distance(transform.position, player.position);
            if (dist > followDistance)
            {
                _agent.SetDestination(player.position);
            }
        }

        private void DetectDanger()
        {
            if (IsDangerInFront())
            {
                currentState = DogState.Warn;
            }
        }

        private bool IsDangerInFront()
        {
            // Köpeğin burnunun önündeki zemini kontrol et (Raycast)
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            if (Physics.Raycast(rayOrigin, transform.forward, out hit, detectionRange, dangerLayer))
            {
                Debug.Log("<color=yellow>Köpek Tehlike Seziyor: </color>" + hit.collider.gameObject.name);
                return true;
            }

            // GridManager ile de kontrol et (Tilemap)
            if (GridManager.Instance != null && player != null)
            {
                Vector3 checkPos = player.position + player.forward * 1.5f; // Oyuncunun önüne bak
                bool isDangerous = GridManager.Instance.KonumTehlikeliMi(checkPos);
                
                if (isDangerous)
                {
                    Debug.Log("<color=yellow>Köpek Tehlike Seziyor (Grid): İnce Buz!</color>");
                    return true;
                }
            }
            
            return false;
        }

        private void BarkAndWarn()
        {
            // Köpeğin durup havlaması ve oyuncuya bakması
            transform.LookAt(player);
            // AudioSource.PlayClipAtPoint(barkSound, transform.position); // Ses eklenebilir
            Debug.Log("<color=yellow>Hav! Hav! (İnce Buz Uyarı!)</color>");
        }

        private void RunAwayFromPlayer()
        {
            // Oyuncudan ters yöne kaçış
            if (player == null) return;
            Vector3 runDir = transform.position - player.position;
            Vector3 dest = transform.position + runDir.normalized * 10f;
            _agent.SetDestination(dest);
        }

        // Oyuncu tarafından state değiştirilmek istenirse
        public void SetState(DogState newState)
        {
            currentState = newState;
        }
    }
}
