using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

[SingleTon]
public class GameCameraCtr
{

    private Camera _UICamera;
    public Camera cameraUI
    {
        get
        {
            if (_UICamera == null)
            {
                GameObject uiObject = GameObject.FindWithTag("CameraUITag");
                if (uiObject != null)
                {
                    _UICamera = uiObject.GetComponent<Camera>();
                    _UICamera.depth = 3;
                }
                else
                {
                    LogMgr.Log("Missing uicamrea use MainCamera");
                    _UICamera = Camera.main;
                }
            }
            return _UICamera;
        }
    }

    private Camera _camera3D;
    public Camera camera3D
    {
        get
        {
            if (_camera3D == null)
            {
                GameObject uiObject = GameObject.FindWithTag("Camera3DTag");
                if (uiObject != null)
                {
                    _camera3D = uiObject.GetComponent<Camera>();
                    _camera3D.depth = 1;
                }
                else
                {
                    LogMgr.Log("Missing 3dcamrea use MainCamera");
                    _UICamera = Camera.main;
                }
            }
            return _camera3D;
        }
    }

    public static GameCameraCtr mIns;

    public Matrix4x4 getMvpMatrix(Transform tr, Camera c, bool rendertoTex = false)
    {
        Matrix4x4 m = tr.localToWorldMatrix;
        Matrix4x4 v = c.worldToCameraMatrix;
        Matrix4x4 p = c.projectionMatrix;

        return GL.GetGPUProjectionMatrix(m * v * p, rendertoTex);
    }


    /// <summary>
    /// 3D组件坐标系转换到UI摄像机坐标系，返回世界坐标系
    /// </summary>
    /// <returns>The dto position U.</returns>
    /// <param name="target">Target.</param>
    public Vector3 Convert3DtoUIWorldPos(Component target, RectTransform rectTransform)
    {
        Vector3 old = target.transform.position;

        Vector3 screenPoint = GameCameraCtr.mIns.camera3D.WorldToScreenPoint(old);

        Vector3 worldpos;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, GameCameraCtr.mIns.cameraUI, out worldpos);
        //LogMgr.Log(ret+" oldscreenPoint=> "+screenPoint +" oldWorld=> "+ old +" newworld= "+ worldpos+" nowscreen "+this.uicamera.WorldToScreenPoint(worldpos));

        return worldpos;
    }

    public Vector2 Convert3DtoUILocalPos(Component target, RectTransform rectTransform)
    {
        Vector3 old = target.transform.position;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GameCameraCtr.mIns.camera3D, old);

        Vector2 localpos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, GameCameraCtr.mIns.cameraUI, out localpos);

        return localpos;
    }


    /// <summary>
    /// ui组件坐标系转换到3D摄像机坐标系，返回世界坐标系
    /// </summary>
    /// <returns>The U ito pos3 d.</returns>
    /// <param name="target">Target.</param>
    public Vector3 ConvertUItoPos3D(Component target)
    {
        Vector3 old = target.transform.position;
        Vector3 screenpos = GameCameraCtr.mIns.cameraUI.WorldToScreenPoint(old);
        Vector3 world3dPos = GameCameraCtr.mIns.camera3D.ScreenToWorldPoint(screenpos);
        return world3dPos;
    }

    /// <summary>
    /// 3d世界坐标转换到2d ui坐标，返回世界坐标系
    /// </summary>
    /// <param name="target">ui组件</param>
    /// <param name="worldPos">3d世界坐标</param>
    /// <returns></returns>
    public Vector3 ConvertUItoPos3D(GameObject target, Vector3 worldPos)
    {
        Vector3 targetScreenPosition = GameCameraCtr.mIns.camera3D.WorldToScreenPoint(worldPos);
        RectTransform rect = target.GetComponentInChildren<RectTransform>();
        Vector3 followPosition;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, targetScreenPosition, GameCameraCtr.mIns.cameraUI, out followPosition);

        return followPosition;
    }

    public static bool ScreenPointToWorldPointInRectangle(Quaternion r, Vector3 p, Vector3 screenPoint, Camera cam, out Vector3 worldPoint)
    {
        worldPoint = Vector2.zero;
        Ray ray = cam.ScreenPointToRay(screenPoint);
        Plane plane = new Plane(r * Vector3.back, p);
        float distance;
        if (!plane.Raycast(ray, out distance))
        {
            return false;
        }
        worldPoint = ray.GetPoint(distance);
        return true;
    }

    public static bool ScreenPointToWorldPointInRectangle(Transform rect, Vector2 screenPoint, Camera cam, out Vector3 worldPoint)
    {
        worldPoint = Vector2.zero;
        Ray ray = RectTransformUtility.ScreenPointToRay(cam, screenPoint);
        Plane plane = new Plane(rect.rotation * Vector3.back, rect.position);
        float distance;
        if (!plane.Raycast(ray, out distance))
        {
            return false;
        }
        worldPoint = ray.GetPoint(distance);
        return true;
    }

    public static bool ScreenPointToLocalPointInRectangle(Transform rect, Vector3 screenPoint, Camera cam, out Vector2 localPoint)
    {
        localPoint = Vector2.zero;
        Vector3 position;
        if (ScreenPointToWorldPointInRectangle(rect, screenPoint, cam, out position))
        {
            localPoint = rect.InverseTransformPoint(position);
            return true;
        }
        return false;
    }
}
