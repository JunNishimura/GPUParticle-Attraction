// reference : https://qiita.com/Nekomasu/items/dcdf73f221fd64875ef0

using System;
using System.IO;
using System.Collections;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
    //タイムスタンプの定義
    public enum TIME_STAMP
    {
        MMDDHHMMSS,
        YYYYMMDDHHMMSS,
    }

    [SerializeField, Tooltip("タイムスタンプの書式設定")]
    private TIME_STAMP _timeStampStyle;

    //ファイル名の指定
    [SerializeField, Tooltip("ファイル名の末尾に付く文字")]
    private string _imageTitle = "img";

    //保存先の指定 (末尾に / を付けてください)
    [SerializeField, Tooltip("ファイルの保存先 末尾の/ を含めてください")]
    private string _screenShotFolder = "/ScreenShots/";

    //撮影ボタンの表示切替
    [SerializeField, Tooltip("trueならGUIの撮影ボタンを表示します")]
    private bool _shotButtonActive = false;

    void Update()
    {
        //「Q」でボタンの表示切替
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _shotButtonActive = !_shotButtonActive;
        }

        //「P」で撮影
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Application.dataPath = ../Assets
            string path = Application.dataPath + _screenShotFolder;
            StartCoroutine(imageShooting(path, _imageTitle));
        }
    }

    //撮影ボタン設定
    void OnGUI()
    {
        if (_shotButtonActive == false) { return; }

        if (GUI.Button(new Rect(10, 10, 40, 20), "Shot"))
        {
            // Application.dataPath = ../Assets
            string path = Application.dataPath + _screenShotFolder;
            StartCoroutine(imageShooting(path, _imageTitle));
        }
    }

    //撮影処理
    //第一引数 ファイルパス / 第二引数 タイトル
    private IEnumerator imageShooting(string path, string title)
    {
        imagePathCheck(path);
        string name = getTimeStamp(_timeStampStyle) + title + ".png";

        ScreenCapture.CaptureScreenshot(path + name);

        Debug.Log("Title: " + name);
        Debug.Log("Directory: " + path);
        yield break;
    }

    //ファイルパスの確認
    private void imagePathCheck(string path)
    {
        if (Directory.Exists(path))
        {
            Debug.Log("The path exists");
        }
        else
        {
            //パスが存在しなければフォルダを作成
            Directory.CreateDirectory(path);
            Debug.Log("CreateFolder: " + path);
        }
    }

    //タイムスタンプ
    private string getTimeStamp(TIME_STAMP type)
    {
        string time;

        //タイムスタンプの設定書き足せます
        switch (type)
        {
            case TIME_STAMP.MMDDHHMMSS:
                time = DateTime.Now.ToString("MMddHHmmss");
                return time;
            case TIME_STAMP.YYYYMMDDHHMMSS:
                time = DateTime.Now.ToString("yyyyMMddHHmmss");
                return time;
            default:
                time = DateTime.Now.ToString("yyyyMMddHHmmss");
                return time;
        }
    }

}
