using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static SceneTransitionManager Instance { get; private set; }

    [Header("フェード設定")]
    [SerializeField] private Image fadePanel; // 画面全体を覆うImageコンポーネント
    [SerializeField] private float fadeDuration = 0.2f; // フェードにかける時間

    private void Awake()
    {
        // シングルトンの設定（これがない場合は自身をインスタンスにする）
        if (Instance == null)
        {
            Instance = this;
            // シーンを跨いでオブジェクトを維持
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 既に存在する場合は自身を破棄
            Destroy(gameObject);
            return;
        }

        // 初期状態でフェードインを実行
        // SceneManager.sceneLoaded += OnSceneLoaded; // イベントを使う方法もありますが、Startでシンプルに

        // 初期状態では画面を暗くしておく（フェードインの準備）
        Color color = fadePanel.color;
        color.a = 1f;
        fadePanel.color = color;
        fadePanel.gameObject.SetActive(true);

        // 最初のシーンのフェードインを開始
        StartCoroutine(FadeIn());
    }

    /// <summary>
    /// 指定したシーン名へフェードアウト・フェードインを挟んで遷移する
    /// </summary>
    /// <param name="sceneName">遷移先のシーン名</param>
    public void LoadScene(string sceneName)
    {
        // フェード処理中に再度遷移が実行されないようガードする
        if (IsFading) return;

        StartCoroutine(FadeOutAndLoadScene(sceneName));
    }

    // フェード処理中かどうかの判定用
    public bool IsFading { get; private set; } = false;

    // --- コルーチンによるフェード処理 ---

    // フェードアウト処理（画面を明るい状態から暗くする）
    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        IsFading = true;
        fadePanel.gameObject.SetActive(true); // フェードパネルを有効化

        float timer = 0f;
        Color startColor = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 0f); // 透明
        Color endColor = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 1f); // 不透明

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        fadePanel.color = endColor; // 確実に不透明にする

        // シーン切り替え
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);

        // 新しいシーンがロードされたら、続けてフェードインを開始
        StartCoroutine(FadeIn());
    }

    // フェードイン処理（画面を暗い状態から明るくする）
    private IEnumerator FadeIn()
    {
        IsFading = true;
        fadePanel.gameObject.SetActive(true); // フェードパネルを有効化

        float timer = 0f;
        Color startColor = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 1f); // 不透明
        Color endColor = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 0f); // 透明

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadePanel.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            yield return null;
        }

        fadePanel.color = endColor; // 確実に透明にする
        fadePanel.gameObject.SetActive(false); // 完全に透明になったら無効化してUI操作を可能にする
        IsFading = false;
    }
}