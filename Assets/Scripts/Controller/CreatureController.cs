using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumList;

// for test
using UnityEngine.Tilemaps;
using System.IO;
using System.Linq;
using System;

public class CreatureController : MonoBehaviour {

    protected Camera _cam;
    protected bool IsCamShake { get; set; }
    protected float _speed = 10.0f;
    protected float _jumpDist = 5.0f;

    protected int _hp;
    public virtual int HP {
        get { return _hp; }
        set {
            // hp가 damage보다 클 경우 Damage State로 전환 및 hp 감소
            if (value <= 0) {
                _hp = 0;
                State = CreatureState.Dead;
            }
            else if(value > 0) {
                State = CreatureState.Damaged;
                _hp = value;
            }

        }
    }

    protected Gun gunInfo = new Gun();

    protected Coroutine _coPullTrigger;
    Vector3 jumpDest;
    protected bool _isJump = false;
    protected Animator _animator;
    protected Rigidbody2D _rigidbody;
    protected Vector3Int _creatureGridPos = Vector3Int.zero;
    protected GunController _equipedGun;
    protected WaitForSeconds _CoMakeBulletWaitSec;
    protected SpriteRenderer _sprite;
    protected Transform _target;
    public Transform ShootTarget { get { return _target; } }
    protected bool _triggerOn = true;
    public bool TriggerState { get { return _triggerOn; } }
    protected bool _isBulletProof = false;
    protected bool _makeJump = false;
    protected Vector3 _fixedPosition = new Vector3(0.5f, 0.5f);
    protected Vector3 _destPos { get; set; }
    protected CreatureDir _lastDir = CreatureDir.Down;
    CreatureState _state = CreatureState.Idle;

  
    void Start() {
        Init();
    }
    

    protected virtual void Init() {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Fixed Time Stamp == 0.01666667 (1/60 근사치)
    void FixedUpdate() {
        UpdateController();
    }

    protected virtual CreatureState State {
        get { return _state; }
        set {
            if (_state.Equals(value)) {
                return;
            }

            _state = value;
        }
    }

    protected virtual CreatureDir MoveDir {
        get { return _lastDir; }
        set {
            if (value.Equals(CreatureDir.None)) {
                _lastDir = value;
                _destPos = Vector3.zero;
                State = CreatureState.Idle;
                return;
            }

            State = CreatureState.Move;

            switch (value) {
                case CreatureDir.Up:
                    _lastDir = value;
                    _destPos = Vector3.up;
                    break;
                case CreatureDir.Down:
                    _lastDir = value;
                    _destPos = Vector3.down;
                    break;
                case CreatureDir.Left:
                    _lastDir = value;
                    _destPos = Vector3.left;
                    //_sprite.flipX = true;
                    break;
                case CreatureDir.Right:
                    _lastDir = value;
                    _destPos = Vector3.right;
                    break;
                case CreatureDir.UpLeft:
                    _lastDir = value;
                    _destPos = (Vector3.up + Vector3.left).normalized;
                    break;
                case CreatureDir.UpRight:
                    _lastDir = value;
                    _destPos = (Vector3.up + Vector3.right).normalized;
                    break;
                case CreatureDir.DownLeft:
                    _lastDir = value;
                    _destPos = (Vector3.down + Vector3.left).normalized;
                    break;
                case CreatureDir.DownRight:
                    _lastDir = value;
                    _destPos = (Vector3.down + Vector3.right).normalized;
                    break;
            }
        }
    }

    // Problem : Attack 상태 내의 UpdateMoving 때문에 공격시 이전 _destpos으로 이동함
    // 키보드 입력 떨어질 경우 _destpos가 zero가 되도록 하기
    protected virtual void UpdateController() {
        switch (State) {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Move:    // move와 attack을 다른 state 항시 대기 상태로 만들기
                UpdateMoving();
                break;
            case CreatureState.Attack:  // how to move while attck
                UpdateAttack();
                UpdateMoving();
                break;
            case CreatureState.Jump:
                UpdateJump();
                break;
            case CreatureState.Damaged:
                UpdateDamaged();
                break;
            case CreatureState.Dead:
                UpdateDead();
                break;
        }
    }

    protected virtual void UpdateIdle() {
        _animator.Play("Idle");
        _sprite.flipX = true;
        _rigidbody.velocity = Vector2.zero;
    }

    protected virtual void UpdateMoving() {
        if (!MoveDir.Equals(CreatureDir.None)) {
            _animator.Play("Walk_Down");
            _sprite.flipX = true;
        }
        
        //MovePossibleDetect();


        // [Fixed] Problem : transform.position으로 하면 Time.deltaTime으로 속도 보정이 되는데 MovePosition을 할 때는 왜 속도 보정 안 되지?
        // MovePosition을 사용하는 경우 frame이 높으면 느리고 frame이 낮으면 느림
        // Sol : MovePosition Documentation을 보면 해당 함수는 FixedUpdate()에서 작동 중
        // >> This sholuld be used if you want to continuously move a rigidbody in each FixedUpdate.
        // >> 구글링 결과 MovePosition이 꼭 FixedUpdate()에서 사용해야하는 것은 아니지만
        // >> Time.fixedDeltaTime을 사용하는 경우 속도에 있어 프레임 보정을 받음
        // >> Documentation에서는 deltaTime을 사용하는데 왜 적용이 안 되었을까?
        // >> years ago, someone at Unity decided that enough users got confused between the meaning of "fixedDeltaTime" and "deltaTime"
        // >> that they'd change it so that if you use "deltaTime" during the fixed-update, we'll just return "fixedDeltaTime" implicitly
        // >> so only then, they mean the same things.
        // >> https://forum.unity.com/threads/why-is-time-deltatime-needed-when-moving-a-gameobject-using-rigidbody-moveposition.1177852/
        // >> 위 링크에 따르면 Documentation에서는 FixedUpdate()에서 MovePosition이 사용되었으므로 예시의 deltaTime은 결국 fixedDeltaTime을 일컫는 말!
        _rigidbody.MovePosition(transform.position + _destPos * _speed * Time.fixedDeltaTime);
        
        //_rigidbody.position = transform.position + _destPos * _speed * Time.deltaTime;
        //transform.position += _destPos * _speed * Time.deltaTime;

        // Player 및 Enemy를 위해 Creature 단계에서 Space를 통해 State 변환되는 것은 추후 수정 필요
        if (_makeJump) {
            jumpDest = transform.position + _destPos * _jumpDist;
            State = CreatureState.Jump;
        }
        
    }

    protected virtual void UpdateAttack() {
        // _coPullTrigger == null ㄷㅐ신 triggerOn을 조건. triggerOn은 GunController에서 사용
        if (_triggerOn) {
            _coPullTrigger = StartCoroutine("CoPullTrigger");
        }



    }

    // [Fixed]Problem : Collsion이 발생해서 _jumpDest로 갈 수 없는 경우 제자리 무한 구르기 발생
    // Sol :  Jump 애니메이션이 1회만 재생되도록 설정

    // !!!!!!!
    // Problem : Collsion 앞에서 Jump 시도시 가끔 뚫고 지나감

    // Problme : 이동 / 사격시 Jump가 씹히는 경우가 간혹 발생
    //01 : Jump모션이 일부만 실행되고 멈춤(마치 점프 하려다 마는 것처럼)
    //02 : 점프 시도를 안 함
    //+ 스페이스를 오래 누르고 있으면 하긴 함.State 변화와 JUmp 사이의
    //연관관계 확인할 필요 있음
    protected virtual void UpdateJump() {
        

        _isJump = true;
        _isBulletProof = true;

        //MovePossibleDetect();

        //// Player 손 제거
        Transform child = transform.GetChild(0);
        child.gameObject.SetActive(false);

        _animator.Play("Jump");

        _sprite.flipX = true;
        if(MoveDir.Equals(CreatureDir.Right) || MoveDir.Equals(CreatureDir.UpRight) || MoveDir.Equals(CreatureDir.DownRight)) {
            _sprite.flipX = false;
        }
        
        // 속도 변수 수정에 따라 자동으로 바뀌게 변수들로 이루어진 식으로 바꾸기
        float _jumpSpeed = _speed * 2.0f;
        float dist = (jumpDest - transform.position).magnitude;

        RaycastHit2D rayHit;
        rayHit = Physics2D.Raycast(this.transform.position, _destPos, 1.5f, LayerMask.GetMask("Collision"));
        if (rayHit.transform != null) {
            _destPos = Vector3.zero;
        }

        if (dist < _jumpSpeed * Time.fixedDeltaTime && rayHit.transform == null) {
            transform.position = jumpDest;
            State = CreatureState.Idle;
            child.gameObject.SetActive(true);
            _isBulletProof = false;
            _isJump = false;
        }
        // else if로 일정 시간이 지나면 혹은 collsion이 발생하면 이동 안 하도록 만들기
        else if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
            State = CreatureState.Idle;
            child.gameObject.SetActive(true);
            _isBulletProof = false;
            _isJump = false;
        }
        // problem 01 : 특정 지역 e.g 예를 들면 덤불과 돌탑 사이 - 에서 collider 경계 뚫고 지나감
        // collisionEnter로 제한 걸어줘야 할까?
        else {  
            _rigidbody.MovePosition(transform.position + _destPos * _jumpSpeed * Time.fixedDeltaTime);
        }
        
    }

    protected virtual void UpdateDamaged() {
        //Debug.Log($"{gameObject.name}'s HP : {hp}");
        State = CreatureState.Idle;
    }


    protected virtual void UpdateDead() { }

    protected virtual void GunInit() {
        _equipedGun = GetComponentInChildren<GunController>();
        if (_equipedGun == null) {
            Debug.Log($"{name}'s Gun is Empty");
        }
        else {
            gunInfo = _equipedGun.getGunInfo;
            _CoMakeBulletWaitSec = new WaitForSeconds(gunInfo.shootCoolTime);

        }

    }

    protected virtual IEnumerator CoPullTrigger() {
        _triggerOn = false;
        #region Fixed Problem
        // [Fixed]Problem 01 : 총알 이동 안 함
        // [Sol] bullet Prefab에 Script 사라져 있었음
        // [Fixed] Problem 01-01 : bullet Prefab에 Script 추가 후 Bullet이 나타났다 사라[
        // [Fixed]Problem 02 : 총알 Pop안 하고 push만 함
        // [Sol] PoolManager에서 poolObj가 아닌 경우에 대한 코드가 테스트용으로 계쏙 실행되고 있었음
        // [Fixed] Problem 02-01 : 총알이 Pop안 하고 Push해서 사용함
        // [Sol] Pool count 문제

        // [Fixed] Problem 03 - related with problem 06 : 총알 사라지면 setAactive(false)하고 pool 변화되게 해야
        // >> bulletController.cs에서 시야 넘어가면 Destroy() 하는 것 때문으로 보임 수정 필요

        // [Fixed]Problme 04 : PoolStack이 empty일 경우 새롭게 오브젝트 생성해서 push 안 함
        // [Sol] : PoolManager에서 오브젝트 pool이 비어져 있는 경우 판별에 대한 방식 오류였음
        // [Fixed]Problem 05 : 총알 오브젝트의 생성 위치가 Pool 오브젝트 위치임 유저 위치에 따라 가도록 하기
        // [Sol] : CreaterController에서 pop된 오브젝트의 초기 위치를 세팅해줌 

        // [Fixed]problem 06 : 같은 코드인데 어떤 때는 pop해서 안 쓰고 어떤 때는 pop해서 쓰는데
        // pop할 때 PoolObj 리스트에서 위로 순서대로 pop, hieachy상에서만 오브젝트가 있지 push/pop 안 되고 있는 것 같음 
        // 마지막에 destroy하는 것 같?
        // [Sol] 01. BulletController에서 카메라 범위 벗어날 경우 / 충돌할 경우 두 곳에서 Destroy()였음
        // [Fixed]problem 08 : 사용한 총알을 poolStack에 PushPoolChild로 반환하는 부분 수정하면서
        // 현재 poolStack 초기화 하는 부분 오류 발생
        // [Fixed]Pool Class의 Push 확인 필요

        // [Fixed] problem 09 : 총알 발사시 위치 마우스 위치 안 따라감
        // [sol] push > pop > push시 dir의 초기화가 이루어지지 않음 > OnEnable을 통해 setActive때마다 초기화 되도록 바꿈
        #endregion

        // Creature가 소지 중인 Gun에서 총알 발사
        // Gun이 Reload 중일 경우 pullTrigger하여도 총알 발사 불가
        if (_equipedGun.Reload == false) {
            _equipedGun.TriggerState = true;

            if(_equipedGun.ShooterType == CreatureType.Player) {
                IsCamShake = true;
            }
        }

        yield return _CoMakeBulletWaitSec;

        _coPullTrigger = null;
        _triggerOn = true;
    }
}
