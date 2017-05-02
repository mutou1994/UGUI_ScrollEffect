using UnityEngine;
using System.Collections;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class CenterOnChild : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    public Action<GameObject> onCenterCallBack;
    public Action<GameObject> onCenterFinshed;
    public GameObject centeredObject { get; private set; }
    public CenterMode centerMode = CenterMode.centerOnChild;
    public float speed = 10;
    ScrollRect scroll;
    GridLayoutGroup grid;
    Transform content;
    void Awake()
    {
        scroll = GetComponentInParent<ScrollRect>();
        content = scroll.content;
        grid = content.GetComponent<GridLayoutGroup>();
    }
    void Start()
    {
        //考虑到Grid排列会先打乱Item的坐标,这回导致寻找最靠近中心的Item与计算偏移量出错，所以等排列完成再居中
        Invoke("reCenter", 0.05f);
    }
    Vector3 CalPickingPoint()
    {
        Vector3 pickingPoint = Vector3.zero;
        //裁剪区域四角坐标
        Vector2 halfSize = (scroll.transform as RectTransform).rect.size / 2;
        Vector2[] Conners = new Vector2[4];
        Conners[0] = new Vector2(-halfSize.x, halfSize.y);
        Conners[1] = new Vector2(halfSize.x, halfSize.y);
        Conners[2] = new Vector2(halfSize.x, -halfSize.y);
        Conners[3] = new Vector2(-halfSize.x, -halfSize.y);

        if (centerMode == CenterMode.centerOnChild)
        {
            for (int i = 0; i < 4; i++)
                Conners[i] = scroll.transform.TransformPoint(Conners[i]);
            pickingPoint = (Conners[0] + Conners[2]) * 0.5f;
        }
        else
        {
            RectTransform item = content.GetChild(0) as RectTransform;
            Vector2 half_itemSize = item.rect.size * 0.5f;
            half_itemSize.y *= -1;
            pickingPoint = scroll.transform.TransformPoint(Conners[0] + half_itemSize);
        //    Debug.Log(pickingPoint);
        }
        return pickingPoint;
    }
    public void reCenter()
    {
        Vector3 pickingPoint = CalPickingPoint();
        float minDist = float.MaxValue;
        Transform closest = null;
        for (int i = 0, Imax = content.childCount; i < Imax; i++)
        {
            Transform child = content.GetChild(i);
            if (!child.gameObject.activeInHierarchy) continue;
            float dist = Vector2.SqrMagnitude(child.position - pickingPoint);

            if (dist < minDist)
            {

                minDist = dist;
                closest = child;
            }

        }
        CenterOn(closest, pickingPoint);
    }
    void CenterOn(Transform target, Vector3 centerPos)
    {
        if (target != null && scroll != null)
        {
            centeredObject = target.gameObject;
            Vector3 cp = content.parent.InverseTransformPoint(target.position);
            Vector3 cc = content.parent.InverseTransformPoint(centerPos);
            Vector3 localOffset = cc - cp;
            if (!scroll.horizontal) localOffset.x = 0;
            if (!scroll.vertical) localOffset.y = 0;
            localOffset.z = 0;
            targetPos = content.localPosition + localOffset;
            centering = true;
            if (onCenterCallBack != null) onCenterCallBack(target.gameObject);
        }
    }
    public void CenterOn(Transform target)
    {
        CenterOn(target, CalPickingPoint());
    }
    Vector3 targetPos;
    bool centering = false;
    void Update()
    {
        if (centering)
        {
            Vector2 v = content.localPosition;
            content.localPosition = Vector2.Lerp(content.localPosition, targetPos, speed * Time.deltaTime);
            if (Vector2.Distance(content.localPosition, targetPos) < 0.01f)
            {
                content.localPosition = targetPos;
                centering = false;
                if (onCenterFinshed != null) onCenterFinshed(centeredObject);
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        scroll.StopMovement();
        reCenter();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        centering = false;
        targetPos = Vector3.zero;
    }
    public enum CenterMode
    {
        centerOnChild,
        centerOnItem,
    }
}

