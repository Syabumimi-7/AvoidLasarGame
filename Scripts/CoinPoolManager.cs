using Cysharp.Threading.Tasks; // UniTaskを使用
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

public class CoinPoolManager : MonoBehaviour
{
    // --- インスペクター設定 ---
    [Header("Coin Prefabs")]
    [SerializeField] private GameObject normalCoinPrefab;
    [SerializeField] private GameObject bigCoinPrefab; // 5枚分の大きなコイン

    public ScoreData firstScore;
    public ScoreData secondScore;
    public ScoreData thirdScore;
    public ScoreData fourthScore;
    public ScoreData fifthScore;

    [Header("Pool Settings")]
    [SerializeField] private int defaultPoolSize = 10;
    [SerializeField] private float respawnDelaySeconds = 1f;

    [Header("Spawn Area")]
    // コインをランダムに配置する範囲（例：画面内の矩形領域）
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -3f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, -3f);

    // --- 同時出現設定 ---
    [Header("Spawn Control")]
    [SerializeField] private int maxActiveCoins = 2;

    // --- プールとカウンター ---
    private ObjectPool<GameObject> normalCoinPool;
    private ObjectPool<GameObject> bigCoinPool;

    // ★追加: 現在画面に出ているアクティブなコインの枚数
    private int activeCoinCount = 0;

    // スコア表示用（通常のコインは1, 大きなコインは5）
    public int totalScore = 0;
    // 獲得したコインの総枚数（通し番号）
    private int coinsCollectedCount = 0;
    // ★追加: 次に生成されるべきコインの通し番号
    private int nextCoinSequence = 1;

    // スコア表示用テキスト
    [SerializeField] private TextMeshProUGUI scoreCounter;
    [SerializeField] private TextMeshProUGUI scoreResult;

    // ニューレコード用テキスト
    [SerializeField] private TextMeshProUGUI newRecord;

    private CancellationTokenSource cts;

    // ★追加: 現在画面上にあるコインオブジェクトを追跡するためのリスト
    private List<GameObject> activeCoinObjects = new List<GameObject>();

    [SerializeField] private ScoreManager scoreManager;

    // AudioSourceコンポーネントを格納する変数
    private AudioSource audioSource;

    // Inspectorから設定できるようにpublicまたは[SerializeField]で宣言
    // 再生したい効果音ファイル（AudioClip）を格納する変数
    public AudioClip clickSound;

    // BGMを鳴らしているAudioSourceをInspectorから設定
    public AudioSource bgmAudioSource;

    // 音量を変更するメソッド
    public void SetBGMVolume(float newVolume)
    {
        // newVolumeは 0.0f (ミュート) から 1.0f (最大) の間で設定
        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = newVolume;
        }
    }

    private void Awake()
    {
        cts = new CancellationTokenSource();

        // オブジェクトプールの初期化
        // UnityEngine.Pool.ObjectPool<T> を使用
        normalCoinPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(normalCoinPrefab, transform),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: defaultPoolSize
        );

        bigCoinPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(bigCoinPrefab, transform),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: defaultPoolSize / 5 // 大きなコインは少なくても良い
        );
    }

    private void Start()
    {
        SetBGMVolume(0.3f);
        // アタッチされているAudioSourceコンポーネントを取得
        audioSource = GetComponent<AudioSource>();

        // ★修正: 初期コインをmaxActiveCoinsの数だけ配置する
        for (int i = 0; i < maxActiveCoins; i++)
        {
            // 最初のコインは固定位置、2枚目以降はランダムになるように、
            // SpawnCoinAtRandomPosition()に処理を任せる
            SpawnCoinAtRandomPosition();
        }
    }

    private void OnDestroy()
    {
        // ★追加: オブジェクトが破棄されるときに、非同期処理をキャンセル
        cts?.Cancel();
        cts?.Dispose();
    }

    // コインが獲得されたときに、Coin.csから呼び出される
    public void OnCoinCollected(GameObject coinObject, int coinValue)
    {
        // 紐づけたAudioClipを一度だけ再生する
        audioSource.PlayOneShot(clickSound);

        // 1. カウンターの更新（通し番号を1増やす）
        coinsCollectedCount++; // 獲得したコインの総枚数を常に1増やす

        // 2. スコアの更新
        totalScore += coinValue;
        scoreCounter.text = ": " + totalScore;
        scoreResult.text = "" + totalScore;

        // ★追加: リストから削除
        activeCoinObjects.Remove(coinObject);

        // 3. プールに返却（非アクティブ化）
        if (coinObject.GetComponent<Coin>().IsBigCoin)
        {
            bigCoinPool.Release(coinObject);
        }
        else
        {
            normalCoinPool.Release(coinObject);
        }

        // ★追加: アクティブなコイン数を減らす
        activeCoinCount--;

        // 4. 次のコインを非同期で再配置
        // 現在の画面上のコイン数が最大枚数より少ない場合のみ、リスポーン処理を開始
        if (activeCoinCount < maxActiveCoins)
        {
            RespawnCoinAsync().Forget();
        }
    }

    // UniTaskを使った非同期再配置ロジック
    private async UniTask RespawnCoinAsync()
    {
        var token = cts.Token;

        // 1秒待機 (UniTaskを使用)
        await UniTask.Delay(TimeSpan.FromSeconds(respawnDelaySeconds), ignoreTimeScale: false);
        if (token.IsCancellationRequested) return;
        // 新しいコインを配置
        // ★追加: 遅延後に、まだ画面上のコインが少ない場合のみ出現させる
        if (activeCoinCount < maxActiveCoins)
        {
            SpawnCoinAtRandomPosition();
        }
    }

    // ランダムな位置にコインを配置
    private void SpawnCoinAtRandomPosition()
    {
        // ★修正: nextCoinSequenceを使用
        // 次に生成するコインの種類を、このカウンターに基づいて決定する
        bool isBigCoinTurn = nextCoinSequence % 5 == 0;

        GameObject coinToSpawn;
        ObjectPool<GameObject> targetPool;

        if (isBigCoinTurn)
        {
            targetPool = bigCoinPool;
            //Debug.Log($"次のコインは{nextCoinNumber}枚目なので、大きなコインを配置します。");
        }
        else
        {
            targetPool = normalCoinPool;
            //Debug.Log($"次のコインは{nextCoinNumber}枚目なので、通常のコインを配置します。");
        }

        // プールからオブジェクトを取得（アクティブ化）
        coinToSpawn = targetPool.Get();

        // ★追加: リストに追加
        activeCoinObjects.Add(coinToSpawn);

        // 最初は位置を固定
        // --- 位置決定ロジック ---
        if (nextCoinSequence == 1) // ★修正: nextCoinSequenceを使用
        {
            // 1枚目のみ固定位置（これはそのまま）
            coinToSpawn.transform.position = new Vector3(5f, -3f, 0f);
        }
        else if (nextCoinSequence == 2)
        {
            // 2枚目も固定
            coinToSpawn.transform.position = new Vector3(-5f, -3f, 0f);
        }
        else
        {
            Vector3 newPosition = Vector3.zero;
            bool positionFound = false;
            int safetyCounter = 0; // 無限ループを防ぐためのカウンター

            // 距離チェックのためのループ (最大50回試行)
            while (!positionFound && safetyCounter < 50)
            {
                float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
                float randomY = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
                newPosition = new Vector3(randomX, randomY, 0f);

                positionFound = true; // 仮にOKとする

                // ★チェック: 既存のアクティブなコイン全てに対して距離を確認
                foreach (GameObject activeCoin in activeCoinObjects)
                {
                    // 自分自身（今生成した coinToSpawn）と比較しないようにチェック
                    if (activeCoin == coinToSpawn) continue;

                    // X座標の絶対距離が3f未満の場合、位置を不採用とする
                    if (Mathf.Abs(activeCoin.transform.position.x - newPosition.x) < 3f)
                    {
                        positionFound = false; // NG
                        break;
                    }
                }

                safetyCounter++;
            }

            // 位置が見つからなかった場合（50回試行後もNG）は、最初の候補位置を使うなど
            // フォールバック処理を実装することも可能ですが、ここでは最後に決まった位置を適用します。

            coinToSpawn.transform.position = newPosition;
        }

        // ★重要修正: コイン生成後、次のコインの番号に進める
        nextCoinSequence++;

        // ★追加: 出現時にカウンターを増やす
        activeCoinCount++;
    }

    // ランダムな位置の取得（必要に応じて）
    private Vector3 GetRandomPosition()
    {
        float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector3(randomX, randomY, 0f);
    }

    // ハイスコアの設定
    public void CheckScore()
    {
        if (totalScore > ScoreManager.Instance.HighScore)
        {
            newRecord.text = "";
            Debug.Log("New Record!");
        }
        ScoreManager.Instance.CheckAndSetHighScore(totalScore);
    }

    public void NewRecord()
    {
        newRecord.text = "NEW RECORD!";
    }

}