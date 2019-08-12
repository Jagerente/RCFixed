﻿using AnimationOrTween;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Active Animation"), RequireComponent(typeof(Animation))]
public class ActiveAnimation : IgnoreTimeScale
{
    public string callWhenFinished;
    public GameObject eventReceiver;
    private Animation mAnim;
    private AnimationOrTween.Direction mDisableDirection;
    private AnimationOrTween.Direction mLastDirection;
    private bool mNotify;
    public OnFinished onFinished;

    private void Play(string clipName, Direction playDirection)
    {
        if (this.mAnim != null)
        {
            base.enabled = true;
            this.mAnim.enabled = false;
            if (playDirection == Direction.Toggle)
            {
                playDirection = ((this.mLastDirection == Direction.Forward) ? Direction.Reverse : Direction.Forward);
            }
            bool flag = string.IsNullOrEmpty(clipName);
            if (flag)
            {
                if (!this.mAnim.isPlaying)
                {
                    this.mAnim.Play();
                }
            }
            else if (!this.mAnim.IsPlaying(clipName))
            {
                this.mAnim.Play(clipName);
            }
            foreach (AnimationState animationState in this.mAnim)
            {
                if (string.IsNullOrEmpty(clipName) || animationState.name == clipName)
                {
                    float num = Mathf.Abs(animationState.speed);
                    animationState.speed = num * (float)playDirection;
                    if (playDirection == Direction.Reverse && animationState.time == 0f)
                    {
                        animationState.time = animationState.length;
                    }
                    else if (playDirection == Direction.Forward && animationState.time == animationState.length)
                    {
                        animationState.time = 0f;
                    }
                }
            }
            this.mLastDirection = playDirection;
            this.mNotify = true;
            this.mAnim.Sample();
        }
    }

    public static ActiveAnimation Play(Animation anim, AnimationOrTween.Direction playDirection)
    {
        return Play(anim, null, playDirection, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
    }

    public static ActiveAnimation Play(Animation anim, string clipName, AnimationOrTween.Direction playDirection)
    {
        return Play(anim, clipName, playDirection, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
    }

    public static ActiveAnimation Play(Animation anim, string clipName, AnimationOrTween.Direction playDirection, EnableCondition enableBeforePlay, DisableCondition disableCondition)
    {
        if (!NGUITools.GetActive(anim.gameObject))
        {
            if (enableBeforePlay != EnableCondition.EnableThenPlay)
            {
                return null;
            }
            NGUITools.SetActive(anim.gameObject, true);
            UIPanel[] componentsInChildren = anim.gameObject.GetComponentsInChildren<UIPanel>();
            int index = 0;
            int length = componentsInChildren.Length;
            while (index < length)
            {
                componentsInChildren[index].Refresh();
                index++;
            }
        }
        ActiveAnimation component = anim.GetComponent<ActiveAnimation>();
        if (component == null)
        {
            component = anim.gameObject.AddComponent<ActiveAnimation>();
        }
        component.mAnim = anim;
        component.mDisableDirection = (AnimationOrTween.Direction) disableCondition;
        component.eventReceiver = null;
        component.callWhenFinished = null;
        component.onFinished = null;
        component.Play(clipName, playDirection);
        return component;
    }

    public void Reset()
    {
        if (this.mAnim != null)
        {
            foreach (AnimationState animationState in this.mAnim)
            {
                if (this.mLastDirection == Direction.Reverse)
                {
                    animationState.time = animationState.length;
                }
                else if (this.mLastDirection == Direction.Forward)
                {
                    animationState.time = 0f;
                }
            }
        }
    }

    
    private void Update()
    {
        float num = base.UpdateRealTimeDelta();
        if (num == 0f)
        {
            return;
        }
        if (this.mAnim != null)
        {
            bool flag = false;
            foreach (AnimationState animationState in this.mAnim)
            {
                if (this.mAnim.IsPlaying(animationState.name))
                {
                    float num2 = animationState.speed * num;
                    animationState.time += num2;
                    if (num2 < 0f)
                    {
                        if (animationState.time > 0f)
                        {
                            flag = true;
                        }
                        else
                        {
                            animationState.time = 0f;
                        }
                    }
                    else if (animationState.time < animationState.length)
                    {
                        flag = true;
                    }
                    else
                    {
                        animationState.time = animationState.length;
                    }
                }
            }
            this.mAnim.Sample();
            if (flag)
            {
                return;
            }
            base.enabled = false;
            if (this.mNotify)
            {
                this.mNotify = false;
                if (this.onFinished != null)
                {
                    this.onFinished(this);
                }
                if (this.eventReceiver != null && !string.IsNullOrEmpty(this.callWhenFinished))
                {
                    this.eventReceiver.SendMessage(this.callWhenFinished, this, SendMessageOptions.DontRequireReceiver);
                }
                if (this.mDisableDirection != Direction.Toggle && this.mLastDirection == this.mDisableDirection)
                {
                    NGUITools.SetActive(base.gameObject, false);
                }
            }
        }
        else
        {
            base.enabled = false;
        }
    }

    public bool isPlaying
    {
        get
        {
            if (this.mAnim == null)
            {
                return false;
            }
            foreach (AnimationState animationState in this.mAnim)
            {
                if (this.mAnim.IsPlaying(animationState.name))
                {
                    if (this.mLastDirection == Direction.Forward)
                    {
                        if (animationState.time < animationState.length)
                        {
                            bool result = true;
                            return result;
                        }
                    }
                    else
                    {
                        if (this.mLastDirection != Direction.Reverse)
                        {
                            bool result = true;
                            return result;
                        }
                        if (animationState.time > 0f)
                        {
                            bool result = true;
                            return result;
                        }
                    }
                }
            }
            return false;
        }
    }


    public delegate void OnFinished(ActiveAnimation anim);
}

