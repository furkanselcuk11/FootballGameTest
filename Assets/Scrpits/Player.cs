using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    private Animator _animator;
    private Ball _ballAttachedToPlayer;
    private float _timeShoot = -1f;
    public const int ANIMATION_LAYER_SHOOT = 1;
    [Space][Header("Football Settings")]
    private bool _shoot;
    private bool _pass;
    [SerializeField] private float _shootPowerMin = 10f;
    [SerializeField] private float _shootPowerMax = 30f;
    [SerializeField] private float _shootHeightMin = 0.25f;
    [SerializeField] private float _shootHeightMax = 0.75f;
    [Space]
    [Header("Score")]
    private int _teamAScore, _teamBScore;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _goalText;
    private float _goalTextColorAlpha;
    [Space]
    [Header("Sonds")]
    [SerializeField] private AudioSource _soundDribble;
    [SerializeField] private AudioSource _soundCheer;
    [SerializeField] private AudioSource _soundKick;
    [SerializeField] private float _dribbleTime = 3f;
    private float _distanceSinceLastDribble;
    public Ball BallAttachedToPlayer { get => _ballAttachedToPlayer; set => _ballAttachedToPlayer = value; }

    void Start()
    {
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (!_shoot)
        {
            // SHOOT
            _shoot=Input.GetKeyDown(KeyCode.D);
            if (_shoot)
            {
                Debug.Log("Shoot!");
                _shoot = false;
                _timeShoot = Time.time;
                _animator.Play("Shoot", ANIMATION_LAYER_SHOOT, 0f);
                _animator.SetLayerWeight(ANIMATION_LAYER_SHOOT, 1f);
            }
            if (_timeShoot > 0)
            {
                // Shoot Ball
                if(_ballAttachedToPlayer !=null && Time.time - _timeShoot > 0.2f)
                {
                    _soundKick.Play();
                    _ballAttachedToPlayer.StickToPlayer = false;

                    Rigidbody rb = _ballAttachedToPlayer.transform.gameObject.GetComponent<Rigidbody>();
                    Vector3 shootDirection = transform.forward;
                    shootDirection.y += Random.Range(_shootHeightMin, _shootHeightMax);
                    float shootPower = Random.Range(_shootPowerMin, _shootPowerMax);
                    rb.AddForce(shootDirection * shootPower, ForceMode.Impulse);

                    _ballAttachedToPlayer = null;
                }
                // Finish Shoot Animation
                if (Time.time - _timeShoot > 0.5f)
                {
                    _timeShoot = -1f;
                }
            }
            else
            {
                _animator.SetLayerWeight(ANIMATION_LAYER_SHOOT, Mathf.Lerp(_animator.GetLayerWeight(ANIMATION_LAYER_SHOOT), 0f, Time.deltaTime * 10f));
            }
        }
        if (!_pass)
        {
            // PASS
            _pass=Input.GetKeyDown(KeyCode.S);
            if (_pass)
            {
                Debug.Log("Pass!");
                _pass = false;
            }            
        }
        if (_goalTextColorAlpha > 0)
        {
            _goalTextColorAlpha -= Time.deltaTime;
            _goalText.alpha = _goalTextColorAlpha;
        }
        float playerSpeed = transform.GetComponent<Rigidbody>().velocity.magnitude;
        if (_ballAttachedToPlayer != null)
        {
            _distanceSinceLastDribble += playerSpeed * Time.deltaTime;
            if (_distanceSinceLastDribble > _dribbleTime)
            {
                _soundDribble.Play();
                _distanceSinceLastDribble = 0;
            }
        }
    }
    public void IncreaseTeamAScore()
    {
        _teamAScore++;
        UpdateScoreUI();
    }
    public void IncreaseTeamBScore()
    {
        _teamBScore++;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        _soundCheer.Play();
        _scoreText.text = "Team A [" + _teamAScore.ToString() + " - " + _teamBScore.ToString() + "] Team B";
        _goalTextColorAlpha = 1f;
    }
}
