using System.Collections;
using JxModule;
using JxModule.DataTable;
using UnityEngine;

public delegate IEnumerator EventSettingDelegate(EventDataTableRow eventRow);
public delegate IEnumerator EventBeginDelegate(EventDataTableRow eventRow);
public delegate IEnumerator EventingDelegate(EventDataTableRow eventRow);
public delegate IEnumerator EventEndDelegate(EventDataTableRow eventRow);

public class EventLoadManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private string testEventID; 
    
    private DataTable _eventTable;

    public EventSettingDelegate OnEventSetting;
    public EventBeginDelegate OnEventBegin;
    public EventingDelegate OnEventPlay;
    public EventEndDelegate OnEventEnd;

    private void Awake()
    {
        _eventTable = DataTableManager.FindTable<EventDataTableRow>("DT_Event");
    }

    [Button("이벤트 로드 테스트")]
    public void LoadEvent()
    {
        StartCoroutine(LoadEventRoutine(testEventID));
    }
    
    public void LoadEvent(string eventID)
    {
        StartCoroutine(LoadEventRoutine(eventID));
    }

    private IEnumerator LoadEventRoutine(string eventID)
    {
        var targetEventRow = _eventTable.Find<EventDataTableRow>(eventID);
        if (targetEventRow == null)
        {
            DebugExtension.LogColor($"EventLoadSystem: Event not found. eventID: {eventID}", Color.red);
            yield break;
        }

        if (OnEventSetting != null)
        {
            yield return OnEventSetting(targetEventRow);
        }

        if (OnEventBegin != null)
        {
            yield return OnEventBegin(targetEventRow);
        }

        if (OnEventPlay != null)
        {
            yield return OnEventPlay(targetEventRow);
        }

        if (OnEventEnd != null)
        {
            yield return OnEventEnd(targetEventRow);
        }
    }
}