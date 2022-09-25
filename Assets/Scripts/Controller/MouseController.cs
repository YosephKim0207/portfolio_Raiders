using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {
    GameObject _mouse;
    

    void Start() {
        
        _mouse = Object.Instantiate(Resources.Load<GameObject>("Prefabs/Mouse"));
    }

    void Update() {
        MouseTracking();
    }

    void MouseTracking() {
        Vector3 mousePos;
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f));
        _mouse.transform.position = mousePos;
        Debug.Log("mouse pos : " + mousePos);

        // 마우스 왼쪽 버튼 클릭시
        if (Input.GetMouseButton(0)) {
            Debug.Log("mouse click");
        }
    }
}
