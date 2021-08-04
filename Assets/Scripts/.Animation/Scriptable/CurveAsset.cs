using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurveAsset : ScriptableObject
{
    [System.Serializable]
    public struct CurveStruct
    {
        public string Name;
        public AnimationCurve Curve;
    }

    public CurveStruct[] Curves = new CurveStruct[0];
    
    public AnimationCurve FindCurve(string name)
    {
        for (int i = Curves.Length - 1; i >= 0; --i)
        {
            if (Curves[i].Name == name)
            {
                return Curves[i].Curve;
            }
        }
        return null;
    }

#if UNITY_EDITOR
    
    public void DeleteCurve(string name)
    {
        int removed = 0;
        for (int i = 0; i < Curves.Length; ++i)
        { 
            if (Curves[i].Name == name)
            {
                for (int j = i + 1; j < Curves.Length; ++j, ++i)
                {
                    Curves[i] = Curves[j];
                }
                EditorUtility.SetDirty(this);
                ++removed;
            }
        }
        System.Array.Resize<CurveStruct>(ref Curves, Curves.Length - removed);
    }
    
    public AnimationCurve AddCurve(string name)
    {
        AnimationCurve curve = FindCurve(name);
        if (curve == null)
        {
            System.Array.Resize<CurveStruct>(ref Curves, Curves.Length + 1);
            int lastIndex = Curves.Length - 1;
            Curves[lastIndex].Name = name;
            Curves[lastIndex].Curve = new AnimationCurve();
            EditorUtility.SetDirty(this);
            curve = Curves[lastIndex].Curve;
            OnValidate();
        }
        return curve;
    }

    void OnValidate()
    {
        for (int i = 0; i < Curves.Length; ++i)
        {
            if (Curves[i].Name.Length > 4)
            {
                Curves[i].Name = Curves[i].Name.Substring(0, 4);
                EditorUtility.SetDirty(this);
            }
        }
    }

#endif

}
