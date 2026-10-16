using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToPlayScene : MonoBehaviour
{
    // AudioSourceコンポーネントを格納する変数
    private AudioSource audioSource;

    // Inspectorから設定できるようにpublicまたは[SerializeField]で宣言
    // 再生したい効果音ファイル（AudioClip）を格納する変数
    public AudioClip clickSound;

    void Start()
    {
        // アタッチされているAudioSourceコンポーネントを取得
        audioSource = GetComponent<AudioSource>();
    }

    public void ToPlay()
    {
        audioSource.PlayOneShot(clickSound);
        Time.timeScale = 1f;
        SceneManager.LoadScene("PlayScene");
    }
}
