using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [Space]
    [Header("AIController Settings")]
    [SerializeField] private Transform _ball;
    [SerializeField] private Transform _targetPlayer;
    private float _moveSpeed;
    [SerializeField] private float _moveSpeedMin = 2f;
    [SerializeField] private float _moveSpeedMax = 7f;
    [SerializeField] private float _targetToPlayerDistanceMin = 10f;
    [SerializeField] private float _targetToPlayerDistanceMax = 20f;
    [Space]
    [Header("Pass And Shoot Settings")]    
    [SerializeField] private float _passDistance = 0.5f;
    [Header("Short Pass Settings")]
    [SerializeField] private float _shortPassPowerMin = 8f;
    [SerializeField] private float _shortPassPowerMax = 12f;
    [SerializeField] private float _shortPassHeightMin = 0f;
    [SerializeField] private float _shortPassHeightMax = 5f;
    [Header("Long Pass Settings")]
    [SerializeField] private float _longPassPowerMin = 15f;
    [SerializeField] private float _longPassPowerMax = 25f;
    [SerializeField] private float _longPassHeightMin = 0f;
    [SerializeField] private float _longPassHeightMax = 8f;

    [SerializeField] private bool _canPass = true;
    private bool _isLongPass = false;
    private bool _isShortPass = false;

    [SerializeField] private float _gravityScale = 1f;

    private Rigidbody _rb;
    private AIBall _ballAttachedToPlayer;
    public AIBall BallAttachedToPlayer { get => _ballAttachedToPlayer; set => _ballAttachedToPlayer = value; }
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        StartCoroutine(nameof(BallFind));
        StartCoroutine(nameof(SelectTargetPlayerAI));
        _moveSpeed = Random.Range(_moveSpeedMin, _moveSpeedMax);
    }
    void Update()
    {
        // Eðer karakter yerde deðilse, yer çekimi uygula
        if (!IsGrounded())
        {
            Vector3 gravity = _gravityScale * Physics.gravity; // Yer çekimi kuvveti
            _rb.AddForce(gravity, ForceMode.Acceleration);
        }

        Move();
        Pass();
    }
    void Move()
    {
        if (_ball != null)
        {
            Vector3 directionToBall = _ball.position - transform.position;
            float distanceToBall = directionToBall.magnitude;
            
            _rb.velocity = directionToBall.normalized * _moveSpeed;
        }
    }
    void Pass()
    {
        if (_ball != null & _targetPlayer != null)
        {
            Vector3 directionToBall = _ball.position - transform.position;
            float distanceToBall = directionToBall.magnitude;

            Vector3 directionToTargetPlayer = _targetPlayer.position - transform.position;
            float distanceToTargetPlayer = directionToTargetPlayer.magnitude;

            if (distanceToBall <= _passDistance)
            {
                // Pas yap
                _canPass = true;
                if (distanceToTargetPlayer > _targetToPlayerDistanceMin && distanceToTargetPlayer < _targetToPlayerDistanceMax)
                {
                    // Uzun pas
                    _isLongPass = true;
                }
                else if (distanceToTargetPlayer < _targetToPlayerDistanceMin)
                {
                    // Kisa pas
                    _isShortPass = true;
                }
                PerformPass();
            }
        }
    }
    void PerformPass()
    {
        if (_canPass && _ballAttachedToPlayer != null)
        {
            _ballAttachedToPlayer.StickToPlayer = false;
            Rigidbody ballRigidbody = _ball.GetComponent<Rigidbody>();
            if (_isLongPass)
            {
                Debug.Log("Uzun Pass");
                PassToTarget(ballRigidbody, _longPassHeightMin, _longPassHeightMax, _longPassPowerMin, _longPassPowerMax);
            }
            else if (_isShortPass)
            {
                Debug.Log("Kisa Pass");
                PassToTarget(ballRigidbody, _shortPassHeightMin, _shortPassHeightMax, _shortPassPowerMin, _shortPassPowerMax);
            }
        }
    }
    void PassToTarget(Rigidbody ballRigidbody, float minHeight, float maxHeight, float minPower, float maxPower)
    {
        Vector3 directionToTarget = _targetPlayer.position - transform.position;
        directionToTarget.y = Random.Range(minHeight, maxHeight);
        float passPower = Random.Range(minPower, maxPower);
        ballRigidbody.velocity = directionToTarget.normalized * passPower;

        _isLongPass = false;
        _isShortPass = false;
        _canPass = false;
        _ballAttachedToPlayer = null;
        _ball.GetComponent<AIBall>().BallToPlayerAINull();
        BallDuration();        
        StartCoroutine(nameof(SelectTargetPlayerAI));
    }
    private bool IsGrounded()
    {
        // Karakterin yerde olup olmadýðýný kontrol et
        RaycastHit hit;
        float raycastDistance = 0.1f; // Yer kontrolü için raycast mesafesi
        return Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance);
    }
    void BallDuration()
    {
        _ball = null;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        StartCoroutine(nameof(BallFind));
    }
    IEnumerator BallFind()
    {        
        yield return new WaitForSeconds(1f);
        _ball = GameObject.FindWithTag("AIBall").gameObject.transform;
    }
    IEnumerator SelectTargetPlayerAI()
    {
        _targetPlayer = null;
        yield return new WaitForSeconds(1f);
        // Sahadaki tüm oyuncularý bul
        GameObject[] players = GameObject.FindGameObjectsWithTag("AIPlayer");

        // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendi takým arkadaþýna pas yapmak istemeyiz)
        players = players.Where(player => player != gameObject).ToArray();

        // Topa olan uzaklýðýna göre oyuncularý sýrala
        players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToArray();

        // Ýlk oyuncuyu hedef olarak seç (yani en yakýn oyuncu)
        if (players.Length > 0)
        {
            _targetPlayer = players[0].transform;
        }
    }
}
