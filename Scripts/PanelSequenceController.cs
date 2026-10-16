using UnityEngine;

public class PanelSequenceController : MonoBehaviour
{
    // AudioSourceコンポーネントを格納する変数
    private AudioSource audioSource;

    // Inspectorから設定できるようにpublicまたは[SerializeField]で宣言
    // 再生したい効果音ファイル（AudioClip）を格納する変数
    public AudioClip clickSound;

    // Inspectorからパネルを設定するための変数
    public GameObject panel1;
    public GameObject panel2;
    public GameObject panel3;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // アプリ起動時の初期状態：Panel 1を表示し、他を非表示にする
        ShowPanel(panel1);
    }

    // --- ヘルパー関数: パネルの表示を切り替える ---
    private void ShowPanel(GameObject panelToShow)
    {
        // すべてのパネルの状態をリセットし、指定されたパネルのみをアクティブにする
        panel1.SetActive(panelToShow == panel1);
        panel2.SetActive(panelToShow == panel2);
        panel3.SetActive(panelToShow == panel3);
    }

    // --- 遷移用関数: 各ボタンに割り当てる ---

    // 1 -> 2 へ進む (Panel 1 にあるボタンに割り当てる)
    public void GoNextTo2()
    {
        audioSource.PlayOneShot(clickSound);
        ShowPanel(panel2);
    }

    // 2 -> 1 へ戻る (Panel 2 にある戻るボタンに割り当てる)
    public void GoPrevTo1()
    {
        audioSource.PlayOneShot(clickSound);
        ShowPanel(panel1);
    }

    // 2 -> 3 へ進む (Panel 2 にある進むボタンに割り当てる)
    public void GoNextTo3()
    {
        audioSource.PlayOneShot(clickSound);
        ShowPanel(panel3);
    }

    // 3 -> 2 へ戻る (Panel 3 にあるボタンに割り当てる)
    public void GoPrevTo2()
    {
        audioSource.PlayOneShot(clickSound);
        ShowPanel(panel2);
    }
}