using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    // 設定したい画面サイズ
    private const int TargetWidth = 960;
    private const int TargetHeight = 540;

    // フルスクリーンモード（タスクバー非表示）
    private const bool IsFullscreen = true;

    // リフレッシュレート。通常は0（現在のディスプレイに合わせる）でOK
    private const int RefreshRate = 0;

    void Awake()
    {
        // アプリケーション起動時に解像度を設定
        Screen.SetResolution(TargetWidth, TargetHeight, IsFullscreen, RefreshRate);
    }
}
