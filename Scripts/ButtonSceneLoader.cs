using UnityEngine;

public class ButtonSceneLoader : MonoBehaviour
{
    // 遷移先のシーン名をインスペクターから設定できるようにする
    [SerializeField] private string targetSceneName = "";

    /// <summary>
    /// ボタンのOnClick()イベントから呼び出す関数
    /// </summary>
    public void OnClickLoadScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("遷移先のシーン名が設定されていません。");
            return;
        }

        // SceneTransitionManagerのシングルトンインスタンスを通じてシーン遷移を呼び出す
        SceneTransitionManager.Instance.LoadScene(targetSceneName);
    }
}