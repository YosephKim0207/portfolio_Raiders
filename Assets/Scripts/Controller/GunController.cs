using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumList;

public class GunController : MonoBehaviour {
    //public bool equip;

    Vector3 _gunLook;
    Vector3 _aimTargetPos;
    Vector3 _shootPointCorrectionPistol = new Vector3(0.0f, 0.2f, 0.0f);
    Vector3 _shootPointCorrectionRifle = new Vector3(0.0f, 0.0f, 0.0f);
    Vector3 _gunGripCorrectioin = new Vector3(0.0f, 0.5f, 0.0f);
    const float rad2Deg = 57.29298f;
    const float rad90Deg = 1.578f;
    GameObject shootPointGo;
    GameObject _bullet;
    Gun gunInfo = new Gun();
    int _ammo;
    int gunName;
    float _angle;
    GameObject _bulletType;
    public Transform ShootPoint { get; private set; }
    Transform _shooter;
    Transform _hand;
    SpriteRenderer _spriteRenderer;
    WaitForSeconds _coShootWaitSecReload, _coShootWaitSecShootCoolTime, _coGunFireWaitSec;

    PlayerController playerController;
    public CreatureType ShooterType { get; private set; }

    public Gun getGunInfo {
        get {
            if (gunInfo.bulletType != null) {
                return gunInfo;
            }
            else {
                //Debug.Log($"{this.name}'s GunController {this.GetComponent<GunController>().enabled} in getGunInfo struct");
                
                return GetGunInfo();
            }
        }
    }

    public int RemainBullet {
        get { return _ammo; }
    }


    public bool IsFever {
        set {
            if (value) {
                _coShootWaitSecReload = new WaitForSeconds(0.0f);
                _coShootWaitSecShootCoolTime = new WaitForSeconds(0.05f);
            }
            else {
                _coShootWaitSecReload = new WaitForSeconds(gunInfo.reloadTime);
                _coShootWaitSecShootCoolTime = new WaitForSeconds(gunInfo.shootCoolTime);
            }
        } }

    bool _triggerState = false;
    public bool TriggerState { set { _triggerState = value; } }
    bool _reload = false;
    public bool Reload { get { return _reload; } }
    Coroutine _coShoot;
    Coroutine _coGunFire;


    // TODO 
    static public System.Action<TargetInfo> SetTargetInfoAction = null;

    public struct TargetInfo {
        Vector3 bulletShootPoint;
        int bulletDamage;
        Transform gunShooter;
    }


    private void OnEnable() {
        //equip = !equip;

        // 총기 소유 피아식별 
        _shooter = transform.parent.parent;
        _hand = transform.parent;

        // _shooter가 Player인 경우 
        if ((playerController = _shooter.GetComponent<PlayerController>()) != null) {

            ShooterType = CreatureType.Player;
            _aimTargetPos = Manager.Mouse.CheckMousePos();
            //_aimTarget = Manager.Mouse.CheckMousePos();


            // 마우스 포지션이 3d고 오브젝트들은 2d 아닌가? 문제 생길 여지 없는지 확인해보기
            // 일단 현재 _aimTarget은 Transform 형태임, 이를 Vector3로 수정하던지 Transform 형태로 이용할 필요
            //_targetPos는 Vector3 형태이므로 이를 활용해보자
            // TODO
            // _aimTargetPos를 해줄 이유가 있나? 
            //_aimTargetPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
        }
        // _shooter가 Enemy인 경우
        else {
            ShooterType = CreatureType.Enemy;
            // TODO
            // 추후 가까운 적 혹은 가장 딜을 많이 넣는 적으로 변경해줄 것
            playerController = GameObject.Find("Player").GetComponent<PlayerController>();
            _aimTargetPos = playerController.transform.position;
            
        }

        GetGunInfo();
        gunName = int.Parse(gunInfo.name);
    }

    private void Start() {
        Init();
        //_spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    }

    private void OnDisable() {
        //equip = !equip;
        if(ShooterType == CreatureType.Player) {
            // GameObject bullet = GameObject.Find(_bulletType.name);
            Transform bulletPool = _bullet.transform.parent;
            if (bulletPool != null) {
                Manager.Pool.DeletePool(_bulletType);
            }
        }
    }

    private void Update() {
        // Equip 필요 없지 않나 ?
        //if (equip) {
            RotateGun();

            // 현재 문제
            // 재장전 동안 트리거 당기고 있을 경우 _triggerState == true
            // 재장전 되는 즉시 아래 조건문을 통과하여 발사 되고 _triggerState는 false로 변경
            // 그 즉시 CreatureController를 통해 다시 _triggerState가 true로 바뀜
            // Shooting() 함수 call이 또 걸림
            // 코루틴이 다중실행되는 문제로 stack 등의 방법을 통한 해결 필요
            if (_triggerState && (_ammo > 0)) {
                _triggerState = false;
                _coShoot = StartCoroutine("CoShoot");
            }
        //}
    }

    protected void Init() {
        GetGunInfo();
    }

    // TODO
    // Player의 경우 GetGunInfo가 불필요하게 두 번씩 발생한다 Creature단에서만 실행되도록 하던지,
    // Playeer 단에서 중복 실행되지 않도록 조치 필요
    // Player의 InitGunPose()와 Creature의 GunInit()에서 각각 gunInfo를 가져온다
    protected Gun GetGunInfo() {
        // JSON으로부터 해당 Obj의 총기 정보 가져오기
        Dictionary<string, Gun> gunDict = Manager.Data.gunDict;
        int idxValue = gameObject.name.IndexOf("_");
        string name = gameObject.name.Substring(idxValue + 1);

        gunDict.TryGetValue(name, out gunInfo);

        _bulletType = (GameObject)Resources.Load($"Prefabs/Bullets/Bullet_{gunInfo.bulletType}");
        
        // 총구 위치 정보 가져오기
        shootPointGo = transform.Find("Point").gameObject;
        ShootPoint = shootPointGo.transform;
        _ammo = gunInfo.ammo;


        

        _coShootWaitSecReload = new WaitForSeconds(gunInfo.reloadTime);
        _coShootWaitSecShootCoolTime = new WaitForSeconds(gunInfo.shootCoolTime);
        //_coGunFireWaitSec = new WaitForSeconds(0.1f);

        return gunInfo;
    }

    protected void RotateGun() {
        if(ShooterType == CreatureType.Player) {
            _aimTargetPos = Manager.Mouse.CheckMousePos();

        }
        else {
            _aimTargetPos = playerController.transform.position;
        }

        _gunLook = (_aimTargetPos - transform.position).normalized;
        //if (gunName > 100) {
        //    _gunLook = ((_aimTargetPos + ) - transform.position).normalized;
        //}
        //else {
        //    _gunLook = (_aimTargetPos - transform.position).normalized;
        //}
        
        if (_gunLook.x < 0 && gunName < 100) {
            _spriteRenderer.flipY = true;
        }
        else if(_gunLook.x >= 0 && gunName < 100) {
            _spriteRenderer.flipY = false;
        }
        else if (_gunLook.x < 0 && gunName >= 100) {
            _spriteRenderer.flipX = true;
        }
        else if (_gunLook.x >= 0 && gunName >= 100) {
            _spriteRenderer.flipX = false;
        }




        if (gunName > 100) {
            _angle = (Mathf.Atan2(_gunLook.y, _gunLook.x) + rad90Deg) * rad2Deg;  // target에 대한 xy방향벡터를 통해 tan 각도 구하기
            transform.rotation = Quaternion.AngleAxis(_angle, Vector3.forward);  //Z축 중심으로 angle만큼 회전
        }
        else {
            _angle = Mathf.Atan2(_gunLook.y, _gunLook.x) * rad2Deg;  // target에 대한 xy방향벡터를 통해 tan 각도 구하기
            transform.rotation = Quaternion.AngleAxis(_angle, Vector3.forward);  //Z축 중심으로 angle만큼 회전
        }

        ShootPoint = shootPointGo.transform;

    }

    float Atan2Degree(Vector3 dest) {
        return Mathf.Atan2(dest.y, dest.x) * 57.2978f; // 57.29578 == Mathf.Rad2Deg
    }

    

    IEnumerator CoShoot() {


        Vector3 shootDir = (_aimTargetPos - ShootPoint.position).normalized;

        float rot = Atan2Degree(shootDir);  // 새로 만들 UsePool에 사용할 rot
        //GameObject _bullet;
        if (gunName < 100 && _spriteRenderer.flipY) {
            shootDir = (_aimTargetPos - _shootPointCorrectionPistol - ShootPoint.position).normalized;  // 총알 생성지점 보정으로 인한 shootDir 보정(Cursor 위치 보정)
            rot = Atan2Degree(shootDir);
            _bullet = Manager.Pool.UsePool(_bulletType, ShootPoint.position + (shootDir * 0.5f) + _shootPointCorrectionPistol, Quaternion.Euler(0.0f, 0.0f, rot), shootDir);
        }
        else if (gunName >= 100 && _spriteRenderer.flipX) {
            _bullet = Manager.Pool.UsePool(_bulletType, ShootPoint.position + (shootDir * 0.5f) + _shootPointCorrectionRifle, Quaternion.Euler(0.0f, 0.0f, rot), shootDir);
        }
        else {
            _bullet = Manager.Pool.UsePool(_bulletType, ShootPoint.position + (shootDir * 0.5f), Quaternion.Euler(0.0f, 0.0f, rot), shootDir);
        }
        BulletController _bulletController = _bullet.GetComponent<BulletController>();
        //_bulletController.DestPos = (_aimTargetPos - ShootPoint.position).normalized;
        //_bulletController.Gun = this;
        //_bullet.transform.position = ShootPoint.position;  // 총알 생성 위치 설정
        _bulletController.BulletDamage = gunInfo.damage;    // 총알 데미지 설정
        _bulletController.ShooterType = ShooterType;   // 총 발사 주체 설정


        // 총알 소모 카운트
        _ammo -= 1;

        // 총알 잔량 UI에 전달
        if(ShooterType == CreatureType.Player) {
            PlayerController.RemainAmmoAction.Invoke(_ammo);
        }

        if (_ammo == 0) {
            _reload = true;
            if(ShooterType == CreatureType.Player && !playerController.IsFever) {
                Manager.Mouse.ReloadMouseShape();
            }
            yield return _coShootWaitSecReload;

            _coShoot = null;
            _ammo = gunInfo.ammo;
            _reload = false;
            if(ShooterType == CreatureType.Player && !playerController.IsFever) {
                Manager.Mouse.DefaultMouseShape();
            }

            PlayerController.RemainAmmoAction.Invoke(_ammo);
        }
        else {

            yield return _coShootWaitSecShootCoolTime;

            _coShoot = null;
        }

        


        #region Fixed Problem
        //    // [Fixed]Problem 01 : 총알 이동 안 함
        //    // [Sol] bullet Prefab에 Script 사라져 있었음
        //    // [Fixed] Problem 01-01 : bullet Prefab에 Script 추가 후 Bullet이 나타났다 사라[
        //    // [Fixed]Problem 02 : 총알 Pop안 하고 push만 함
        //    // [Sol] PoolManager에서 poolObj가 아닌 경우에 대한 코드가 테스트용으로 계쏙 실행되고 있었음
        //    // [Fixed] Problem 02-01 : 총알이 Pop안 하고 Push해서 사용함
        //    // [Sol] Pool count 문제

        //    // [Fixed] Problem 03 - related with problem 06 : 총알 사라지면 setAactive(false)하고 pool 변화되게 해야
        //    // >> bulletController.cs에서 시야 넘어가면 Destroy() 하는 것 때문으로 보임 수정 필요

        //    // [Fixed]Problme 04 : PoolStack이 empty일 경우 새롭게 오브젝트 생성해서 push 안 함
        //    // [Sol] : PoolManager에서 오브젝트 pool이 비어져 있는 경우 판별에 대한 방식 오류였음
        //    // [Fixed]Problem 05 : 총알 오브젝트의 생성 위치가 Pool 오브젝트 위치임 유저 위치에 따라 가도록 하기
        //    // [Sol] : CreaterController에서 pop된 오브젝트의 초기 위치를 세팅해줌 

        //    // [Fixed]problem 06 : 같은 코드인데 어떤 때는 pop해서 안 쓰고 어떤 때는 pop해서 쓰는데
        //    // pop할 때 PoolObj 리스트에서 위로 순서대로 pop, hieachy상에서만 오브젝트가 있지 push/pop 안 되고 있는 것 같음 
        //    // 마지막에 destroy하는 것 같?
        //    // [Sol] 01. BulletController에서 카메라 범위 벗어날 경우 / 충돌할 경우 두 곳에서 Destroy()였음
        //    // [Fixed]problem 08 : 사용한 총알을 poolStack에 PushPoolChild로 반환하는 부분 수정하면서
        //    // 현재 poolStack 초기화 하는 부분 오류 발생
        //    // [Fixed]Pool Class의 Push 확인 필요

        //    // [Fixed] problem 09 : 총알 발사시 위치 마우스 위치 안 따라감
        //    // [sol] push > pop > push시 dir의 초기화가 이루어지지 않음 > OnEnable을 통해 setActive때마다 초기화 되도록 바꿈
        #endregion

        //    // problem 07 : 최초 총알 발사시 총알 2발이 씹히는 현상 발생 맥에서만 그런 것인지 확인 필[
        //    // problem 10 : 총알 방향 인식이 직전 발사된 방향으로 나감.
        //    // >> 총알 씹히는 것 생각하면 shootDir이 한 템포 늦게 반영되는 것 아닌가?

        //    GameObject go = Resources.Load<GameObject>("Prefabs/Bullet");
        //    GameObject _bullet = Manager.Pool.UsePool(go);
        //    GameObject _gun = GetComponentInChildren<GunController>().gameObject;
        //    // 총알 parent를 별도의 obj로 빼지 말고 총으로 해야지 총알 발사 위치 정하기 좋을 것 같은데.. 수정하;
        //    _bullet.transform.position = _gun.transform.position;

        //    State = CreatureState.Idle;
        //    yield return new WaitForSeconds(ShootCoolTIme);
        //    _triggerOn = true;

        //    _coMakeBullet = null;
        //}
    }
}