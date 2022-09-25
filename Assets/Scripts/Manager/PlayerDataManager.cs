using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumList;

public class PlayerDataManager {
    // TODO multiplayer player data Diecionary
    public Dictionary<int, Transform> playerInfo = new Dictionary<int, Transform>();
    // temp player data
    public Transform playerPosition;

    public GameState GamePlayState;
}
