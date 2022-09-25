using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumList;

public class BulletController : CreatureController {
    Vector3 _bullPos;
    Camera _cam;
    int _damage = 0;
    public int BulletDamage { get { return _damage; } set { _damage = value; } }
    public GunController Gun { private get; set; }
    public CreatureType ShooterType { private get; set; }
    public Vector3 DestPos { get { return _destPos; } set { _destPos = value; } }

    float time = 0.0f;

    // TODO
    // bullet 종류마다 buttet 클래스를 상속 받는 별도의 클래스를 따로 두고 관리하는 것보다
    // 하나의 bullet 클래스로 두고 데이터 값에 따라 bulelt을 분류하는 것이
    // 새로운 bullet를 추가 / 관리하기 편하다


    private void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
        _speed *= 2.5f;
        State = CreatureState.Move;

    }

    protected override void UpdateController() {
        base.UpdateController();
    }


    protected override void UpdateMoving() {
        //_rigidbody.velocity = _destPos * _speed;
        _rigidbody.MovePosition(transform.position + _destPos * _speed * Time.fixedDeltaTime);
        _bullPos = _cam.WorldToViewportPoint(transform.position);
        if (_bullPos.x <= -0.05f || _bullPos.x >= 1.05f || _bullPos.y <= -0.05f || _bullPos.y >= 1.05f) {
            //_shooter = null;
            ShooterType = CreatureType.Null;
            Manager.Pool.PushPoolChild(this.gameObject);
        }
    }

    
    private void OnTriggerEnter2D(Collider2D collision) {
        // 총알 간의 충돌이 발생하는 경우 || 아이템과 충돌하는 경우 
        if (collision.gameObject.layer.Equals(8) || collision.gameObject.layer.Equals(9)) {
            return;
        }
        // Player가 쏜 총알이 Enemy와 충돌하는 경우
        else if (collision.gameObject.layer.Equals(10)) {
            if(ShooterType == CreatureType.Player) {
                //Debug.Log("OnTrigger : Player to Enemy");
                // bullet이 enemy에게 damage 가함
                EnemyController enemy = collision.GetComponent<EnemyController>();

                if(enemy != null) {
                    enemy.HP -= _damage;
                    ShooterType = CreatureType.Null;
                    Manager.Pool.PushPoolChild(gameObject);
                }
            }
        }
        // Enemy가 쏜 총알이 Player와 충돌하는 경우
        else if (collision.gameObject.layer.Equals(6)) {
            if(ShooterType == CreatureType.Enemy) {
                //Debug.Log("OnTrigger : Enemy to Player");
                // bullet이 player에게 damage 가함 
                PlayerController player = collision.GetComponent<PlayerController>();
                if(player.IsFever == false) {
                    player.HP -= _damage;
                }
                
                ShooterType = CreatureType.Null;
                Manager.Pool.PushPoolChild(gameObject);
            }
        }
        else {
            Manager.Pool.PushPoolChild(gameObject);
        }

        
    }
}
