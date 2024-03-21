using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PointClick : MonoBehaviour
{
    [SerializeField]
    private GameObject pointPrefab;

    [SerializeField]
    private GameObject pointParent;

    [SerializeField]
    private LineRenderer linePrefab;

    [SerializeField]
    private Vector2[] points;

    [SerializeField]
    private InputField filenameInput;

    [SerializeField]
    private bool isDirty = false;

    [SerializeField]
    private bool updateByClick = false;

    private LineRenderer line;

    void Start()
    {
        var go = Instantiate(linePrefab);
        line = go.GetComponent<LineRenderer>();

        /*
        {
            var b = Vector3.zero;
            var a = new Vector3(1, 0);

            Debug.Log(area2(b, a, new Vector3(1, 1)));
            Debug.Log(area2(b, a, new Vector3(2, 2)));
            Debug.Log(area2(b, a, new Vector3(1, 2)));
            Debug.Log(area2(b, a, new Vector3(-1, 1)));
            Debug.Log(area2(b, a, new Vector3(-1, -1)));
            Debug.Log(area2(b, a, new Vector3(1, -1)));
        }

        Debug.Log("xxx");*/

        Vector2[] vector2s = new Vector2[]
        {
            new Vector2 (0,0),
            new Vector2 (0,1),
            new Vector2 (2,1),
            new Vector2 (3,1),
        };

        vector2s = new Vector2[]
        {
            new Vector2(0,1),
            new Vector2(1,4),
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(-1, 4)
        };

        vector2s = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 5),
            new Vector2(2, 5),
            new Vector2(3, 4),
            new Vector2(1, 1),
            new Vector2(2, 2),
            new Vector2(6, 2.7f)
        };


        vector2s = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 1),
            new Vector2(1, 2),
            new Vector2(1, 3),
            new Vector2(1, 4),
            new Vector2(1, 5),
            new Vector2(1, 6),
        };
        vector2s = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(2, 1),
            new Vector2(3, 4),
            new Vector2(4, 5),
        };

        vector2s = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(2, 1),
            new Vector2(3, 2),
        };

        ReDrawPoints(vector2s);
        //UpdateLine(vector2s);
    }

    void Update()
    {
        if (isDirty)
        {
            UpdatePoints();
            isDirty = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x < 0 || Input.mousePosition.y < 0 ||
                Input.mousePosition.x > Screen.width || Input.mousePosition.y > Screen.height)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Physics.Raycast(ray, out hit);

            if (hit.collider == null)
            {
                var go = Instantiate(pointPrefab, pointParent.transform);
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                pos.x = (float)Math.Round(pos.x, 1);
                pos.y = (float)Math.Round(pos.y, 1);
                pos.z = 0;
                go.transform.localPosition = pos;

                isDirty = true;
            }
            else
            {
                if (hit.collider.name.StartsWith(pointPrefab.name))
                {
                    Destroy(hit.collider.gameObject);
                    isDirty = true;
                }
            }
        }
    }

    private void ReDrawPoints(Vector2[] points)
    {
        foreach (var point in points)
        {
            var go = Instantiate(pointPrefab, pointParent.transform);
            Vector3 pos = point;
            pos.z = 0;
            go.transform.localPosition = pos;
        }

        isDirty = true;
    }

    private void UpdatePoints()
    {
        List<Vector3> list3 = new List<Vector3>();
        List<Vector2> list = new List<Vector2>();

        for (int i = 0; i < pointParent.transform.childCount; i++)
        {
            var pos = pointParent.transform.GetChild(i).position;

            list.Add(pos);
            list3.Add(pos);
        }

        points = list.ToArray();
        if (updateByClick)
        {
            line.positionCount = list3.Count;
            line.SetPositions(list3.ToArray());
        }
        else
        {
            UpdateLine(points.ToArray());
        }
    }

    public void SaveList()
    {
        UpdatePoints();

        List<Point2D> list = new List<Point2D>();
        foreach (var point in points)
        {
            list.Add(new Point2D { x = point.x, y = point.y });
        }

        var json = Newtonsoft.Json.JsonConvert.SerializeObject(list.ToArray(), formatting: Newtonsoft.Json.Formatting.Indented);

        Debug.Log(Application.persistentDataPath);

        File.WriteAllText(Path.Join(Application.persistentDataPath, filenameInput.text + ".json"), json);
    }

    private void UpdateLine(Vector2[] list)
    {
        if (list.Length <= 2)
        {
            line.positionCount = 0;
            return;
        }

        Vector3[] extremePoints = Vector2ToVector3(GetExtremePointByGraham(list));

        line.positionCount = extremePoints.Length;
        line.SetPositions(extremePoints);
        line.loop = true;
    }

    private Vector2[] GetExtremePoint(Vector2[] list)
    {
        List<Vector2> result = new List<Vector2>(list);

        for (int i = 0; i < list.Length - 2; i++)
        {
            for (int j = i + 1; j < list.Length - 1; j++)
            {
                for (int k = j + 1; k < list.Length; k++)
                {
                    Vector2 pi = list[i];
                    Vector2 pj = list[j];
                    Vector2 pk = list[k];

                    for (int s = result.Count - 1; s >= 0; s--)
                    {
                        Vector2 ps = result[s];

                        if (ps.Equals(pi) || ps.Equals(pj) || ps.Equals(pk))
                        {
                            continue;
                        }

                        var triangle = new Vector2[] { pi, pj, pk };

                        if (isPointInTriangle(ps, triangle))
                        {
                            result.RemoveAt(s);
                        }
                    }
                }
            }
        }

        return SortExtremePoints(result.ToArray());
    }

    private Vector2[] GetExtremePointByExtremeEdge(Vector2[] list)
    {
        List<Vector2> result = new List<Vector2>();

        for (int i = 0; i < list.Length - 1; i++)
        {
            for (int j = i + 1; j < list.Length; j++)
            {
                var p = list[j];
                var q = list[i];

                var hasLeft = false;
                var hasRight = false;

                foreach (var s in list)
                {
                    if (p.Equals(s) || q.Equals(s))
                    {
                        continue;
                    }

                    var rs = toLeftTest(p, q, s);
                    if (!hasLeft && rs)
                    {
                        hasLeft = rs;
                    }
                    if (!hasRight && !rs)
                    {
                        hasRight = !rs;
                    }

                    if (hasLeft && hasRight)
                    {
                        break;
                    }
                }

                if (hasLeft == !hasRight)
                {
                    bool hasQ = false;
                    bool hasP = false;

                    foreach (var r in result)
                    {
                        if (r.Equals(q))
                        {
                            hasQ = true;
                        }
                        if (r.Equals(p))
                        {
                            hasP = true;
                        }
                    }

                    if (!hasP)
                    {
                        result.Add(p);
                    }
                    if (!hasQ)
                    {
                        result.Add(q);
                    }
                }
            }
        }

        return SortExtremePoints(result.ToArray());
    }

    private Vector2[] SortExtremePoints(Vector2[] list)
    {
        List<Vector2> rs = new List<Vector2>();

        List<Vector2> array = new List<Vector2>(list);

        var startPoint = array[0];
        rs.Add(startPoint);
        array.RemoveAt(0);

        int selectA = 0;
        float angle = float.MaxValue;

        for (int i = 0; i < array.Count - 1; i++)
        {
            for (int j = i + 1; j < array.Count; j++)
            {
                var va = array[i];
                var vb = array[j];

                float c = Vector2.Dot(va - startPoint, vb - startPoint);
                c = c / ((va - startPoint).magnitude * (vb - startPoint).magnitude);

                if (c < angle)
                {
                    angle = c;
                    selectA = i;
                }
            }
        }

        var currentPoint = array[selectA];
        rs.Add(currentPoint);
        array.RemoveAt(selectA);

        while (array.Count > 0)
        {
            int pop = 0;

            var v1 = startPoint - currentPoint;
            float min = float.MaxValue;

            for (int i = 0; i < array.Count; i++)
            {
                var vp = array[i] - currentPoint;

                var vc = Vector2.Dot(vp, v1);
                vc = vc / (vp.magnitude * v1.magnitude);

                if (vc < min)
                {
                    min = vc;
                    pop = i;
                }
            }

            startPoint = currentPoint;
            currentPoint = array[pop];
            rs.Add(currentPoint);
            array.RemoveAt(pop);
        }

        return rs.ToArray();
    }

    private bool isPointInTriangle(Vector2 s, Vector2[] triangle)
    {
        bool a = toLeftTest(triangle[0], triangle[1], s);
        bool b = toLeftTest(triangle[1], triangle[2], s);
        bool c = toLeftTest(triangle[2], triangle[0], s);

        return (a == b && b == c);
    }

    private bool toLeftTest(Vector2 p1, Vector2 p2, Vector2 s)
    {
        return area2(p1, p2, s) > 0;
    }

    private float area2(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        var v = p1.x * p2.y - p1.y * p2.x
            + p2.x * p3.y - p2.y * p3.x
            + p3.x * p1.y - p3.y * p1.x;

        return v;
    }

    private Vector3[] Vector2ToVector3(Vector2[] list)
    {
        Vector3[] vector3s = new Vector3[list.Length];

        for (int i = 0; i < list.Length; i++)
        {
            vector3s[i] = list[i];
        }

        return vector3s;
    }

    // aka LTL Lowest-Then-Leftmost
    private int GetLowestThenLeftPointIndex(Vector2[] points)
    {
        int ltl = 0;

        for (int i = 0; i < points.Length; i++)
        {
            if ((points[i].y < points[ltl].y) ||
                ((points[i].y - points[ltl].y > -float.Epsilon) && (points[i].y - points[ltl].y < float.Epsilon)
                && (points[i].x < points[ltl].x)))
            {
                ltl = i;
            }
        }

        return ltl;
    }

    private Vector2[] GetExtremePointByJarvisMatch(Vector2[] points)
    {
        List<Vector2> list = new List<Vector2>(points);
        List<Vector2> result = new List<Vector2>();

        Vector2 currentPoint, ltlPoint;
        {
            int start = GetLowestThenLeftPointIndex(points);

            currentPoint = points[start];
            ltlPoint = currentPoint;
        }

        do
        {
            result.Add(currentPoint);
            Vector2 endPoint = list[0];

            int selectIndex = 0;
            for (int i = 1; i < list.Count; i++)
            {
                //if (endPoint.Equals(currentPoint) || IsCountClockWise(currentPoint, endPoint, list[i]) < 0) // or use this line
                if (endPoint.Equals(currentPoint) || toLeftTest(currentPoint, list[i], endPoint))
                {
                    endPoint = list[i];
                    selectIndex = i;
                }
            }

            currentPoint = endPoint;
            list.RemoveAt(selectIndex);
        }
        while (!currentPoint.Equals(ltlPoint));

        return result.ToArray();
    }

    private float IsCountClockWise(Vector2 p, Vector2 q, Vector2 s)
    {
        return (q.x - p.x) * (s.y - q.y) - (q.y - p.y) * (s.x - q.x);
    }

    private Vector2[] GetExtremePointByGraham(Vector2[] points)
    {
        int startIndex = GetLowestThenLeftPointIndex(points);

        Vector2 startPoint = points[startIndex];
        Vector2 rigthVector = Vector2.right;

        List<Vector2> list = new List<Vector2>(points);
        list.RemoveAt(startIndex);

        // cos
        list = list.OrderBy(p => Vector2.Dot((p - startPoint).normalized, rigthVector)).ToList();

        Stack<Vector2> stack = new Stack<Vector2>();
        stack.Push(startPoint);

        Stack<Vector2> tt = new Stack<Vector2>();

        foreach (Vector2 p in list)
        {
            tt.Push(p);
        }

        while (tt.Count > 0)
        {
            if (stack.Count < 2)
            {
                stack.Push(tt.Pop());
                continue;
            }

            var s = tt.Pop();

            var p2 = stack.Pop();
            var p1 = stack.Pop();

            var area2V = area2(p1, p2, s);

            if (area2V > 0)
            {
                stack.Push(p1);
                stack.Push(p2);
                stack.Push(s);
                print("add: " + s);
            }
            else if (area2V < 0)
            {
                print("drop: " + p2);
                print("add: " + s);
                tt.Push(s);
                stack.Push(p1);
            }
            else
            {
                // need to test more point in stack make a line????
                stack.Push(p1);
                if ((p1 - p2).sqrMagnitude > (p1 - s).sqrMagnitude)
                {
                    stack.Push(s);
                    stack.Push(p2);
                    print("add: " + s);
                }
                else
                {
                    stack.Push(p2);
                    stack.Push(s);
                    print("add: " + s);
                }
            }
        }

        return stack.ToArray();
    }
}
