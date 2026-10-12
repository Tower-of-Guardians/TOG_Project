using System;

public interface IRandomAreaEventHost
{
    bool IsDialoguePlaying { get; }
    bool TryInteract(string npcID, Action specialAction);
    bool HasRunnableEvent(string npcID);
    bool TryStartBattle();
    void Complete();
    void Fail();
}
