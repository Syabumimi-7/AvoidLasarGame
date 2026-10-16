using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1; // 通常コインは1
    [SerializeField] private bool isBigCoin = false; // 大きなコインはtrue (値は5)

    // プロパティ
    public bool IsBigCoin => isBigCoin;

    // プールマネージャーへの参照
    private CoinPoolManager poolManager;

    // パーティクルのプレハブ
    public GameObject particlePrefab;

    private void Start()
    {
        // プールマネージャーの取得（シングルトンやFindObjectOfTypeなど、適切な方法で）
        poolManager = FindObjectOfType<CoinPoolManager>();
        if (poolManager == null)
        {
            Debug.LogError("CoinPoolManagerが見つかりません！");
        }

        // 大きなコインの場合、値を5に設定（インスペクターで設定しても良い）
        if (isBigCoin)
        {
            coinValue = 5;
        }
    }

    // プレイヤーとの衝突を検知
    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーのタグやレイヤーを確認
        if (other.CompareTag("Player"))
        {
            if (poolManager != null)
            { 
                poolManager.OnCoinCollected(gameObject, coinValue);
            }
            else
            {
                // シーン遷移などでマネージャーが破棄されていた場合は、処理をスキップする
                Debug.LogWarning("CoinPoolManagerが破棄されているため、獲得処理をスキップしました。", this);
            }

            // プレイヤーの位置、無回転でプレハブをインスタンス化
            GameObject effectInstance = Instantiate(
                particlePrefab,
                transform.position,
                Quaternion.identity
            );

            // インスタンス化されたパーティクルシステムを取得
            ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                // ParticleSystemの再生を開始
                // CFXR_Effectがアタッチされている場合、Play()を呼び出すことでエフェクトが開始され、
                // 再生終了後に自動で破棄されます。
                ps.Play();
            }
        }
    }
}