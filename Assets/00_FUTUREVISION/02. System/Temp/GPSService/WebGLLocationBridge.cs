using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLLocationBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void   FV_StartSensors();
    [DllImport("__Internal")] private static extern void   FV_StopSensors();
    [DllImport("__Internal")] private static extern void   FV_RequestOrientationPermission();

    [DllImport("__Internal")] private static extern double FV_GetLat();
    [DllImport("__Internal")] private static extern double FV_GetLon();
    [DllImport("__Internal")] private static extern double FV_GetAcc();
    [DllImport("__Internal")] private static extern double FV_GetHeading();
    [DllImport("__Internal")] private static extern int    FV_HasGeo();
    [DllImport("__Internal")] private static extern int    FV_HasHeading();
#endif

    public static void StartSensors()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FV_StartSensors();
#endif
    }

    public static void StopSensors()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FV_StopSensors();
#endif
    }

    /// <summary>
    /// iOS 13+에서 '사용자 제스처 내부'에서 호출해야 함 (버튼 onClick 등)
    /// </summary>
    public static void RequestOrientationPermission()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        FV_RequestOrientationPermission();
#endif
    }

    public static bool HasGeo()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return FV_HasGeo() == 1;
#else
        return false;
#endif
    }

    public static bool HasHeading()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return FV_HasHeading() == 1;
#else
        return false;
#endif
    }

    public static double GetLatitude()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return FV_GetLat();
#else
        return 0d;
#endif
    }

    public static double GetLongitude()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return FV_GetLon();
#else
        return 0d;
#endif
    }

    public static double GetAccuracy()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return FV_GetAcc();
#else
        return 0d;
#endif
    }

    public static float GetHeading()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // JS는 number(double) → C# float로 캐스팅
        return (float)FV_GetHeading();
#else
        return 0f;
#endif
    }
}
