using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            Physics.Raycast(ray, out hit);

            if (hit.collider == null)
            {
                var go = Instantiate(pointPrefab, pointParent.transform);
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
        
    }
}
