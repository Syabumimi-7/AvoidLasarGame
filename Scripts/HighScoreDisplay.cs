using UnityEngine;
using UnityEngine.UI; // UI要素を操作するために必要
using TMPro;

public class HighScoreDisplay : MonoBehaviour
{
    // ハイスコアを表示するためのTextコンポーネント
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        // Textコンポーネントが設定されているか確認
        if (highScoreText == null)
        {
            Debug.LogError("ハイスコアを表示するための Text コンポーネントが設定されていません。");
            return;
        }

        // ScoreManagerのインスタンスがシーンに存在するか確認
        if (ScoreManager.Instance != null)
        {
            // シングルトンインスタンスを通じてハイスコアの値を取得し、表示を更新
            int currentHighScore = ScoreManager.Instance.HighScore;

            highScoreText.text = "ハイスコア: " + currentHighScore.ToString();
            Debug.Log("別のシーンでハイスコアをロードしました: " + currentHighScore);
        }
        else
        {
            // ScoreManagerがシーンに存在しない場合（通常、最初にゲーム開始シーンをロードしていない場合）
            highScoreText.text = "ハイスコア: 0";
            //Debug.LogError("ScoreManagerのインスタンスが見つかりません。");
        }
    }
}