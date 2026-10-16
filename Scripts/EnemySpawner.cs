using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // --- インスペクター設定 ---
    [Header("Enemy Prefab")]
    [SerializeField] private UFOController ufoPrefab; // UFOControllerがアタッチされたPrefab

    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, 3f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, 3f);
    [SerializeField] private float minDistanceBetweenUFOs = 2.0f; // UFO間の最小距離

    // --- 内部管理 ---
    private List<UFOController> activeUFOs = new List<UFOController>();
    private CancellationTokenSource cts;
    private int loopCount = 0;
    private const int SPAWN_CYCLE_COUNT = 5;

    private bool secondUFOSpawned = false;

    private void Start()
    {
        cts = new CancellationTokenSource();

        // Sceneに配置済みの最初のUFOがいれば、そのインスタンスをリストに追加する
        var initialUFO = FindObjectOfType<UFOController>();
        if (initialUFO != null && !activeUFOs.Contains(initialUFO))
        {
            activeUFOs.Add(initialUFO);
        }
        StartSynchronizedUFOLoopAsync().Forget();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }

/*    private async UniTask DelayedSpawnSecondUFOAsync()
    {
        var token = cts.Token;

        // 20秒待機
        await UniTask.Delay(TimeSpan.FromSeconds(spawnTime), cancellationToken: token);

        if (token.IsCancellationRequested) return;

        // 2つ目のUFOを生成
        SpawnNewUFO();
    }*/

    private void SpawnNewUFO()
    {
        // 1. 重複しないランダムな位置を取得
        Vector3 newPosition = GetNonOverlappingRandomPosition();

        if (newPosition != Vector3.zero)
        {
            // 2. UFOを生成
            var newUFO = Instantiate(ufoPrefab, newPosition, Quaternion.identity);

            // 3. UFOController側の固定位置ロジックをスキップするため、firstをfalseに設定
            // (UFOController側の first 変数が public か [SerializeField] である前提)
            newUFO.first = false;

            // 4. 管理リストに追加
            activeUFOs.Add(newUFO);
            Debug.Log($"20秒経過。2つ目のUFOを生成しました。現在のUFO数: {activeUFOs.Count}");
        }
        else
        {
            Debug.LogWarning("重複しない位置が見つからなかったため、2つ目のUFOの生成をスキップしました。");
        }
    }

    // 全てのUFOの動作を同期させるメインループ
    private async UniTask StartSynchronizedUFOLoopAsync()
    {
        var token = cts.Token;
        while (!token.IsCancellationRequested)
        {
            // サイクル開始 (T=0, 4, 8, 12, 16, 20...)

            if (loopCount == SPAWN_CYCLE_COUNT && !secondUFOSpawned)
            {
                // 2つ目のUFOを生成し、リストに追加する
                SpawnNewUFO();

                // ★追加: 生成フラグをtrueにする
                secondUFOSpawned = true;

                // loopCountはこのままでOKですが、
                // 処理が完了したことを明確にするため、次の出現時にはifに入らないようにSPAWN_CYCLE_COUNT以上の値に設定します。
                loopCount = SPAWN_CYCLE_COUNT + 1;

                Debug.Log("2つ目のUFOを生成しました。");
            }

            // サイクルカウンターを進める (生成が完了しなかった場合のみ)
            // 生成完了後の loopCount = 6 (SPAWN_CYCLE_COUNT + 1) の場合はカウントしない
            if (!secondUFOSpawned)
            {
                loopCount++;
            }

            // === 1. UFOの再出現フェーズ (この時点でactiveUFOsには2つ目のUFOが含まれている) ===
            foreach (var ufoController in activeUFOs.Where(u => u != null))
            {
                // 位置を設定し、アクティブ化
                ufoController.RespawnUFO();
                ufoController.gameObject.SetActive(true);
            }

            // === 2. 光線フェーズ (1秒待機 + 2秒光線) ===
            var laserTasks = activeUFOs
                             .Where(u => u != null)
                             .Select(ufoController => ufoController.ShootLaserAsync(token)) // そのまま呼び出す
                             .ToArray();

            await UniTask.WhenAll(laserTasks);
            if (token.IsCancellationRequested) return;

            // === 3. 消滅フェーズ ===
            foreach (var ufo in activeUFOs.Where(u => u != null))
            {
                // UFOを非アクティブにする
                ufo.HideUFO();
            }

            // === 4. 再出現までの待機フェーズ (1秒) ===
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);

            // ループの最初に戻り、同時に再出現
            if (token.IsCancellationRequested) return;
        }
    }

    // 重複しないランダムな位置を探索するロジック (変更なし)
    private Vector3 GetNonOverlappingRandomPosition()
    {
        const int maxAttempts = 100;
        float zDepth = ufoPrefab.transform.position.z;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = UnityEngine.Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float randomY = UnityEngine.Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector3 potentialPosition = new Vector3(randomX, randomY, zDepth);

            bool isOverlapping = false;
            foreach (var ufo in activeUFOs.Where(u => u != null))
            {
                if (Vector3.Distance(ufo.transform.position, potentialPosition) < minDistanceBetweenUFOs)
                {
                    isOverlapping = true;
                    break;
                }
            }

            if (!isOverlapping)
            {
                return potentialPosition;
            }
        }

        return Vector3.zero;
    }
}
