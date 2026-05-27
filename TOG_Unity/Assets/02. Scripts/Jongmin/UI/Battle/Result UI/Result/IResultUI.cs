using UnityEngine;

public interface IResultUI : IOpenableUI
{
    void Construct(ResultPresenter resultPresenter);
    void Initialize();

    void ShowCloseButton();
}