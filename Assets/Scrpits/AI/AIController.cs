using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static AIPlayerTeam;

public class AIController : MonoBehaviour
{
    [Space]
    [Header("AIController Settings")]
    [SerializeField] private Transform _ball;
    [SerializeField] private Transform _targetPlayer;
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private float _maxYPositionPlayerAI = 1.5f;
    [SerializeField] private AIPlayerState _currentState;
    private float _moveSpeed;
    [SerializeField] private float _moveSpeedMin = 2f;
    [SerializeField] private float _moveSpeedMax = 7f;
    [Space]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Transform _groundCheckTransform;
    [Space]
    [Header("Pass And Shoot Settings")]    
    [SerializeField] private float _passDistance = 0.5f;
    [SerializeField] private float _passCheckInterval = 0.5f;
    [SerializeField] private float _minPassDistanceBetweenPlayerToTarget = 2f;  // Pas vermesi için min mesafe
    [SerializeField] private float _passDistanceToTargetPlayerMin = 10f;    // Pas vermesi için hedef oyunuc ile arasýndaki min mesafesi
    [SerializeField] private float _passDistanceToTargetPlayerMax = 20f;    // Pas vermesi için hedef oyunuc ile arasýndaki max mesafesi
    [Header("Short Pass Settings")]
    [SerializeField] private float _passDistanceToShortMin = 5f;
    [SerializeField] private float _passDistanceToShortMax = 10f;
    [SerializeField] private float _shortPassPowerMin = 8f;     // Min Kýsa Pas gücü
    [SerializeField] private float _shortPassPowerMax = 12f;    // Max Kýsa Pas gücü
    [SerializeField] private float _shortPassHeightMin = 0f;    // Min Kýsa Pas yüksekliði
    [SerializeField] private float _shortPassHeightMax = 5f;    // Max Kýsa Pas yyüksekliði
    [Header("Long Pass Settings")]
    [SerializeField] private float _passDistanceToLongMin = 10f;
    [SerializeField] private float _passDistanceToLongMax = 30f;
    [SerializeField] private float _longPassPowerMin = 15f; // Min Uzun Pas gücü
    [SerializeField] private float _longPassPowerMax = 25f; // Max Uzun Pas gücü
    [SerializeField] private float _longPassHeightMin = 0f; // Min Uzun Pas yüksekliði
    [SerializeField] private float _longPassHeightMax = 8f; // Max Uzun Pas yüksekliði
    [Header("Shoot Settings")]
    [SerializeField] private Transform _goal;
    [SerializeField] private float _shootCheckInterval = 0.5f;
    [SerializeField] private float _shootDistanceMin = 5f;  // Min Þut mesafesi
    [SerializeField] private float _shootDistanceMax = 30f; // Max Þut mesafesi
    [SerializeField] private float _shootPowerMin = 10f;    // Min Þut gücü
    [SerializeField] private float _shootPowerMax = 30f;    // Min Þut gücü
    [SerializeField] private float _shootHeightMin = 0.25f; // Min Þut yüksekliði
    [SerializeField] private float _shootHeightMax = 0.75f; // Max Þut  yüksekliði
    private bool _canPass = true;
    private bool _isLongPass = false;
    private bool _isShortPass = false;

    [Space]
    [Header("Dribbling And Who's Ball")]
    [SerializeField] private float _dribbleToAttackGoalDistance = 2f; // Rakip Kaleye yaklaþabileceði min uzaklýk) 
    // Forvet:5-8   OrtaSaha:20-25   Defans:45-50   Kaleci:80
    [SerializeField] private float _dribbleToDefenceGoalDistance = 2f; // Kendi Kalesi yaklaþabileceði min uzaklýk) 
    // Forvet:40-45  OrtaSaha:20-25   Defans:5-10   Kaleci:0-5
    private bool _isWaiting = false;
    [SerializeField] private float _waitingDuration = 1f;

    [Space]
    [Header("Teams")]
    public bool isAttack = false;
    [SerializeField] private AITeams _teams;
    [SerializeField] private List<GameObject> _teamA = new List<GameObject>();
    [SerializeField] private List<GameObject> _teamB = new List<GameObject>();
    [SerializeField] private Transform _teamAGoal;
    [SerializeField] private Transform _teamBGoal;
    private AIPlayerTeam.Team _playerTeam;  // oyunucunun takýmý
    
    Vector3 _directionToDefenseGoal; // Team Defense Goal
    Vector3 _directionToAttackGoal;  // Team Attack Goal
    Vector3 _directionToBall; // Top pozisyonu

    private Rigidbody _rb;
    private AIAnimatoinController _animController;

    private AIBall _ballAttachedToPlayer;   // Top oyuncuya baglý    
    public AIBall BallAttachedToPlayer { get => _ballAttachedToPlayer; set => _ballAttachedToPlayer = value; }
    public AIPlayerState CurrentState { get => _currentState; set => _currentState = value; }

    private void Awake()
    {
        this._animController = this.GetComponent<AIAnimatoinController>();
        _currentState = AIPlayerState.Idle; // Baþlangýç durumunu ayarlayabilirsiniz.
    }
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        AIPlayerTeam aiPlayerTeam = GetComponent<AIPlayerTeam>();
        if (aiPlayerTeam != null)
        {
            _playerTeam = aiPlayerTeam.playerTeam;
        }
        else
        {
            return;
        }

        _moveSpeed = Random.Range(_moveSpeedMin, _moveSpeedMax);    // Random  hýz belirleme

        StartWork();               
    }
    void StartWork()
    {
        StartCoroutine(nameof(BallFind));   // Top arama     
        FindingTeamPlayers(); // Takým arkadaþlarýný bul
        StartCoroutine(nameof(ContinuousTargetSearch)); // En yakýn oyuncu arama        
        StartCoroutine(CheckAndShootCoroutine());   // Sut kontol ve cekme
        StartCoroutine(CheckAndPassCoroutine());   // Pass kontol ve pas verme 
    }
    void Update()
    {
        PlayerYAxisLimit();        
    }
    private void FixedUpdate()
    {
        // Eðer karakter yerde deðilse, yer çekimi uygula
        if (!IsGrounded())
        {
            Vector3 gravity = _gravityScale * Physics.gravity; // Yer çekimi kuvveti
            _rb.AddForce(gravity, ForceMode.Acceleration);
        }
        if (!_isWaiting && _ball != null)
        {
            Move();
        }
        else
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }
    void Move()
    {
        //// Eðer top rakipteyse, topa ve oyuncuya doðru hareket et
        //if (PlayerStatusCheck())
        //{
        //    Vector3 directionToBallWithDribble = _ball.position - transform.position;
        //    _rb.velocity = directionToBallWithDribble.normalized * _moveSpeed;
        //    this._animController.MoveAnimPlay();    // Move Anim
        //}
        //else
        //{
        //    // Eðer top bizdeyse, kaleye doðru hareket et
        //    Vector3 directionToBall = _goal.position - transform.position;
        //    float distanceToBall = directionToBall.magnitude;

        //    // _dribbleDistance (deðerini oyucunun kaleci=100, defan=50s, orta saha=10 ve forvet=2 olamsýna göre ayarla
        //    // _dribbleDistance kaleye yaklaþabileceði min uzaklýk) 

        //    // Top bizde ise, topa doðru dribbling mesafesine gelene kadar hareket et
        //    if (distanceToBall > _dribbleDistance)
        //    {
        //        _rb.velocity = directionToBall.normalized * _moveSpeed;
        //        this._animController.MoveAnimPlay();    // Move Anim
        //    }
        //    else
        //    {
        //        _rb.velocity = Vector3.zero; // Dribbling mesafesine gelindiðinde dur
        //        this._animController.IdleAnimPlay();    // Idle Anim
        //    }
        //}

        //------

        if (this._playerTeam == AIPlayerTeam.Team.ATeam)
        {
            _directionToDefenseGoal = _teamAGoal.position - transform.position; // A Team Defense Goal
            _directionToAttackGoal = _teamBGoal.position - transform.position; // A Team Attack Goal
        }
        else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
        {
            _directionToDefenseGoal = _teamBGoal.position - transform.position;    // B Team Defense Goal
            _directionToAttackGoal = _teamAGoal.position - transform.position;     // B Team Attack Goal
        }

        _directionToBall = _ball.position - transform.position;   // Top pozisyonu

        float distanceToAttackGoal = _directionToAttackGoal.magnitude;
        float distanceToDefenceGoal = _directionToDefenseGoal.magnitude;

        switch (_currentState)
        {
            case AIPlayerState.Idle:
                Idle();
                break;
            case AIPlayerState.FreeMove:
                FreeMove();
                break;
            case AIPlayerState.PositionMove:
                PositionMove();
                break;
            case AIPlayerState.CatchTheBall:
                CatchTheBall(_directionToBall);
                break;
            case AIPlayerState.Defense:
                Defense(distanceToDefenceGoal, _directionToDefenseGoal);
                break;
            case AIPlayerState.Attack:
                Attack(distanceToAttackGoal, _directionToAttackGoal);
                break;
            default:
                Debug.LogError("Geçersiz durum!");
                break;
        }
    }
    void Idle()
    {
        //* Bekle
        _rb.velocity = Vector3.zero;
        this._animController.IdleAnimPlay();    // Idle Anim
    }
    void FreeMove()
    {
        //* Random hareket et
        this._animController.MoveAnimPlay();    // Move Anim
    }
    void PositionMove()
    {
        //* AI Player kendi mevkisine döner
        this._animController.MoveAnimPlay();    // Move Anim
    }
    void CatchTheBall(Vector3 directionToBall)
    {
        //* Top boþta ise topa doðru hareket et
        //** Topa en yakýn oyuncu hareket eder   
        if (_ballAttachedToPlayer == null)
        {
            // Top kendisinde deðilse topa git
            _rb.velocity = directionToBall.normalized * _moveSpeed;
            this._animController.MoveAnimPlay();    // Move Anim

            if (this._playerTeam == AIPlayerTeam.Team.ATeam)
            {
                foreach (GameObject player in _teamA)
                {
                    player.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.Idle;
                }
            }
            else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
            {
                foreach (GameObject player in _teamB)
                {
                    player.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.Idle;
                }
            }
        }
        else
        {
            // Topu aldýðýnda ataða çýk
            _currentState = AIPlayerState.Attack;

            if (this._playerTeam == AIPlayerTeam.Team.ATeam)
            {
                foreach (GameObject player in _teamA)
                {
                    player.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.Attack;
                }
            }
            else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
            {
                foreach (GameObject player in _teamB)
                {
                    player.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.Attack;
                }
            }
            _teams.isTheBallFree = false;
            _teams.isTheBallInTeamA = 1;
        }
        
    }
    void Defense(float distanceToDefenceGoal, Vector3 directionToDefence)
    {
        //* Top rakipte ise savunmaya dön _dribbleToDefenceGoalDistance(dribling) mesafesi kadar dön
        //** Topa en yakýn oyuncu rakip oyuncuya gider

        //+  Top bizde ise, top ile _dribbleToDefenceGoalDistance(dribling) mesafesi kadar hareket et
        if (distanceToDefenceGoal > _dribbleToDefenceGoalDistance)
        {
            _rb.velocity = directionToDefence.normalized * _moveSpeed;
            this._animController.MoveAnimPlay();    // Move Anim
        }
        else
        {
            _currentState = AIPlayerState.Idle;
        }
    }
    void Attack(float distanceToAttackGoal, Vector3 directionToAttack)
    {
        //* Top kendi takýmýnda ise rakip kaleye _dribbleToAttackGoalDistance(dribling) mesafesi kadar ilerle 
        //** Top Bu AI'da ise rakip kaleye doðru açýlý ilerle

        //+ Top bizde ise, top ile _dribbleToAttackGoalDistance(dribling) mesafesi kadar hareket et
        if (distanceToAttackGoal > _dribbleToAttackGoalDistance)
        {
            _rb.velocity = directionToAttack.normalized * _moveSpeed;
            this._animController.MoveAnimPlay();    // Move Anim
        }
        else
        {
            _currentState = AIPlayerState.Idle;
        }
    }
    private IEnumerator CheckAndPassCoroutine()
    {
        while (true)
        {
            if (_ball != null && _ballAttachedToPlayer != null && _targetPlayer != null && CanPass())
            {
                // Belli aralýklar ile kontrol et ve þut çekileblir ise þut çek
                Pass();
            }
            yield return new WaitForSeconds(_passCheckInterval);
        }
    }
    bool CanPass()
    {
        Vector3 directionToBall = _ball.position - transform.position;  // Oyuncu ile top arasýndaki yön
        float distanceToBall = directionToBall.magnitude; // Oyuncu ile top arasýndaki mesafe

        Vector3 directionToTargetPlayer = _targetPlayer.position - transform.position;  // Oyucunun pas vereceði oyuncu ile arasýndaki yön
        float passDistanceToTargetPlayer = directionToTargetPlayer.magnitude;   // Oyucunun pas vereceði oyuncu ile arasýndaki mesafe

        // Oyuncunun pas vereceði arkadaþý ile arasýndaki mesafe  Pas vermesi için min mesafeden büyükse pas verebilir
        if (passDistanceToTargetPlayer > _passDistanceToShortMin || passDistanceToTargetPlayer <= _passDistanceToShortMax)
        {
            // Kýsa pas
            _isShortPass = true;            
        }
        else if (passDistanceToTargetPlayer > _passDistanceToLongMin && passDistanceToTargetPlayer <= _passDistanceToLongMax)
        {
            // Uzun pas
            _isLongPass = true;
        }
        else if (passDistanceToTargetPlayer <= _passDistanceToShortMin || passDistanceToTargetPlayer > _passDistanceToLongMax )
        {
            _isLongPass = false;
            _isShortPass = false;
        }

        //if (passDistanceToTargetPlayer > _passDistanceToTargetPlayerMin && passDistanceToTargetPlayer < _passDistanceToTargetPlayerMax)
        //{
        //    // Uzun pas
        //    _isLongPass = true;
        //}
        //else if (passDistanceToTargetPlayer > _minPassDistanceBetweenPlayerToTarget && passDistanceToTargetPlayer < _passDistanceToTargetPlayerMin)
        //{
        //    // Kisa pas
        //    _isShortPass = true;
        //}
        //else
        //{
        //    _isLongPass = false;
        //    _isShortPass = false;
        //}

        return distanceToBall <= _passDistance;
    }
    void Pass()
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
    void PassToTarget(Rigidbody ballRigidbody, float minHeight, float maxHeight, float minPower, float maxPower)
    {
        // Pass Mekaniði
        Vector3 directionToTarget = _targetPlayer.position - transform.position;
        directionToTarget.y = Random.Range(minHeight, maxHeight);
        float passPower = Random.Range(minPower, maxPower);

        this._animController.PassAnimPlay();    // Pass Anim
        ballRigidbody.velocity = directionToTarget.normalized * passPower;  // Pass 

        _isLongPass = false;
        _isShortPass = false;
        _canPass = false;
        _ballAttachedToPlayer = null;
        _ball.GetComponent<AIBall>().BallToPlayerAINull();
        PlayerWaiting();
        StartCoroutine(nameof(SelectTargetPlayerAI));   // Test edilecek
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
    bool CanShoot()
    {
        Vector3 directionToGoal = _directionToAttackGoal - _ball.position;
        float distanceToBall = directionToGoal.magnitude;

        float shootDistance = Random.Range(_shootDistanceMin, _shootDistanceMax);
        return distanceToBall < shootDistance;
    }
    void Shoot()
    {
        // Shoot mekaniði
        Vector3 directionToGoal = _directionToAttackGoal - _ball.position;
        float distanceToBall = directionToGoal.magnitude;
        directionToGoal.y = Random.Range(_shootHeightMin, _shootHeightMax);
        float shootPower = Random.Range(_shootPowerMin, _shootPowerMax);

        this._animController.ShootAnimPlay();   // Shoot Anim
        Debug.Log("SHOOT - UZAKLIK: " + distanceToBall);
        _ball.GetComponent<Rigidbody>().velocity = directionToGoal.normalized * shootPower; // Shoot

        _ballAttachedToPlayer.StickToPlayer = false;
        _ballAttachedToPlayer = null;
        _ball.GetComponent<AIBall>().BallToPlayerAINull();        
        PlayerWaiting();
    }
    bool PlayerStatusCheck()
    {
        // Burada topun nerede olduðunu ve kimin kontrolünde olduðunu belirleyen bir mekanizma olmalýdýr.(A takýmý B takýmý)

        // * isBallWithOpponent eðer true ise=> Topa git * isBallWithOpponent eðer false ise=> Kaleye git

        this.isAttack = false;    // Top kendisinde veya takýmýnda ise false / Rakipte ise true
        if (_ball != null)   // Top hedeflenmiþsse
        {
            if (this._ballAttachedToPlayer != null)
            {
                // Top bu AI'da ise
                this.isAttack = false;
                //Debug.Log(this.gameObject.name+ "_ballAttachedToPlayer != null");

                // Top bu AI'da ise diðer takýmlara göre yazýlacak kod 

                if (this._playerTeam == AIPlayerTeam.Team.ATeam)
                {
                    foreach (GameObject playerTeamA in _teamA)
                    {
                        playerTeamA.GetComponent<AIController>().isAttack = false;
                    }
                    foreach (GameObject playerTeamB in _teamB)
                    {
                        playerTeamB.GetComponent<AIController>().isAttack = true;
                    }
                }
                else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
                {
                    foreach (GameObject playerTeamB in _teamB)
                    {
                        playerTeamB.GetComponent<AIController>().isAttack = false;
                    }
                    foreach (GameObject playerTeamA in _teamA)
                    {
                        playerTeamA.GetComponent<AIController>().isAttack = true;
                    }
                }
            }
            else
            {
                // Top bu AI'da deðilse
                //Debug.Log(this.gameObject.name + "_ballAttachedToPlayer == null");
                //isBallWithOpponent = true;

                // Top bu AI'da deðilse diðer takýmlara göre yazýlacak kod 

                if (this._playerTeam == AIPlayerTeam.Team.ATeam)
                {
                    foreach (GameObject playerTeamA in _teamA)
                    {
                        if (playerTeamA.GetComponent<AIController>().isAttack)
                        {
                            this.isAttack = true;
                        }
                        else
                        {
                            this.isAttack = false;
                        }
                    }
                }
                else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
                {
                    foreach (GameObject playerTeamB in _teamB)
                    {
                        if (playerTeamB.GetComponent<AIController>().BallAttachedToPlayer == null)
                        {
                            this.isAttack = true;
                        }
                        else
                        {
                            this.isAttack = false;
                        }
                        //if (playerTeamB.GetComponent<AIController>().isBallWithOpponent)
                        //{
                        //    this.isBallWithOpponent = true;
                        //}
                        //else
                        //{
                        //    this.isBallWithOpponent = false;
                        //}
                    }
                }
            }
        }
        return isAttack;
    }
    private bool IsGrounded()
    {
        // Karakterin yerde olup olmadýðýný kontrol et
        RaycastHit hit;
        float raycastDistance = 0.1f; // Yer kontrolü için raycast mesafesi
        return Physics.Raycast(_groundCheckTransform.position, Vector3.down, out hit, raycastDistance, _groundLayer);
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
        // Pas veya þut attýktan sonra oyuncu beklemesi
        _isWaiting = true;
        _ball = null;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _currentState = AIPlayerState.Idle;
        _teams.isTheBallFree = true;
        _teams.isTheBallInTeamA = -1;
        StartCoroutine(nameof(BallFind));
    }
    public void PlayerToBallWaiting()
    {
        // Pas aldýktan sonra veya top alýndýðýnda oyuncu beklemesi
        _isWaiting = true;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _currentState = AIPlayerState.Idle;
        _teams.isTheBallFree = false;
        _teams.isTheBallInTeamA = 1;
        StartCoroutine(nameof(PlayerÝsWaiting));
    }
    IEnumerator PlayerÝsWaiting()
    {
        // Hareket etmek için bekle
        yield return new WaitForSeconds(_waitingDuration);
        _isWaiting = false;

        _currentState = AIPlayerState.Attack;

        if (this._playerTeam == AIPlayerTeam.Team.ATeam)
        {
            foreach (GameObject player in _teamA)
            {
                player.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.Attack;
            }
        }
        else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
        {
            foreach (GameObject player in _teamB)
            {
                player.GetComponent<AIController>().CurrentState = AIController.AIPlayerState.Attack;
            }
        }

    }
    IEnumerator BallFind()
    {
        // Topu bul
        yield return new WaitForSeconds(_waitingDuration);
        _ball = GameObject.FindWithTag("AIBall").gameObject.transform;
        _isWaiting = false;
    }
    IEnumerator ContinuousTargetSearch()
    {
        while (true) // Sonsuz bir döngü oluþturur, yani oyun bitene kadar çalýþýr.
        {
            SelectTargetPlayerAI();

            // 1 saniye bekleme süresi ekleyebilirsiniz (opsiyonel)
            yield return new WaitForSeconds(1f);
        }
    }
    void SelectTargetPlayerAI()
    {
        // Pas verilecek en yakýn oyuncuyu bul
        _targetPlayer = null;

        if (this._playerTeam == AIPlayerTeam.Team.ATeam)
        {
            // Sahadaki tüm oyuncularý bul
            List <GameObject> players = _teamA;
            // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendisine pas yapmak istemeyiz)
            players = _teamA.Where(player => player != gameObject).ToList();

            // Kendisine olan uzaklýðýna göre oyuncularý sýrala
            players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToList();

            // Ýlk oyuncuyu hedef olarak seç (yani en yakýn oyuncu)
            if (players.Count > 0)
            {
                _targetPlayer = players[0].transform;
            }
        }
        else if (this._playerTeam == AIPlayerTeam.Team.BTeam)
        {
            // Sahadaki tüm oyuncularý bul
            List<GameObject> players = _teamB;
            // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendisine pas yapmak istemeyiz)
            players = players.Where(player => player != gameObject).ToList();

            // Kendisine olan uzaklýðýna göre oyuncularý sýrala
            players = players.OrderBy(player => Vector3.Distance(transform.position, player.transform.position)).ToList();

            // Ýlk oyuncuyu hedef olarak seç (yani en yakýn oyuncu)
            if (players.Count > 0)
            {
                _targetPlayer = players[0].transform;
            }
        }
    }
    void FindingTeamPlayers()
    {
        if(this._playerTeam == AIPlayerTeam.Team.ATeam)
        {
            // Sahadaki tüm oyuncularý bul
            _teamA = _teams.teamA;
            // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendisine pas yapmak istemeyiz)
            _teamA = _teamA.Where(player => player != gameObject).ToList();
        }
        else if(this._playerTeam == AIPlayerTeam.Team.BTeam)
        {
            // Sahadaki tüm oyuncularý bul
            _teamB = _teams.teamB;
            // Eðer kendisi oyuncular listesinde varsa, kendisini listeden çýkar (kendisine pas yapmak istemeyiz)
            _teamB = _teamB.Where(player => player != gameObject).ToList();
        }
    }

    public enum AIPlayerState
    {
        Idle,
        FreeMove,
        PositionMove,
        CatchTheBall,
        Defense,
        Attack,
    }
}
