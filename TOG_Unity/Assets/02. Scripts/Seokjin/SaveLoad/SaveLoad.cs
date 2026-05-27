using NUnit.Framework;
using System.Collections.Generic;
using System.IO; // File I/O를 위해 필요
using System.Text; // 인코딩을 위해 필요 (선택 사항)
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    // 저장 데이터의 인스턴스
    private SaveData currentData = new SaveData();

    // 저장 경로 (플랫폼에 따라 안전한 경로)
    private string saveFilePath;
    private readonly string SAVE_FILENAME = "savefile.json";

    void Start()
    {
        // Application.persistentDataPath는 운영체제에 따라 안전하게 읽고 쓸 수 있는 경로를 제공합니다.
        // ex: PC에서는 AppData/LocalLow/... , 모바일에서는 문서 폴더 등
        saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
        Debug.Log("저장 파일 경로: " + saveFilePath);
    }

    // --- 저장 (Save) 함수 ---
    [ContextMenu("Save Game Data")] // 인스펙터 우클릭 메뉴로 쉽게 테스트 가능
    public void SaveGame()
    {
        // 1. 저장할 데이터(변수)를 currentData 인스턴스에 업데이트합니다.
        // 예시: 플레이어의 현재 골드 값을 업데이트했다고 가정

        /*for (int i = 0; i < 10; i++)
        {
            currentData.userDeckData.Add(new CardData(i,i.ToString(),i.ToString()));
        }*/

        // 2. SaveData 인스턴스를 JSON 문자열로 변환합니다.
        // ToJson(data, prettyPrint) : prettyPrint가 true면 가독성 있게 줄 바꿈이 들어갑니다.
        string json = JsonUtility.ToJson(currentData, true);

        // 3. 파일에 JSON 문자열을 씁니다.
        try
        {
            File.WriteAllText(saveFilePath, json, Encoding.UTF8);
            Debug.Log("게임 저장 완료! JSON:\n" + json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("파일 저장 실패: " + e.Message);
        }
    }

    // --- 불러오기 (Load) 함수 ---
    [ContextMenu("Load Game Data")]
    public void LoadGame()
    {
        // 1. 저장 파일이 존재하는지 확인합니다.
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("저장 파일이 존재하지 않아 로드할 수 없습니다. 새 게임 시작.");
            // 저장 파일이 없으면 기본값으로 currentData를 유지합니다.
            return;
        }

        // 2. 파일에서 JSON 문자열을 읽어옵니다.
        try
        {
            string json = File.ReadAllText(saveFilePath, Encoding.UTF8);

            // 3. JSON 문자열을 SaveData 인스턴스로 역직렬화(변환)합니다.
            currentData = JsonUtility.FromJson<SaveData>(json);

            // 4. 로드된 데이터를 사용합니다.
            Debug.Log(currentData.userDeckData);
        }
        catch (System.Exception e)
        {
            Debug.LogError("파일 로드 실패: " + e.Message);
        }
    }
    // 테스트용으로 현재 데이터에 접근하는 간단한 함수
    public void SetSaveData(List<string> data)
    {
        currentData.userDeckData = data;
    }
    // 테스트용으로 현재 데이터에 접근하는 간단한 함수
    public SaveData GetCurrentData()
    {
        return currentData;
    }
}