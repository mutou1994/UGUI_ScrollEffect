using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScrollRectEffect : MonoBehaviour
{

    float left_alpha = 0.6f;
    float right_alpha = 0.4f;
    float middle_alpha = 1f;

    float left_height = 150;
    float right_height = 100;
    float middle_height = -120;

    float left_scale = 0.6f;
    float right_scale = 0.3f;
    float middle_scale = 1f;

    ScrollRect scroll;
    List<Transform> contents;
    Vector2 centerPoint;
    Vector2 viewPort_size;

    void Awake()
    {
        scroll = transform.GetComponentInParent<ScrollRect>();
        CacheContents();
        CalCenterPoint();
        scroll.onValueChanged.AddListener(onScroll);
    }

    void CacheContents()
    {
        if (contents == null)
            contents = new List<Transform>();
        contents.Clear();
        for (int i = 0, Imax = transform.childCount; i < Imax; i++)
        {
            Transform child = transform.GetChild(i).Find("content");
            contents.Add(child);
        }
    }

    void CalCenterPoint()
    {
        viewPort_size = (scroll.transform as RectTransform).rect.size;

        Vector3 pickingPoint = Vector3.zero;
        //裁剪区域四角坐标
        Vector2 halfSize = viewPort_size / 2;
        Vector2[] Conners = new Vector2[4];
        Conners[0] = new Vector2(-halfSize.x, halfSize.y);
        Conners[1] = new Vector2(halfSize.x, halfSize.y);
        Conners[2] = new Vector2(halfSize.x, -halfSize.y);
        Conners[3] = new Vector2(-halfSize.x, -halfSize.y);

        centerPoint = (Conners[0] + Conners[2]) * 0.5f;
    }

    void onScroll(Vector2 vec)
    {
        Adjust();
    }

    void Adjust()
    {
        CalCenterPoint();
        for (int i = 0, Imax = contents.Count; i < Imax; i++)
        {
            Vector3 pos = scroll.transform.InverseTransformPoint(contents[i].position);
            if (!scroll.horizontal) pos.x = centerPoint.x;
            if (!scroll.vertical) pos.y = centerPoint.y;
            pos.z = 0;
            float dis = Vector3.Distance(centerPoint, pos);
            int direct = pos.x < centerPoint.x ? -1 : 1;

            AdjustAlpha(i, dis, direct < 0 ? left_alpha : right_alpha);
            AdjustScale(i, dis, direct < 0 ? left_scale : right_scale);
            AdjustPosition(i, dis, direct < 0 ? left_height : right_height);
        }
    }

    void AdjustScale(int index, float dis, float minVal)
    {
        float rate = 1 - (dis / (viewPort_size.x / 2f));
        float scale = Mathf.Lerp(minVal, middle_scale, rate);
        Vector3 vec = Vector3.zero;
        vec.x = vec.y = vec.z = scale;
        contents[index].localScale = vec;
    }

    void AdjustAlpha(int index, float dis, float minVal)
    {
        float rate = 1 - (dis / (viewPort_size.x / 2f));
        float alpha = Mathf.Lerp(minVal, middle_alpha, rate);
        var canvas = contents[index].GetComponent<CanvasGroup>();
        canvas.alpha = alpha;
    }

    void AdjustPosition(int index, float dis, float minVal)
    {
        float height = Mathf.Lerp(minVal, middle_height, 1 - (dis / (viewPort_size.x / 2f)));
        Vector3 pos = contents[index].localPosition;
        pos.y = height;
        contents[index].localPosition = pos;

    }
}

