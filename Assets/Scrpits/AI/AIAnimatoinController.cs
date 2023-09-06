using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimatoinController : MonoBehaviour
{
    [Header("AI Animator Controller")]
    private Animator _animator;
    public const int ANIMATION_LAYER_MOVE = 0;
    public const int ANIMATION_LAYER_SHOOT = 1;
    public const int ANIMATION_LAYER_PASS = 2;
    [SerializeField] private float _animDelay = 0.25f;
    private void Awake()
    {
        this._animator = this.GetComponent<Animator>();
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    public void IdleAnimPlay()
    {        
        this._animator.SetBool("isMove", false);
        this._animator.SetBool("isIdle", true);
    }
    public void MoveAnimPlay()
    {
        this._animator.SetBool("isMove", true);
        this._animator.SetBool("isIdle", false);
    }
    public void PassAnimPlay()
    {
        this._animator.Play("Pass", ANIMATION_LAYER_PASS, 0);
        this._animator.SetLayerWeight(ANIMATION_LAYER_PASS, 1f);
        StartCoroutine(PassAnimStopCorutine());
    }
    IEnumerator PassAnimStopCorutine()
    {
        yield return new WaitForSeconds(_animDelay);
        this._animator.SetLayerWeight(ANIMATION_LAYER_PASS, Mathf.Lerp(_animator.GetLayerWeight(ANIMATION_LAYER_PASS), 0f, Time.deltaTime * 50f));
        yield return new WaitForSeconds(_animDelay);
        this._animator.Play("Idle", ANIMATION_LAYER_MOVE, 0);
        this._animator.SetLayerWeight(ANIMATION_LAYER_MOVE, 1f);
    }
    public void ShootAnimPlay()
    {
        this._animator.Play("Shoot", ANIMATION_LAYER_SHOOT, 0.35f);
        this._animator.SetLayerWeight(ANIMATION_LAYER_SHOOT, 1f);
        StartCoroutine(ShootAnimStopCorutine());
    }
    IEnumerator ShootAnimStopCorutine()
    {
        yield return new WaitForSeconds(_animDelay);
        this._animator.SetLayerWeight(ANIMATION_LAYER_SHOOT, Mathf.Lerp(_animator.GetLayerWeight(ANIMATION_LAYER_SHOOT), 0f, Time.deltaTime * 50f));
        yield return new WaitForSeconds(_animDelay);
        this._animator.Play("Idle", ANIMATION_LAYER_MOVE, 0);
        this._animator.SetLayerWeight(ANIMATION_LAYER_MOVE, 1f);
    }
    public void ShootAnimStop()
    {
       this._animator.SetLayerWeight(ANIMATION_LAYER_SHOOT, Mathf.Lerp(_animator.GetLayerWeight(ANIMATION_LAYER_SHOOT), 0f, Time.deltaTime * 50f));
    }
}
