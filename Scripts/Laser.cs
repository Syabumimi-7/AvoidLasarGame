using UnityEngine;

public class Laser : MonoBehaviour
{
    // ゲームオーバーを管理するマネージャーへの参照
    // （ここでは簡単のためFindObjectOfTypeを使用しますが、SingletonやDependency Injectionがより良い方法です）
    private GameManager gameManager;

    private void Start()
    {
        // 適切なGameManagerを見つける
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManagerが見つかりません！");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameManager != null)
            {
                gameManager.GameOver(); // ゲームオーバー処理を呼び出す
            }
        }
    }
}
