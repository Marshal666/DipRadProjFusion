using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Utils 
{

    // Source: https://gist.github.com/maxattack/4c7b4de00f5c1b95a33b
    public static Quaternion SmoothDampQuaternion(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
    {
        if (Time.deltaTime < Mathf.Epsilon) return rot;
        // account for double-cover
        var Dot = Quaternion.Dot(rot, target);
        var Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        var Result = new Vector4(
            Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;

        // ensure deriv is tangent
        var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
        deriv.x -= derivError.x;
        deriv.y -= derivError.y;
        deriv.z -= derivError.z;
        deriv.w -= derivError.w;

        return new Quaternion(Result.x, Result.y, Result.z, Result.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NormalizeAngle360(float a)
    {
        a %= 360f;
        if (a < 0f)
            a += 360f;
        return a;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="a">Angle to be clamped from [0 .. 360></param>
    /// <param name="min">Number from [0 .. 360></param>
    /// <param name="max">Number from [0 .. 360></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClampAngleLPositive(float a, float min, float max)
    {
        //if (a >= 0)
        //    return Mathf.Clamp(a, min, max);
        //else
        //    return Mathf.Clamp(a + 360f, min, max);
        min = NormalizeAngle360(min);
        max = NormalizeAngle360(max);
        if (min == max)
            return a;
        float ret;
        if (a < 0)
            a += 360f;
        if(max >= min)
        {
            if (a == Mathf.Clamp(a, min, max))
                ret = a;
            else
            {
                //Debug.Log($"d(min, a) = {Mathf.DeltaAngle(a, min)}, d(a, max) = {Mathf.DeltaAngle(a, max)}");
                if (Mathf.Abs(Mathf.DeltaAngle(a, min)) < Mathf.Abs(Mathf.DeltaAngle(a, max)))
                    ret = min;
                else
                    ret = max;
            }
        }
        else
        {
            max += 360f;
            if(a == Mathf.Clamp(a, min, max))
            {
                ret = a;
            } else if(a <= max - 360f)
            {
                ret = a;
            } else
            {
                //Debug.Log($"d(min, a) = {Mathf.DeltaAngle(a, min)}, d(a, max) = {Mathf.DeltaAngle(a, max)}");
                if (Mathf.Abs(Mathf.DeltaAngle(a, min)) < Mathf.Abs(Mathf.DeltaAngle(a, max)))
                    ret = min;
                else
                    ret = max;
            }

        }
        //Debug.Log($"a: {a}, min: {min}, max: {max}, ret: {NormalizeAngle360(ret)}");
        return NormalizeAngle360(ret);
    }

    public static bool HasNaN(this Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }

    public static float MapInterval(float val, float min1, float max1, float min2, float max2) 
        => (val - min1) * (max2 - min2) / (max1 - min1) + min1;

    public static void DestroyObject(UnityEngine.Object o)
    {
        Action<UnityEngine.Object> d = UnityEngine.Object.Destroy;
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            d = UnityEngine.Object.DestroyImmediate;
        }
#endif
        d(o);
    }

}
