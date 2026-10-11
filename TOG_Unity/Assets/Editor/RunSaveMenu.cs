using JxModule;
using Jongmin;
using UnityEditor;
using UnityEngine;

public static class RunSaveMenu
{
    private const string Root = "JxModule/Save Data/";

    [MenuItem(Root + "Delete Slot 1", false, 100)]
    private static void DeleteSlot1()
    {
        DeleteSlot(0);
    }

    [MenuItem(Root + "Delete Slot 2", false, 101)]
    private static void DeleteSlot2()
    {
        DeleteSlot(1);
    }

    [MenuItem(Root + "Delete Slot 3", false, 102)]
    private static void DeleteSlot3()
    {
        DeleteSlot(2);
    }

    [MenuItem(Root + "Delete All Save Data", false, 120)]
    private static void DeleteAll()
    {
        bool confirmed = EditorUtility.DisplayDialog("Delete All Save Data", "세 개의 저장 슬롯을 모두 삭제합니다. 이 작업은 되돌릴 수 없습니다.", "Delete All", "Cancel");
        if (!confirmed)
        {
            return;
        }
        SaveSlotSystem.RemoveAll();
        RunSaveService.SelectSlot(0);
        Debug.Log("모든 세이브 데이터를 삭제했습니다.");
    }

    [MenuItem(Root + "Delete Slot 1", true)]
    [MenuItem(Root + "Delete Slot 2", true)]
    [MenuItem(Root + "Delete Slot 3", true)]
    [MenuItem(Root + "Delete All Save Data", true)]
    private static bool ValidateDelete()
    {
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void DeleteSlot(int slot)
    {
        string label = $"Slot {slot + 1}";
        bool confirmed = EditorUtility.DisplayDialog($"Delete {label}", $"{label}의 세이브 데이터를 삭제합니다. 이 작업은 되돌릴 수 없습니다.", "Delete", "Cancel");
        if (!confirmed)
        {
            return;
        }
        SaveSlotSystem.Remove(slot);
        if (RunSaveService.SelectedSlot == slot)
        {
            RunSaveService.SelectSlot(slot);
        }
        Debug.Log($"{label} 세이브 데이터를 삭제했습니다.");
    }
}
