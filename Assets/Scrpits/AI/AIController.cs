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
    [SerializeField] private float _dribbleDistance = 2f; // Topa doðru dribbling mesafesi
    private bool _isWaiting = false;
    [SerializeField] private float _waitingDuration = 1f;

    private Rigidbody _rb;
    private AIBall _ballAttachedToPlayer;   // Top oyuncuya baglý
    public AIBall BallAttachedToPlayer { get => _ballAttachedToPlayer; set => _ballAttachedToPlayer = value; }
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _moveSpeed = Random.Range(_moveSpeedMin, _moveSpeedMax);    // Random  hýz belirleme
        StartCoroutine(nameof(BallFind));   // Top arama
        StartCoroutine(nameof(SelectTargetPlayerAI)); // En yakýn oyuncu arama        
        StartCoroutine(CheckAndShootCoroutine());   // Sut kontol ve cekme
    }
    void Update()
    {
        // Eðer karakter yerde deðilse, yer çekimi uygula
        if (!IsGrounded())
        {
            Vector3 gravity = _gravityScale * Physics.gravity; // Yer çekimi kuvveti
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
        // Eðer top rakipteyse, topa ve oyuncuya doðru hareket et
        if (IsBallWithOpponent())
        {
            Vector3 directionToBallWithDribble = _ball.position - transform.position;
            _rb.velocity = directionToBallWithDribble.normalized * _moveSpeed;
        }
        else
        {
            // Eðer top bizdeyse, kaleye doðru hareket et
            Vector3 directionToBall = _goal.position - transform.position;
            float distanceToBall = directionToBall.magnitude;

            // _dribbleDistance (deðerini oyucunun kaleci=100, defan=50s, orta saha=10 ve forvet=2 olamsýna göre ayarla
            // _dribbleDistance kaleye yaklaþabileceði min uzaklýk) 

            // Top bizde ise, topa doðru dribbling mesafesine gelene kadar hareket et
            if (distanceToBall > _dribbleDistance)
            {
                _rb.velocity = directionToBall.normalized * _moveSpeed;
            }
            else
            {
                _rb.velocity = Vector3.zero; // Dribbling mesafesine gelindiðinde dur
            }
        }
    }
    void LookTheBall()
    {
        if (_ball != null && _ballAttachedToPlayer == null)
        {
            // Hedef top belirlenmiþse ve oyuncu topa baðlý deðilse Topa doðru bak
            Vector3 lookAtPosition = _ball.position;
            lookAtPosition.y = transform.position.y;
            transform.LookAt(lookAtPosition);
        }
        //else if (_ball != null && _ballAttachedToPlayer != null)
        //{
        //    // Hedef top belirlenmiþse ve oyuncu topa baðlý ise kaleye doðru bak
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
                // Hedef top belirlenmiþse ve oyuncu topa baðlý ise kaleye doðru bak
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
        // Pass Mekaniði
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
                // Belli aralýklar ile kontrol et ve þut çekileblir ise þut çek
                Shoot();
            }
            yield return new WaitForSeconds(_shootCheckInterval);
        }
    }
    void Shoot()
    {
        // Shoot mekaniði
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
        // Burada topun nerede olduðunu ve kimin kontrolünde olduðunu belirleyen bir mekanizma olmalýdýr.(A takýmý B takýmý)
        // _ball içindeki TransformPlayerAI dan hangi takým olduðunu isimden bul

        bool isBallWithOpponent = false;
        if(_ball != null)   // Top hedeflenmiþsse
        {
            if (_ballAttachedToPlayer != null)
            {
                // Oyuncu topa baðlý ise top bizde
                isBallWithOpponent = false;
            }
            else if (_ballAttachedToPlayer == null)
            {
                // Oyuncu topa baðlý deðilse top rakipte
                isBallWithOpponent = true;
            }

            //if (this.CompareTag("TeamA"))
            //{                
            //    if (_ballAttachedToPlayer != null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamA"))
            //    {
            //        // Oyuncu topa baðlý ve top bizim herhangi bir oyuncuda ise top bizde
            //        isBallWithOpponent = false;
            //    }
            //    else if (_ballAttachedToPlayer == null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamB"))
            //    {
            //        // Oyuncu topa baðlý deðilse ve top bizim herhangi bir oyuncuda ise top bizde
            //        isBallWithOpponent = false;
            //    }
            //    else if (_ballAttachedToPlayer == null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamB"))
            //    {
            //        // Oyuncu topa baðlý deðilse ve top bizim rakip bir oyuncuda ise top rakipte
            //        isBallWithOpponent = true;
            //    }
            //}
            //else
            //{
            //    if (_ballAttachedToPlayer != null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamB"))
            //    {
            //        // Oyuncu topa baðlý ise top bizde
            //        isBallWithOpponent = false;
            //    }
            //    else if (_ballAttachedToPlayer == null && _ball.GetComponent<AIBall>().TransformPlayerAI.CompareTag("TeamA"))
            //    {
            //        // Oyuncu topa baðlý deðilse top rakipte
            //        isBallWithOpponent = true;
            //    }
            //}

        }        

        return isBallWithOpponent;
    }
    private bool IsGrounded()
    {
        // Karakterin yerde olup olmadýðýný kontrol et
        RaycastHit hit;
        float raycastDistance = 0.1f; // Yer kontrolü için raycast mesafesi
        return Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance);
    }
    private void PlayerYAxisLimit()
    {
        // Oyunucun Y eksenindeki pozisyonunu sýnýrla
        Vector3 newPosition = transform.position;
        newPosition.y = Mathf.Min(_maxYPositionPlayerAI, newPosition.y);
        transform.position = newPosition;
    }
    void PlayerWaiting()
    {
        // Pas veya þut sonrasý oyuncu beklemesi
        _isWaiting = true;
        _ball = null;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        StartCoroutine(nameof(BallFind));
    }
    public void PlayerToBallWaiting()
    {
        // Pas aldýktan sonra veya top alýndýðýnda oyuncu beklemesi
        _isWaiting = true;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        StartCoroutine(nameof(PlayerÝsWaiting));
    }
    IEnumerator BallFind()
    {
        // Topu bul
        yield return new WaitForSeconds(_waitingDuration);
        _ball = GameObject.FindWithTag("AIBall").gameObject.transform;
        _isWaiting = false;
    }
    IEnumerator PlayerÝsWaiting()
    {
        // Hareket etmek için bekle
        yield return new WaitForSeconds(_waitingDuration);
        _isWaiting = false;
    }
    IEnumerator SelectTargetPlayerAI()
    {
        // Pas verilecek en yakýn oyuncuyu bul
        _targetPlayer = null;
        yield return new WaitForSeconds(1f);

        if (this.CompareTag("TeamA"))
        {
            // Sahadaki tüm oyuncularý bul
            GameObject[] players = GameObject.FindGameObjectsWithTag("TeamA");

            // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendi takým arkadaþýna pas yapmak istemeyiz)
            players = players.Where(player => player != gameObject).ToArray();

            // Kendisine olan uzaklýðýna göre oyuncularý sýrala
            players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToArray();

            // Ýlk oyuncuyu hedef olarak seç (yani en yakýn oyuncu)
            if (players.Length > 0)
            {
                _targetPlayer = players[0].transform;
            }
        }
        else
        {
            // Sahadaki tüm oyuncularý bul
            GameObject[] players = GameObject.FindGameObjectsWithTag("TeamB");

            // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendi takým arkadaþýna pas yapmak istemeyiz)
            players = players.Where(player => player != gameObject).ToArray();

            // Kendisine olan uzaklýðýna göre oyuncularý sýrala
            players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToArray();

            // Ýlk oyuncuyu hedef olarak seç (yani en yakýn oyuncu)
            if (players.Length > 0)
            {
                _targetPlayer = players[0].transform;
            }
        }
    }
}
