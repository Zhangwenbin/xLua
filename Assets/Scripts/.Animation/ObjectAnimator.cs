using UnityEngine;
using System.Collections;

public static partial class SRPG_Extensions
{
    public static float Evaluate(this ObjectAnimator.CurveType curve, float t)
    {
        switch (curve)
        {
            case ObjectAnimator.CurveType.EaseIn:
                return 1.0f - Mathf.Cos(t * Mathf.PI * 0.5f);
            case ObjectAnimator.CurveType.EaseOut:
                return Mathf.Cos((1.0f - t) * Mathf.PI * 0.5f);
            case ObjectAnimator.CurveType.EaseInOut:
                return (1.0f - Mathf.Cos(t * Mathf.PI)) * 0.5f;
        }
        return t;
    }

    public static float ToSpan(this CameraInterpSpeed speed)
    {
        if (speed == CameraInterpSpeed.Immediate)
        {
            return 0;
        }

        return (int)speed * 0.25f + 0.5f;
    }
}

public enum CameraInterpSpeed
{
    Immediate = -1,
    Fast = 0,
    Normal = 1,
    Slow = 2,
}

public class ObjectAnimator : MonoBehaviour {
    
    public enum CurveType
    {
        Linear, // 
        EaseIn, // 
        EaseOut, // 
        EaseInOut // 
    }
    CurveType mCurveType;

    AnimationCurve mCurve;

    Vector3 mStartPos;
    Vector3 mEndPos;
    Quaternion mStartRot;
    Quaternion mEndRot;
    Vector3 mStartScale;
    Vector3 mEndScale;
    float mTime;
    float mDuration;
    bool mPositionSet;
    bool mRotationSet;
    bool mScaleSet;

    // 
    public bool isMoving
    {
        get { return enabled; }
    }

    // 
    public float NormalizedTime
    {
        get
        {
            return mDuration > 0.0f ? Mathf.Clamp01(mTime / mDuration) : 0.0f;
        }
    }

    void Update () {
        if (mTime < mDuration)
        {
            mTime = Mathf.Min(mTime + Time.deltaTime, mDuration);

            float t = mTime / mDuration;
            float f;

            if (mCurve != null)
            {
                f = mCurve.Evaluate(t);
            }
            else
            {
                f = mCurveType.Evaluate(t);
            }

            Transform tr = transform;

            if (mPositionSet)
            {
                tr.position = Vector3.Lerp(mStartPos, mEndPos, f);
            }

            if (mRotationSet)
            {
                tr.rotation = Quaternion.Slerp(mStartRot, mEndRot, f);
            }

            if (mScaleSet)
            {
                tr.localScale = Vector3.Lerp(mStartScale, mEndScale, f);
            }

        }
        else
        {
            enabled = false;
        }
    }

    // 
    public void ScaleTo(Vector3 scale, float duration, CurveType curveType)
    {
        mPositionSet = false;
        mRotationSet = false;
        mScaleSet = true;
        mTime = 0;

        if (duration > 0.0f)
        {
            mStartScale = transform.localScale;
            mEndScale = scale;
            mCurve = null;
            mCurveType = curveType;
            mDuration = duration;
        }
        else
        {
            transform.localScale = scale;
            mDuration = 0;
        }

        enabled = true;
    }

    // 
    public void AnimateTo(Vector3 position, Quaternion rotation, float duration, AnimationCurve curve)
    {
        AnimateTo(position, rotation, duration, CurveType.Linear);
        mCurve = curve;
    }

    // 
    public void AnimateTo(Vector3 position, Quaternion rotation, float duration, CurveType curveType)
    {
        mPositionSet = true;
        mRotationSet = true;
        mScaleSet = false;
        mTime = 0;

        if (duration > 0.0f)
        {
            mStartPos = transform.position;
            mStartRot = transform.rotation;
            mEndPos = position;
            mEndRot = rotation;
            mCurve = null;
            mCurveType = curveType;
            mDuration = duration;
        }
        else
        {
            transform.position = position;
            transform.rotation = rotation;
            mDuration = 0;
        }

        enabled = true;
    }

    public static ObjectAnimator Get(Component component)
    {
        return Get(component.gameObject);
    }

    public static ObjectAnimator Get(GameObject obj)
    {
        ObjectAnimator objAnimator = obj.GetComponent<ObjectAnimator>();
        if (objAnimator == null)
        {
            objAnimator = obj.AddComponent<ObjectAnimator>();
        }
        return objAnimator;
    }

}
