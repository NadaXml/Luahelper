using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(BerizerMono)), CanEditMultipleObjects]
public class BerizerMonoEditor : Editor
{
    private BerizerMono tempSelf;
    public override void OnInspectorGUI()
    {
        tempSelf = target as BerizerMono;
        base.OnInspectorGUI();

        if( GUILayout.Button("生成") )
        {
            tempSelf.GenerateAllNoise();
        }
    }

    public void OnSceneGUI()
    {
        if(tempSelf == null )
        {
            return;
        }
        for (int i = 0; i < tempSelf.bs.Count; i++)
        {
            Vector2 temp = tempSelf.bs[i];
            Vector3 pp = new Vector3(temp.x, temp.y, 0);
            if (Handles.Button(pp, Quaternion.identity, tempSelf.bsSize, tempSelf.bsSize, Handles.RectangleHandleCap))
            {
                tempSelf.pList[i] = tempSelf.pList[i] > 0 ? 0: 1;
                Debug.Log("click");
            }
        }
    }
}

public class BerizerMono : MonoBehaviour
{
    BerizerLogic bLogic = new BerizerLogic();

    public int segment = 10;
    public int lastSegment = 0;
    private float perSegment = 0;

    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;

    public List<float> pList = new List<float>();

    [Range(0,1)]
    public float processT = 0;
    public float proceesTRadius = 10;

    public float noiseSource = 1.0f;
    public float noiseRadius = 0.1f;
    public float noiseLength = 10f;

    public int dx;

    public List<Vector2> bs = new List<Vector2>();
    public float bsSize = 5;

    private void OnDrawGizmosSelected()
    {
        bool dirty = false;
        if (lastSegment != segment)
        {
            dirty = true;
            bs.Clear();
            pList.Clear();
        }
        perSegment = 1f / segment;
        lastSegment = segment;

        RectTransform rc = GetComponent<RectTransform>();
       
        bLogic.p0 = rc.TransformPoint(p0);
        bLogic.p1 = rc.TransformPoint(p1);
        bLogic.p2 = rc.TransformPoint(p2);

        Vector3 last = bLogic.p0;
        for (int i = 0; i <= segment; i++)
        {
            bLogic.t = i * perSegment;
            Vector3 lb = bLogic.getRectV3();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(last, lb);
            last = lb;

        }

        float dxMax = rc.rect.width / dx;
        int dy = Mathf.CeilToInt(rc.rect.height / dxMax);
        float dyMax = dxMax;

        for (int i = 0; i < dx; i++)
        {
            for (int j = 0; j < dy; j++)
            {
                Vector2 tmp = new Vector2(i * dxMax  + 0.5f * dxMax - rc.rect.width * 0.5f , j * dyMax - rc.rect.height * 0.5f + 0.5f * dyMax);
                Vector2 tmp2 = generateNoisePoint(tmp, noiseSource);

                tmp2 = rc.TransformPoint(tmp2);
                int fl = i * dy + j;
                if (fl >= pList.Count)
                {
                    pList.Add(1f);
                }
                float ts = pList[fl];
                if (ts > 0)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(new Vector3(tmp2.x, tmp2.y, 0), proceesTRadius);
                }

                if (dirty)
                {
                    Vector2 rectCenter = new Vector2(i * dxMax + 0.5f * dxMax - rc.rect.width * 0.5f, j * dyMax + 0.5f * dyMax - rc.rect.height * 0.5f);
                    rectCenter = rc.TransformPoint(rectCenter);
                    bs.Add(rectCenter);
                }
            } 
        }

        Vector3 offset = new Vector3(-rc.rect.width * 0.5f, -rc.rect.height * 0.5f);
        //辅助线
        for (int i = 0;i < dx; i++)
        {
            Gizmos.color = Color.green;
            Vector3 bd = new Vector3(i * dyMax, 0, 0);
            bd = bd + offset;
            Vector3 ed = new Vector3(i * dyMax, rc.rect.height, 0);
            ed = ed + offset;
            bd = rc.TransformPoint(bd);
            ed = rc.TransformPoint(ed);
            Gizmos.DrawLine(bd, ed);
        }

        for (int i = 0; i < dy; i++)
        {
            Gizmos.color = Color.green;
            Vector3 bd1 = new Vector3(0, i * dyMax, 0);
            bd1 = bd1 + offset;
            Vector3 ed2 = new Vector3(rc.rect.width, i * dyMax, 0);
            ed2 = ed2 + offset;
            bd1 = rc.TransformPoint(bd1);
            ed2 = rc.TransformPoint(ed2);
            Gizmos.DrawLine(bd1, ed2);
        }

        bLogic.t = processT;
        Vector3 lb2 = bLogic.getRectV3();
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(lb2, proceesTRadius);
    }

    private Vector2 generateNoisePoint(Vector2 v2, float flash)
    {
        NoiseGen ns = new NoiseGen();
        //ns.flash = noiseSource;
        ns.flash = flash;
        ns._radius = noiseRadius;
        Vector2 delta = Vector2.zero;
        return v2 + (ns.getPoint(v2)+delta) * noiseLength;
    }

    public void GenerateAllNoise()
    {
        RectTransform rc = GetComponent<RectTransform>();
        float dxMax = rc.rect.width / dx;
        int dy = Mathf.CeilToInt(rc.rect.height / dxMax);
        float dyMax = dxMax;

        Vector2 pointSz = new Vector2(25, 25);

        int allCount = 0;
        for (int i = 0; i < dx; i++)
        {
            for (int j = 0; j < dy; j++)
            {
                int allIndex = i * dy + j;
                if (allIndex >= pList.Count)
                {
                    pList.Add(1f);
                }
                float ts = pList[allIndex];
                if ( ts == 0f)
                {
                    continue;
                }

                

                Vector2 tmp = new Vector2(i * dxMax - rc.rect.width * 0.5f + 0.5f * dxMax, j * dyMax - rc.rect.height * 0.5f + 0.5f * dyMax);
                Vector2 dd = generateNoisePoint(tmp, noiseSource);
                
                if (allCount >= transform.childCount )
                {
                    GameObject go = new GameObject();
                    go.name = allCount + "";
                    go.transform.SetParent(transform);
                    go.transform.SetAsLastSibling();
                    go.transform.localPosition = dd;
                    RectTransform selfRc = go.AddComponent<RectTransform>();
                    Image selfImg = go.AddComponent<Image>();
                    selfRc.sizeDelta = pointSz;
                }
                else
                {
                    Transform tsf = transform.GetChild(allCount);
                    tsf.localPosition = dd;
                    RectTransform selfRc = tsf.gameObject.GetComponent<RectTransform>();
                    Image selfImg = tsf.gameObject.GetComponent<Image>();
                    selfRc.sizeDelta = pointSz;
                }
                allCount++;
            }
        }

        for ( int i = transform.childCount-1; i >= allCount+1 ; i--)
        {
            Transform tsf = transform.GetChild(i);
            DestroyImmediate(tsf.gameObject);
        }
    }
}
