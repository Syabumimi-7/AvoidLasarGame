using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class UFOController : MonoBehaviour
{
    // --- インスペクター設定 ---
    [Header("Laser Prefab")]
    [SerializeField] private GameObject laserPrefab;
    [SerializeField] private Transform laserSpawnPoint; // 光線が出現するUFOの下の位置

    [Header("Timings")]
    private const float INITIAL_DELAY = 1f; // UFO出現から光線出現までの待機時間
    private const float LASER_DURATION = 2f;  // 光線が表示される時間
    private const float RESPAWN_DELAY = 1f; // UFOと光線が消えてから再出現するまでの待機時間

    [Header("Spawn Area")]
    // コインマネージャーと同じランダム配置範囲を考慮
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, 3f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 3f);

    // --- プールとインスタンス ---
    private GameObject currentLaser;

    // UniTaskのキャンセル処理用トークン
    private CancellationTokenSource cts;

    // 1回目のループかどうか
    public bool first = true;

    private void Start()
    {
        // 初期光線オブジェクトを生成し、非アクティブにしておく（簡単なプール代わり）
        currentLaser = Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.identity, transform);
        currentLaser.SetActive(false);

        // CancellationTokenSourceを初期化
        cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        // オブジェクトが破棄されるときに、非同期処理をキャンセル
        cts?.Cancel();
        cts?.Dispose();
    }

    // UFOの行動を外部（EnemySpawner）から呼び出すためのメソッド群
    public async UniTask ShootLaserAsync(CancellationToken token)
    {
        // 1. 1秒待機 (UFO出現から光線出現まで)
        await UniTask.Delay(TimeSpan.FromSeconds(INITIAL_DELAY), cancellationToken: token);
        if (token.IsCancellationRequested) return;

        // 2. 光線の出現
        ShootLaser();

        // 3. 2秒待機 (光線が表示される時間)
        await UniTask.Delay(TimeSpan.FromSeconds(LASER_DURATION), cancellationToken: token);
        if (token.IsCancellationRequested) return;

        // 4. 光線の消滅
        HideLaser();
    }

    // UFOを非アクティブにする処理をパブリック化
    public void HideUFO()
    {
        gameObject.SetActive(false);
    }

    // 内部関数も必要に応じてパブリック化、または内部処理を整理
    private void ShootLaser()
    {
        Debug.Log($"Laser Activated at: {currentLaser.transform.position}");
        // 光線をUFOの下に再配置し、アクティブにする
        currentLaser.transform.position = laserSpawnPoint.position;
        currentLaser.SetActive(true);
    }

    private void HideLaser()
    {
        // 光線を非アクティブにする
        currentLaser.SetActive(false);
    }

    // RespawnUFO()は位置設定のために残しますが、SetActive(true)の行は削除します。
    // （SetActive(true)はSpawnerが制御するため）
    public void RespawnUFO()
    {
        // 1. 位置を設定
        if (first)
        {
            transform.position = new Vector3(-5f, 3f, -0.5f);
            first = false;
        }
        else
        {
            transform.position = GetRandomPosition();
        }
    }

    private void HideLaserAndUFO()
    {
        // 光線とUFOを非アクティブにする
        currentLaser.SetActive(false);
        gameObject.SetActive(false);
    }

    // ランダムな位置を決定
    private Vector3 GetRandomPosition()
    {
        float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        // Z軸は2Dなので0とするか、適切な値に設定
        return new Vector3(randomX, randomY, -0.5f);
    }
}