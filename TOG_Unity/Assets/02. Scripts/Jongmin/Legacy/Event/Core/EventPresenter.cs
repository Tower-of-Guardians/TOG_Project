using System.Collections;
using JxModule;
using JxDialogueBox;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

public class EventPresenter : MonoBehaviour
{
    [BigHeader("References")]
    [SerializeField, Required] private EventLoadManager eventLoadManager;
    [FormerlySerializedAs("eventView")] [SerializeField, Required] private EventUI eventUI;
    
    private AsyncOperationHandle<Sprite>? _backgroundHandle;
    private bool _eventFinished;
    private IObjectResolver _resolver;
    
    public EventNpc EventNpc { get; private set; }
    
    [Inject]
    private void Construct(IObjectResolver resolver)
    {
        _resolver =  resolver;
    }

    private void Awake()
    {
        eventLoadManager ??= FindFirstObjectByType<EventLoadManager>();
        if (eventLoadManager == null)
        {
            DebugExtension.LogColor($"EventPresenter: EventLoadSystem not found.", Color.red);
            enabled = false;
            return;
        }
        
        eventUI ??=  FindFirstObjectByType<EventUI>();
        if (eventUI == null)
        {
            DebugExtension.LogColor($"EventPresenter: EventView not found.", Color.red);
            enabled = false;
        }
        
        eventUI.BindCloseButton(HandleCloseButton);
    }
    
    private void OnEnable()
    {
        eventLoadManager.OnEventSetting = OnEventSetting;
        eventLoadManager.OnEventBegin = OnEventBegin;
        eventLoadManager.OnEventPlay = OnEventPlay;
        eventLoadManager.OnEventEnd = OnEventEnd;
    }
    
    private void OnDisable()
    {
        eventLoadManager.OnEventSetting = null;
        eventLoadManager.OnEventBegin = null;
        eventLoadManager.OnEventPlay = null;
        eventLoadManager.OnEventEnd = null;
    }
    
    private IEnumerator OnEventSetting(EventDataTableRow eventRow)
    {
        _eventFinished = false;
        
        ReleaseBackground();
        yield return LoadBackground(eventRow.backgroundKey);

        
        ConfigureEventNpc(eventRow.rowID);
        eventUI.Show();
    }

    private IEnumerator OnEventBegin(EventDataTableRow eventRow)
    {
        if (EventNpc == null)
        {
            yield break;
        }

        yield return EventNpc.HandleOnEventBegin();
        eventUI.ToggleCloseButton(true);
    }

    private IEnumerator OnEventPlay(EventDataTableRow eventRow)
    {
        if (EventNpc == null)
        {
            yield break;
        }

        EventNpc.SetInteractable(true);
        yield return new WaitUntil(() => _eventFinished);
        eventUI.ToggleCloseButton(false);
        EventNpc.SetInteractable(false);
    }

    private IEnumerator OnEventEnd(EventDataTableRow eventRow)
    {
        if (EventNpc == null)
        {
            yield break;
        }

        yield return EventNpc.HandleOnEventEnd();
        ReleaseEventNpc();
        eventUI.Hide();
    }

    private IEnumerator LoadBackground(string backgroundKey)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(backgroundKey);
        yield return handle;
        
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            DebugExtension.LogColor($"EventPresenter: Fail to load Background Image. Key: {backgroundKey}", Color.red);
            yield break;
        }
        
        _backgroundHandle = handle;

        eventUI?.SetBackground(handle.Result);
    }

    private void ReleaseBackground()
    {
        if (_backgroundHandle.HasValue)
        {
            Addressables.Release(_backgroundHandle.Value);
            _backgroundHandle = null;
        }
        
        eventUI?.ClearBackground();
    }
    
    public void ConfigureEventNpc(string eventID)
    {
        var npc = EventNPCFactory.Create(eventID);
        EventNpc = _resolver.Instantiate(npc, eventUI.PrefabRoot).GetComponent<EventNpc>();
    }

    public void ReleaseEventNpc()
    {
        if (EventNpc == null)
        {
            return;
        }
        
        Destroy(EventNpc.gameObject);
    }
    
    private void HandleCloseButton()
    {
        _eventFinished = true;
    }
}