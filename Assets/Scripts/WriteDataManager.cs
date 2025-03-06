using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using System;

public class WriteDataManager : MonoBehaviour
{
    [SerializeField] private List<GameObject> targetObjects;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float recordInterval = 0.1f;

    private Dictionary<GameObject, List<string>> objectPositionData;
    private bool isRecording = false;
    private string userName = "default_user"; // ユーザー名
    private string folderPath;
    private float lastRecordTime;

    void Start()
    {
        objectPositionData = new Dictionary<GameObject, List<string>>();
    }

    void Update()
    {
        // Meta Quest 3 の B ボタンで録画の開始・停止をトグル
        if (OVRInput.GetDown(OVRInput.Button.Two)) // Bボタン
        {
            isRecording = !isRecording;

            if (isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecording();
            }
        }

        if (isRecording && Time.time - lastRecordTime >= recordInterval)
        {
            RecordObjectPositions(Time.time);
            lastRecordTime = Time.time;
        }
    }

    void StartRecording()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (!objectPositionData.ContainsKey(obj))
            {
                objectPositionData[obj] = new List<string>();
                objectPositionData[obj].Add("Time,Position.x,Position.y,Position.z");
            }
        }

        if (_text != null)
        {
            _text.text = "Recording started...";
        }
    }

    void StopRecording()
    {
        string inputText = _textMeshProUGUI.text;
        userName = string.IsNullOrEmpty(inputText) ? "default_user" : inputText;

        // フォルダを生成または確認
        folderPath = CreateUniqueFolder(userName);

        SaveAllObjectDataToCSV();
        objectPositionData.Clear();

        if (_text != null)
        {
            _text.text = "Recording stopped and saved.";
        }
    }

    void RecordObjectPositions(float currentTime)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                Vector3 position = obj.transform.position;
                string line = $"{currentTime:F2},{position.x},{position.y},{position.z}";
                objectPositionData[obj].Add(line);
            }
        }
    }

    void SaveAllObjectDataToCSV()
    {
        foreach (var kvp in objectPositionData)
        {
            GameObject obj = kvp.Key;
            List<string> csvData = kvp.Value;

            string fileName = GenerateFileName(userName, obj.name);
            string filePath = Path.Combine(folderPath, fileName);

            File.WriteAllText(filePath, string.Join("\n", csvData));
            Debug.Log($"CSV saved for {obj.name} at: {filePath}");
        }
    }

    string CreateUniqueFolder(string baseFolderName)
    {
        string basePath = Application.persistentDataPath;
        string folderName = baseFolderName;
        string fullFolderPath = Path.Combine(basePath, folderName);
        int count = 1;

        // フォルダ名の重複を避ける
        while (Directory.Exists(fullFolderPath))
        {
            folderName = $"{baseFolderName}({count})";
            fullFolderPath = Path.Combine(basePath, folderName);
            count++;
        }

        // フォルダを作成
        Directory.CreateDirectory(fullFolderPath);
        Debug.Log($"Folder created: {fullFolderPath}");

        return fullFolderPath;
    }

    string GenerateFileName(string userName, string objectName)
    {
        // 日付をフォーマットする
        string date = DateTime.Now.ToString("yyyy-MM-dd");

        // ファイル名を生成
        return $"{userName}_{objectName}_{date}.csv";
    }
}
