using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewScoreData", menuName = "GameData/Score")]
public class ScoreData : ScriptableObject
{
    [SerializeField] private int score = 0;

    // 外部から値を受け取るためのpublicな変数
    public int CurrentScore { get { return score; } }

    // 外部からスコアを追加するためのpublicなメソッド
    public void AddScore(int amount)
    {
        score += amount;
    }

    // スコアを特定の値で上書き（設定）するためのメソッドを追加
    public void SetCurrentScore(int newScore)
    {
        // ここで値のバリデーション（例: 負の値のチェック）や、
        // スコア変更に伴うイベント発行などの処理を一元的に行える
        if (newScore < 0)
        {
            Debug.LogWarning("スコアは負の値に設定できません。");
            return;
        }

        score = newScore;
        Debug.Log($"スコアを上書きしました: {score}");
    }
}
