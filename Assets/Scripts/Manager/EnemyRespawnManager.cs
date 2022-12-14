using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static EnumList;

public class EnemyRespawnManager {
    Camera _cam;
    int _respawnRange = 5;
    int _remainEnemy = 0;
    public int RemainEnemy { get { return _remainEnemy; } set { _remainEnemy = value; } }
    MapManager.Pos pos;
    MapManager.Pos playerPos;
    MapManager.Pos respawnRangePos;
    Stack<MapManager.Pos> respawnPosStack = new Stack<MapManager.Pos>();
    Vector3 viewLeftDownPos = new Vector3(0.0f, 0.0f, -10.0f);
    Vector3 vidwRightUpPos = new Vector3(1.0f, 1.0f, -10.0f);
    float _minX;
    float _maxX;
    float _minY;
    float _maxY;
    GameObject go;

    MapManager.Pos[] respawnArray;


    public void Init() {
        bool[,] _collisionData = Manager.Map._collsionData;
        _cam = Camera.main;
        CheckMaxCamRange();
    }




    public void Respawn(Vector3 _playerTransPos, GameObject enemy, RespawnPattern respawnPattern = RespawnPattern.NormalRandom, int density = 1) {
        respawnPosStack.Clear();
        playerPos = Manager.Map.CellToArrPos(Manager.Map.Grid.WorldToCell(_playerTransPos));


        switch (respawnPattern) {
            case RespawnPattern.NormalRandom:
                break;
            case RespawnPattern.Square:
                // Temp
                SetMaxRespawnRange(_playerTransPos, CheckMaxCamRange());

                SetSquarePattern(_respawnRange, playerPos);
                MakeEnemy(enemy, density);
                break;
        }

    }

    void MakeEnemy(GameObject enemy, int density = 1) {
        if (density < 1 || density > respawnPosStack.Count) {
            density = respawnPosStack.Count / 4;
        }

        for (int i = 0; i < density; ++i) {
            pos = respawnArray[i];
            Vector3Int cellPos = Manager.Map.ArrToCellPos(pos);
            go = Manager.Pool.UsePool(enemy);
            go.transform.position = cellPos;
            go = null;

            RemainEnemy += 1;
            Debug.Log(Manager.EnemyRespawn.RemainEnemy);

        }

    }

    float CheckMaxCamRange() {
        // cam??? ?????? ???????????? ?????? ?????? ??? ???????????? min/max ???????????? ????????? ??????
        Vector3 worldLeftDownPos = _cam.ViewportToWorldPoint(viewLeftDownPos);
        Vector3 worldRightUpPos = _cam.ViewportToWorldPoint(vidwRightUpPos);
        _maxX = worldLeftDownPos.x;
        _minX = worldRightUpPos.x;
        _maxY = worldLeftDownPos.y;
        _minY = worldRightUpPos.y;

        return _maxX;
        Debug.Log($"Cam maxX : {_maxX}");
    }

    // ????????? ????????? ????????? ?????? ????????? ?????? ???????????? respawnPosStack??? Push
    void SetSquarePattern(int distance, MapManager.Pos playerArrPos) {
        if (distance < 1) {
            distance = 10;
        }


        MapManager.Pos respawnPos = playerArrPos;
        respawnPos.X -= distance;
        respawnPos.Y -= distance;
        int initYPos = respawnPos.Y;

        // ????????? ?????? ?????? ??? ?????? ??????
        for (int i = 0; i <= distance * 2; ++i) {
            for (int j = 0; j <= distance * 2; ++j) {

                // ????????? / ????????? ?????????
                if (i == j) {
                    ++respawnPos.Y;
                    continue;
                }

                // ????????? ??????
                if ((i != 0 && i != distance * 2) && (j > 0 && j < distance * 2)) {
                    ++respawnPos.Y;
                    continue;
                }

                // ????????? / ????????? ?????????
                if (i == (distance * 2 - j)) {
                    ++respawnPos.Y;
                    continue;
                }

                // collision??? ??????, map ?????? ???????????? respawn ????????? ??????
                if (Manager.Map.CheckCollision(respawnPos)) {
                    respawnPosStack.Push(respawnPos);
                    ++respawnPos.Y;
                }

                // enemy respawn ????????? ?????? ????????? ??????
                respawnArray = respawnPosStack.OrderBy(pos => Random.value).ToArray();
            }

            respawnPos.Y = initYPos;
            ++respawnPos.X;
        }
    }

    void SetMaxRespawnRange(Vector3 playerTransPos, float maxCamRange) {
        playerTransPos.x = maxCamRange - playerTransPos.x;
        respawnRangePos = Manager.Map.CellToArrPos(Manager.Map.Grid.WorldToCell(playerTransPos));

        _respawnRange = respawnRangePos.X - playerPos.X + 1;
    }
}