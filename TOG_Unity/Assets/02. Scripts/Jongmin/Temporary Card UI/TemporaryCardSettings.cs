using UnityEngine;
using System;
using DG.Tweening;

[Serializable]
public class TemporaryCardSettings
{
    public float Duration = 0.5f;

    public bool UseJump = false;
    public float JumpPower = 0f;
    public Ease MoveEase = Ease.InQuad;

    public bool UseScale = false;
    public Vector3 Scale = Vector3.zero;
    public Ease ScaleEase = Ease.OutBack;

    public bool UseRotation = false;
    public Vector3 TargetEuler;
    public RotateMode RotateMode = RotateMode.FastBeyond360;
    public Ease RotateEase = Ease.OutQuad;

    public bool UseOpacity = false;
    public float Opacity = 1f;
    public Ease OpacityEase = Ease.InQuad;

    public bool ForceStartScale = false;
    public Vector3 StartScale = Vector3.one;

    public bool ForceStartRotation = false;
    
    public bool ForceStartOpacity = false;
    public float StartOpacity; 
}