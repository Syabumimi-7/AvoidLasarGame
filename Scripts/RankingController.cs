using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RankingController : MonoBehaviour
{
    // scoreを入れる変数
    [SerializeField] private ScoreData firstScore;
    [SerializeField] private ScoreData secondScore;
    [SerializeField] private ScoreData thirdScore;
    [SerializeField] private ScoreData fourthScore;
    [SerializeField] private ScoreData fifthScore;

    // scoreランキングを表示するテキスト
    [SerializeField] private TextMeshProUGUI rankingScore;

    void Start()
    {
        // ScoreDataにスコアを入れる
        int firstScoreText = firstScore.CurrentScore;
        int secondScoreText = secondScore.CurrentScore;
        int thirdScoreText = thirdScore.CurrentScore;
        int fourthScoreText = fourthScore.CurrentScore;
        int fifthScoreText = fifthScore.CurrentScore;

        // 読み取ったスコアをUIに表示
        rankingScore.text = firstScoreText + "点" + "\n" +
                            secondScoreText + "点" + "\n" +
                            thirdScoreText + "点" + "\n" +
                            fourthScoreText + "点" + "\n" +
                            fifthScoreText + "点";
    }
}
