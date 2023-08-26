using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBall : MonoBehaviour
{
    [SerializeField] private Transform _transformPlayerAI;
    [SerializeField] private Transform _playerAIBallPositon;
    private float _ballSpeed;
    private Vector3 _previousPosition;
    public float distanceToPlayerAI;
    [SerializeField] private float _fieldWith = 10f;
    [SerializeField] private float _fieldLength= 10f;
    [SerializeField] private float _minYPosition = 0.61f;  // Alt sýnýr pozisyonu

    [SerializeField] private bool _stickToPlayer;    // oyuncuda ise
    [SerializeField] private float _distanceToPlayerAIStickMin = 0.45f;
    AIController _playerAI;

    [SerializeField] private AudioSource _soundPole;
    public bool StickToPlayer { get => _stickToPlayer; set => _stickToPlayer = value; }
    public Transform TransformPlayerAI { get => _transformPlayerAI; set => _transformPlayerAI = value; }

    void Start()
    {
        
    }
    private void Update()
    {
        BallYAxisLimit();
    }

    private void BallYAxisLimit()
    {
        // Topun Y eksenindeki pozisyonunu sýnýrla
        Vector3 newPosition = transform.position;
        newPosition.y = Mathf.Max(_minYPosition, newPosition.y);
        transform.position = newPosition;
    }

    void FixedUpdate()
    {
        if (_transformPlayerAI != null)
        {
            if (!_stickToPlayer)
            {
                distanceToPlayerAI = Vector3.Distance(_transformPlayerAI.position, transform.position);

                if (distanceToPlayerAI < _distanceToPlayerAIStickMin)
                {
                    _stickToPlayer = true;
                    _playerAI.BallAttachedToPlayer = this;
                }
            }
            else
            {
                Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
                _ballSpeed = Vector2.Distance(currentPosition, _previousPosition) / Time.deltaTime;

                transform.position = _playerAIBallPositon.position;
                transform.Rotate(new Vector3(_transformPlayerAI.right.x, 0, _transformPlayerAI.right.z), _ballSpeed, Space.World);
                _previousPosition = currentPosition;
            }
        }        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FieldBallLimit"))
        {
            BallResetPosition();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pole"))
        {
            _soundPole.Play();
        }
        if (collision.gameObject.CompareTag("TeamA") || collision.gameObject.CompareTag("TeamB") && this.gameObject.CompareTag("AIBall"))
        {            
            if (_playerAIBallPositon == null)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                _transformPlayerAI = collision.gameObject.transform;
                _playerAIBallPositon = _transformPlayerAI.Find("BallPosition");
                _playerAI = _transformPlayerAI.GetComponent<AIController>();
                _playerAI.PlayerToBallWaiting();
            }
            else
            {
                _transformPlayerAI = collision.gameObject.transform;
                _playerAIBallPositon = _transformPlayerAI.Find("BallPosition");
                _playerAI = _transformPlayerAI.GetComponent<AIController>();
            }
        }
    }
    public void BallResetPosition()
    {
        //transform.position = new Vector3(0f, 2f, 0f);

        // Rastgele bir yeni pozisyon oluþtur
        Vector3 randomPosition = new Vector3(Random.Range(-_fieldWith, _fieldWith), 2f, Random.Range(-_fieldLength, _fieldLength));
        transform.position = randomPosition;  // Yeni pozisyona taþý

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;        
    }
    public void BallToPlayerAINull()
    {
        _transformPlayerAI = null;
        _playerAIBallPositon = null;
        BallTagChange(false);
    }
    private void BallTagChange(bool value)
    {
        if (value)
        {
            this.gameObject.tag = "AIBall";
        }
        else
        {
            this.gameObject.tag = "Untagged";
            StartCoroutine(nameof(BallTagChangeCoroutine));
        }
    }
    IEnumerator BallTagChangeCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        BallTagChange(true);
    }
}
