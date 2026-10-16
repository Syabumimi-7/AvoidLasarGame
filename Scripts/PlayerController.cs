using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    void Update()
    {
        // Playerの移動
        if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
             transform.Translate(-0.4f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            transform.Translate(0.4f, 0f, 0f);
        }

        // playerの位置を取得
        Vector3 playerPos = transform.position;
        float xPos = playerPos.x;

        // 左右の端でループするようにx座標を変更
        if(xPos < -10.2)
        {
            xPos = 10.1f;
        }
        if(xPos >10.2)
        {
            xPos = -10.1f;
        }

        // 変更したx座標をplayerPosに反映
        playerPos.x = xPos;

        // transform.position全体に新しいVector2を代入して位置を更新
        transform.position = playerPos;
    }
}
