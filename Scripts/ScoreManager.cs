using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static ScoreManager Instance { get; private set; }

    // シーンをまたいで保持したいハイスコア
    public int HighScore { get; private set; } = 0;

    [SerializeField] private CoinPoolManager coinPoolManager;

    private void Awake()
    {
        // シングルトンの初期化とインスタンスの重複防止
        if (Instance == null)
        {
            Instance = this;
            // シーンが切り替わってもこのオブジェクトを破棄しない
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ゲーム終了時（ゲームオーバー時など）に呼び出し、
    /// 既存のスコアとハイスコアを比較・更新する
    /// </summary>
    public void CheckAndSetHighScore(int finalScore)
    {
        // 今回のスコアがハイスコアより高ければ更新
        if (finalScore > HighScore)
        {
            HighScore = finalScore;
            Debug.Log("ハイスコア更新: " + HighScore);
        }
        else
        {
            Debug.Log("ハイスコアは更新されませんでした。現在のハイスコア: " + HighScore);
        }
    }
}