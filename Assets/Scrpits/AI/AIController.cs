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
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private float _maxYPositionPlayerAI = 1.5f;
    private float _moveSpeed;
    [SerializeField] private float _moveSpeedMin = 2f;
    [SerializeField] private float _moveSpeedMax = 7f;    
    [Space]
    [Header("Pass And Shoot Settings")]    
    [Header("Short Pass Settings")]
    [SerializeField] private float _passDistance = 0.5f;
    [SerializeField] private float _minPassDistanceBetweenPlayerToTarget = 2f;
    [SerializeField] private float _targetToPlayerDistanceMin = 10f;
    [SerializeField] private float _targetToPlayerDistanceMax = 20f;
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
    [Header("Shoot Settings")]
    [SerializeField] private Transform _goal;
    [SerializeField] private float _shootDistanceMin = 5f;
    [SerializeField] private float _shootDistanceMax = 30f;
    [SerializeField] private float _shootPowerMin = 10f;
    [SerializeField] private float _shootPowerMax = 30f;
    [SerializeField] private float _shootHeightMin = 0.25f;
    [SerializeField] private float _shootHeightMax = 0.75f;   
    [SerializeField] private float _shootCheckInterval = 0.5f;
    [Space]
    [Header("Dribbling And Who's Ball")]
    [SerializeField] private float _dribbleDistance = 2f; // Topa do�ru dribbling mesafesi
    private bool _isWaiting = false;
    [SerializeField] private float _waitingDuration = 1f;

    private Rigidbody _rb;
    private AIBall _ballAttachedToPlayer;   // Top oyuncuya bagl�
    public AIBall BallAttachedToPlayer { get => _ballAttachedToPlayer; set => _ballAttachedToPlayer = value; }
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _moveSpeed = Random.Range(_moveSpeedMin, _moveSpeedMax);    // Random  h�z belirleme
        StartCoroutine(nameof(BallFind));   // Top arama
        StartCoroutine(nameof(SelectTargetPlayerAI)); // En yak�n oyuncu arama        
        StartCoroutine(CheckAndShootCoroutine());   // Sut kontol ve cekme
    }
    void Update()
    {
        // E�er karakter yerde de�ilse, yer �ekimi uygula
        if (!IsGrounded())
        {
            Vector3 gravity = _gravityScale * Physics.gravity; // Yer �ekimi kuvveti
            _rb.AddForce(gravity, ForceMode.Acceleration);
        }
        PlayerYAxisLimit();

        LookTheBall();
        if (!_isWaiting)
        {
            Move();
            Pass();                   
        }
        else
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
    void Move()
    {        
        // E�er top rakipteyse, topa ve oyuncuya do�ru hareket et
        if (IsBallWithOpponent())
        {
            Vector3 directionToBallWithDribble = _ball.position - transform.position;
            _rb.velocity = directionToBallWithDribble.normalized * _moveSpeed;
        }
        else
        {
            // E�er top bizdeyse, kaleye do�ru hareket et
            Vector3 directionToBall = _goal.position - transform.position;
            float distanceToBall = directionToBall.magnitude;

            // _dribbleDistance (de�erini oyucunun kaleci=100, defan=50s, orta saha=10 ve forvet=2 olams�na g�re ayarla
            // _dribbleDistance kaleye yakla�abilece�i min uzakl�k) 

            // Top bizde ise, topa do�ru dribbling mesafesine gelene kadar hareket et
            if (distanceToBall > _dribbleDistance)
            {
                _rb.velocity = directionToBall.normalized * _moveSpeed;
            }
            else
            {
                _rb.velocity = Vector3.zero; // Dribbling mesafesine gelindi�inde dur
            }
        }
    }
    void LookTheBall()
    {
        if (_ball != null && _ballAttachedToPlayer == null)
        {
            // Hedef top belirlenmi�se ve oyuncu topa ba�l� de�ilse Topa do�ru bak
            Vector3 lookAtPosition = _ball.position;
            lookAtPosition.y = transform.position.y;
            transform.LookAt(lookAtPosition);
        }
        //else if (_ball != null && _ballAttachedToPlayer != null)
        //{
        //    // Hedef top belirlenmi�se ve oyuncu topa ba�l� ise kaleye do�ru bak
        //    Vector3 lookAtPosition = _goal.position;
        //    lookAtPosition.y = transform.position.y;
        //    transform.LookAt(lookAtPosition);
        //}
        ////else if (_ball != null && _ballAttachedToPlayer != null && _isShortPass || _isLongPass)
        ////{
        ////    // Oyunucuya bak
        ////    Vector3 lookAtPosition = _targetPlayer.position;
        ////    lookAtPosition.y = transform.position.y;
        ////    transform.LookAt(lookAtPosition);
        ////}
        else if (_ball != null && _ballAttachedToPlayer != null)
        {
            if (!_isShortPass && !_isLongPass)
            {
                // Hedef top belirlenmi�se ve oyuncu topa ba�l� ise kaleye do�ru bak
                Vector3 lookAtPosition = _goal.position;
                lookAtPosition.y = transform.position.y;
                transform.LookAt(lookAtPosition);
            }
            else if(_canPass &&_isShortPass || _isLongPass)
            {
                // Oyunucuya bak
                Vector3 lookAtPosition = _targetPlayer.position;
                lookAtPosition.y = transform.position.y;
                transform.LookAt(lookAtPosition);
            }
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
                else if (distanceToTargetPlayer > _minPassDistanceBetweenPlayerToTarget && distanceToTargetPlayer < _targetToPlayerDistanceMin)
                {
                    // Kisa pas
                    _isShortPass = true;
                }
                else
                {
                    _isLongPass = false;
                    _isShortPass = false;
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
        // Pass Mekani�i
        Vector3 directionToTarget = _targetPlayer.position - transform.position;
        directionToTarget.y = Random.Range(minHeight, maxHeight);
        float passPower = Random.Range(minPower, maxPower);
        ballRigidbody.velocity = directionToTarget.normalized * passPower;

        _isLongPass = false;
        _isShortPass = false;
        _canPass = false;
        _ballAttachedToPlayer = null;
        _ball.GetComponent<AIBall>().BallToPlayerAINull();
        PlayerWaiting();
        StartCoroutine(nameof(SelectTargetPlayerAI));
    }
    private IEnumerator CheckAndShootCoroutine()
    {
        while (true)
        {
            if (_ball != null && _ballAttachedToPlayer != null && CanShoot())
            {
                // Belli aral�klar ile kontrol et ve �ut �ekileblir ise �ut �ek
                Shoot();
            }
            yield return new WaitForSeconds(_shootCheckInterval);
        }
    }
    void Shoot()
    {
        // Shoot mekani�i
        Vector3 directionToGoal = _goal.position - _ball.position;
        float distanceToBall = directionToGoal.magnitude;
        directionToGoal.y = Random.Range(_shootHeightMin, _shootHeightMax);
        float shootPower = Random.Range(_shootPowerMin, _shootPowerMax);

        Debug.Log("SHOOT - UZAKLIK: " + distanceToBall);
        _ball.GetComponent<Rigidbody>().velocity = directionToGoal.normalized * shootPower;

        _ballAttachedToPlayer.StickToPlayer = false;
        _ballAttachedToPlayer = null;
        _ball.GetComponent<AIBall>().BallToPlayerAINull();
        PlayerWaiting();
    }
    bool CanShoot()
    {
        Vector3 directionToGoal = _goal.position - _ball.position;
        float distanceToBall = directionToGoal.magnitude;

        float shootDistance = Random.Range(_shootDistanceMin, _shootDistanceMax);
        return distanceToBall < shootDistance;
    }
    bool IsBallWithOpponent()
    {
        // Burada topun nerede oldu�unu ve kimin kontrol�nde oldu�unu belirleyen bir mekanizma olmal�d�r.(A tak�m� B tak�m�)
        // _ball i�indeki TransformPlayerAI dan hangi tak�m oldu�unu isimden bul

        bool isBallWithOpponent = false;
        if(_ball != null)   // Top hedeflenmi�sse
        {
            if (_ballAttachedToPlayer != null)
            {
                // Oyuncu topa ba�l� ise top bizde
                isBallWithOpponent = false;
            }
            else if (_ballAttachedToPlayer == null)
            {
                // Oyuncu topa ba�l� de�ilse top rakipte
                isBallWithOpponent = true;
            }

            //if (this.CompareTag("TeamA"))
            //{                
            //    if (_ballAttachedToPlayer != null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamA"))
            //    {
            //        // Oyuncu topa ba�l� ve top bizim herhangi bir oyuncuda ise top bizde
            //        isBallWithOpponent = false;
            //    }
            //    else if (_ballAttachedToPlayer == null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamB"))
            //    {
            //        // Oyuncu topa ba�l� de�ilse ve top bizim herhangi bir oyuncuda ise top bizde
            //        isBallWithOpponent = false;
            //    }
            //    else if (_ballAttachedToPlayer == null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamB"))
            //    {
            //        // Oyuncu topa ba�l� de�ilse ve top bizim rakip bir oyuncuda ise top rakipte
            //        isBallWithOpponent = true;
            //    }
            //}
            //else
            //{
            //    if (_ballAttachedToPlayer != null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamB"))
            //    {
            //        // Oyuncu topa ba�l� ise top bizde
            //        isBallWithOpponent = false;
            //    }
            //    else if (_ballAttachedToPlayer == null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamA"))
            //    {
            //        // Oyuncu topa ba�l� de�ilse top rakipte
            //        isBallWithOpponent = true;
            //    }
            //}

        }        

        return isBallWithOpponent;
    }
    private bool IsGrounded()
    {
        // Karakterin yerde olup olmad���n� kontrol et
        RaycastHit hit;
        float raycastDistance = 0.1f; // Yer kontrol� i�in raycast mesafesi
        return Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance);
    }
    private void PlayerYAxisLimit()
    {
        // Oyunucun Y eksenindeki pozisyonunu s�n�rla
        Vector3 newPosition = transform.position;
        newPosition.y = Mathf.Min(_maxYPositionPlayerAI, newPosition.y);
        transform.position = newPosition;
    }
    void PlayerWaiting()
    {
        // Pas veya �ut sonras� oyuncu beklemesi
        _isWaiting = true;
        _ball = null;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        StartCoroutine(nameof(BallFind));
    }
    public void PlayerToBallWaiting()
    {
        // Pas ald�ktan sonra veya top al�nd���nda oyuncu beklemesi
        _isWaiting = true;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        StartCoroutine(nameof(Player�sWaiting));
    }
    IEnumerator BallFind()
    {
        // Topu bul
        yield return new WaitForSeconds(_waitingDuration);
        _ball = GameObject.FindWithTag("AIBall").gameObject.transform;
        _isWaiting = false;
    }
    IEnumerator Player�sWaiting()
    {
        // Hareket etmek i�in bekle
        yield return new WaitForSeconds(_waitingDuration);
        _isWaiting = false;
    }
    IEnumerator SelectTargetPlayerAI()
    {
        // Pas verilecek en yak�n oyuncuyu bul
        _targetPlayer = null;
        yield return new WaitForSeconds(1f);

        if (this.CompareTag("TeamA"))
        {
            // Sahadaki t�m oyuncular� bul
            GameObject[] players = GameObject.FindGameObjectsWithTag("TeamA");

            // E�er kendisi oyuncular listesinde varsa, kendisini listeden ��kar (kendi tak�m arkada��na pas yapmak istemeyiz)
            players = players.Where(player => player != gameObject).ToArray();

            // Kendisine olan uzakl���na g�re oyuncular� s�rala
            players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToArray();

            // �lk oyuncuyu hedef olarak se� (yani en yak�n oyuncu)
            if (players.Length > 0)
            {
                _targetPlayer = players[0].transform;
            }
        }
        else
        {
            // Sahadaki t�m oyuncular� bul
            GameObject[] players = GameObject.FindGameObjectsWithTag("TeamB");

            // E�er kendisi oyuncular listesinde varsa, kendisini listeden ��kar (kendi tak�m arkada��na pas yapmak istemeyiz)
            players = players.Where(player => player != gameObject).ToArray();

            // Kendisine olan uzakl���na g�re oyuncular� s�rala
            players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToArray();

            // �lk oyuncuyu hedef olarak se� (yani en yak�n oyuncu)
            if (players.Length > 0)
            {
                _targetPlayer = players[0].transform;
            }
        }
    }
}
