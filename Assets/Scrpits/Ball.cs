using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private Transform _transformPlayer;
    [SerializeField] private Transform _playerBallPositon;
    private float _ballSpeed;
    private Vector3 _previousPosition;

    private bool _stickToPlayer;    // oyuncuda ise
    [SerializeField] private float _distanceToPlayerStickMin = 0.5f;
    Player _scriptPlayer;

    [SerializeField] private AudioSource _soundPole;
    public bool StickToPlayer { get => _stickToPlayer; set => _stickToPlayer = value; }

    void Start()
    {
        _playerBallPositon = _transformPlayer.Find("BallPosition");
        _scriptPlayer = _transformPlayer.GetComponent<Player>();
    }
    void Update()
    {
        if (!_stickToPlayer)
        {
            float distanceToPlayer = Vector3.Distance(_transformPlayer.position, transform.position);
            if (distanceToPlayer < _distanceToPlayerStickMin)
            {
                _stickToPlayer = true;
                _scriptPlayer.BallAttachedToPlayer = this;
            }
        }
        else
        {
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);
            _ballSpeed = Vector2.Distance(currentPosition, _previousPosition) / Time.deltaTime;

            transform.position = _playerBallPositon.position;
            transform.Rotate(new Vector3(_transformPlayer.right.x, 0, _transformPlayer.right.z), _ballSpeed, Space.World);
            _previousPosition = currentPosition;
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
    }
    public void BallResetPosition()
    {
        transform.position = new Vector3(0f, 2f, 0f);
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
