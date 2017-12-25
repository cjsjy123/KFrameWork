using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using KFrameWork;
using KUtils;

[SingleTon]
public class GameCameraCtr  {

    public static GameCameraCtr mIns;
	
    public static Matrix4x4 getMvpMatrix(Transform tr, Camera c,bool rendertoTex =false)
    {
        Matrix4x4 m = tr.localToWorldMatrix;
        Matrix4x4 v = c.worldToCameraMatrix;
        Matrix4x4 p = c.projectionMatrix;

        return GL.GetGPUProjectionMatrix( m*v*p,rendertoTex);
    }

    public static Matrix4x4 GetOrthographicMatrix(Camera target)
    {
        return Matrix4x4.Ortho(-target.orthographicSize * target.aspect, target.orthographicSize * target.aspect, -target.orthographicSize, target.orthographicSize, target.nearClipPlane, target.farClipPlane);
    }

    public static Matrix4x4 GetPerspectiveMatrix(Camera target)
    {
        return Matrix4x4.Perspective(target.fieldOfView,target.aspect, target.nearClipPlane, target.farClipPlane);
    }

    /// <summary>
    /// Use this to calculate the camera space position of objects or to provide custom camera's location that is not based on the transform.

    ///Note that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis.
    /// </summary>
    /// <param name="tr"></param>
    /// <returns></returns>
    public static Matrix4x4 getCameraToWorldMatrix(Transform tr)
    {
        var matrix = Matrix4x4.TRS(tr.position, tr.rotation, tr.lossyScale);//or tr.localscale?
        matrix.m20 *= -1;
        matrix.m21 *= -1;
        matrix.m22 *= -1;
        matrix.m23 *= -1;
        return matrix;
    }
    /// <summary>
    /// Use this to calculate the camera space position of objects or to provide custom camera's location that is not based on the transform.
    ///Note that camera space matches OpenGL convention: camera's forward is the negative Z axis. This is different from Unity's convention, where forward is the positive Z axis.
    /// </summary>
    /// <param name="tr"></param>
    /// <returns></returns>
    public static Matrix4x4 getWorldToCameraMatrix(Transform tr)
    {
        var matrix = Matrix4x4.Inverse(Matrix4x4.TRS(tr.position, tr.rotation, tr.lossyScale));//or tr.localscale?
        matrix.m20 *= -1;
        matrix.m21 *= -1;
        matrix.m22 *= -1;
        matrix.m23 *= -1;
        return matrix;
    }

    public void LookatWithDistance(Camera camera ,Transform trans, float distance)
    {
        if (camera != null && trans != null)
        {
            Transform tr = camera.transform;
            Vector3 pos = trans.position - distance * tr.forward;
            tr.position = pos;

            camera.transform.LookAt(trans);
            // MathUtility.BillbordLookAt(tr,c);
        }
    }

    /// <summary>
    /// 3D组件坐标系转换到UI摄像机坐标系，返回世界坐标系
    /// </summary>
    /// <returns>The dto position U.</returns>
    /// <param name="target">Target.</param>
    public Vector3 Convert3DtoUIWorldPos(Camera camera, Canvas canvas, Component target,RectTransform rectTransform)
    {
        Vector3 old = target.transform.position;

        Vector3 screenPoint =  camera.WorldToScreenPoint(old);

        Vector3 worldpos ;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform,screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out worldpos);
        //LogMgr.Log(ret+" oldscreenPoint=> "+screenPoint +" oldWorld=> "+ old +" newworld= "+ worldpos+" nowscreen "+this.uicamera.WorldToScreenPoint(worldpos));

        return worldpos;
    }

    public Vector2 Convert3DtoUILocalPos(Camera camera,Canvas canvas, Component target,RectTransform rectTransform)
    {
        Vector3 old = target.transform.position;

        Vector2 screenPoint =  RectTransformUtility.WorldToScreenPoint(camera, old);

        Vector2 localpos  ;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,screenPoint, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null: canvas.worldCamera, out localpos);

        return localpos;
    }


    /// <summary>
    /// 3d世界坐标转换到2d ui坐标，返回世界坐标系
    /// </summary>
    /// <param name="target">ui组件</param>
    /// <param name="worldPos">3d世界坐标</param>
    /// <returns></returns>
    public Vector3 ConvertUItoPos3D(Camera camera, Canvas canvas, GameObject target, Vector3 worldPos)
    {
        Vector3 targetScreenPosition = camera.WorldToScreenPoint(worldPos);
        RectTransform rect = target.GetComponentInChildren<RectTransform>();
        Vector3 followPosition;

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, targetScreenPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out followPosition);
        
        return followPosition;
    }

    public static bool ScreenPointToWorldPointInRectangle(Quaternion r,Vector3 p, Vector3 screenPoint, Camera cam, out Vector3 worldPoint)
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
