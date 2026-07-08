using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AreaEventUI_Blacksmith : MonoBehaviour
{
    [SerializeField] private GameObject _obPanel;




    public void Start()
    {

    }

    public void Open()
    {
        _obPanel.SetActive(true);
    }
}
