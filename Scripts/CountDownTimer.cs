using UnityEngine;
using UnityEngine.UI; // Textコンポーネントを使うために必要です
using TMPro; // TextMeshProを使う場合はこちらも必要です（使用するTextに合わせて調整）

public class CountDownTimer : MonoBehaviour
{
    // 初期設定時間（秒）
    [SerializeField] private float timeLimit = 61f;

    // 現在の残り時間
    private float currentTime;

    // 時間を表示するTextコンポーネント
    [SerializeField] private TextMeshProUGUI timerText; // TextMeshProUGUIの場合

    // もし通常のUnityEngine.UI.Textを使用する場合はこちら
    // [SerializeField] private Text timerText; 

    private bool isCounting = true;

    void Start()
    {
        // 初期残り時間を設定
        currentTime = timeLimit;

        // 初回表示の更新
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (isCounting)
        {
            // 前のフレームからの経過時間を引く
            currentTime -= Time.deltaTime;

            // 表示の更新
            UpdateTimerDisplay();

            // 0秒以下になったかチェック
            if (currentTime <= 0f)
            {
                currentTime = 0f; // マイナスにならないように0に固定
                isCounting = false; // カウントダウンを停止

                // 終了時の処理（ゲームオーバー処理など）をここに記述
                Debug.Log("タイムアップ！");
                UpdateTimerDisplay(); // 0秒になったことを確定表示
            }
        }
    }

    // 時間表示を更新するメソッド
    void UpdateTimerDisplay()
    {
        // currentTimeを整数（秒）に丸める
        int seconds = Mathf.FloorToInt(currentTime);

        // 「残りXX秒」の形式で表示を更新
        timerText.text = seconds.ToString();
    }
}
