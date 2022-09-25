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
        }
        // _shooter가 Enemy인 경우
        else {
            ShooterType = CreatureType.Enemy;
            playerController = GameObject.Find("Player").GetComponent<PlayerController>();
            _aimTargetPos = playerController.transform.position;
            
        }

        GetGunInfo();
        gunName = int.Parse(gunInfo.name);
    }

    private void Start() {
        Init();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

    }

    private void OnDisable() {
        if(ShooterType == CreatureType.Player) {
            Transform bulletPool = _bullet.transform.parent;
            if (bulletPool != null) {
                Manager.Pool.DeletePool(_bulletType);
            }
        }
    }

    private void Update() {
            RotateGun();

            if (_triggerState && (_ammo > 0)) {
                _triggerState = false;
                _coShoot = StartCoroutine("CoShoot");
            }
    }

    protected void Init() {
        GetGunInfo();
    }

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
    }
}