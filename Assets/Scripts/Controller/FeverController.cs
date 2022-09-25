using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeverController : ItemController {
    float _feverTime = 10.0f;
    protected override void Init() {
        _itemName = EnumList.ItemName.Fever;
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<PlayerController>() != null) {
            _playerController = collision.GetComponent<PlayerController>();
        }

        // TODO
        // 아이템 습득시 UI_Fever 적용시키기

        // Fever 아이템 습득에 따른 fever state 시간 설정 및 fever 기능 on 후 필드 아이템 제거 
        if (_playerController != null && !_playerController.HasFever) {
            //fever = Resources.Load<GameObject>($"Prefabs/UI/UI_Fever");
            //fever = Object.Instantiate<GameObject>(fever);

            Manager.UI.ShowPopupUI<UI_Fever>(useRootCanvas : false);

            _playerController.FeverTime = _feverTime;
            _playerController.HasFever = true;
            Destroy(this.gameObject);
        }
    }
}