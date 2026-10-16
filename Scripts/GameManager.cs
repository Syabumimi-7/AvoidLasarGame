using UnityEngine;
using Cysharp.Threading.Tasks; // UniTaskのために必要
using System; // TimeSpanのために必要
using System.Threading; // CancellationTokenのために必要

public class GameManager : MonoBehaviour
{
    // --- Inspector設定 ---
    [SerializeField] private GameObject gameOverUIPanel;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private CoinPoolManager coinPoolManager;

    // --- 時間制限設定 ---
    private const float GAME_DURATION_SECONDS = 60f;

    // --- 内部管理 ---
    private CancellationTokenSource cts;

    // AudioSourceコンポーネントを格納する変数
    private AudioSource audioSource;

    // Inspectorから設定できるようにpublicまたは[SerializeField]で宣言
    // 再生したい効果音ファイル（AudioClip）を格納する変数
    public AudioClip clickSound;

    private void Start()
    {
        // アタッチされているAudioSourceコンポーネントを取得
        audioSource = GetComponent<AudioSource>();

        // CancellationTokenSourceを初期化
        cts = new CancellationTokenSource();

        // 時間制限タイマーを開始
        StartGameTimerAsync(cts.Token).Forget();
    }

    private void OnDestroy()
    {
        // オブジェクト破棄時に非同期タスクをキャンセル
        cts?.Cancel();
        cts?.Dispose();
    }

    // ゲームタイマーの非同期処理
    private async UniTask StartGameTimerAsync(CancellationToken token)
    {
        // 60秒間待機
        await UniTask.Delay(TimeSpan.FromSeconds(GAME_DURATION_SECONDS), cancellationToken: token);

        // 待機中にキャンセルされていれば処理を終了
        if (token.IsCancellationRequested) return;

        // 60秒経過後、ゲームオーバーメソッドを呼び出す
        GameOver();
    }

    public void GameOver()
    {
        // 既にゲームオーバー処理が走っている場合、二重実行を防ぐ
        if (Time.timeScale == 0f) return;

        // 紐づけたAudioClipを一度だけ再生する
        audioSource.PlayOneShot(clickSound);

        // ハイスコアの処理
        coinPoolManager.CheckScore();

        // 1. ゲーム時間の停止
        Time.timeScale = 0f;

        // 2. カーソルを表示 (ボタン操作のため)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. タイマーをキャンセル (レーザーヒットでGameOverした場合は、時間切れタイマーを止める)
        cts?.Cancel();

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // 4. ゲームオーバーUIパネルを表示
        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(true);
        }
    }
}
