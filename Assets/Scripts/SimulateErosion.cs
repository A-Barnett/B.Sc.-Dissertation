using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SimulateErosion : MonoBehaviour
{
    [SerializeField] private int rainCount;
    [SerializeField] private int passes;
    [SerializeField] private float frictionRate;
    [SerializeField] private bool displayPaths;
    [SerializeField] private bool erosionEnabled;
    [SerializeField] private bool layeringEnabled;
    [SerializeField] private bool timelapseEnabled;
    [SerializeField] private float changeAmount;
    [SerializeField] private float evaporationRate;
    [SerializeField] private float volume;
    [SerializeField] private float upwardsVelocityMulti;
    [SerializeField] private float velocityExponent;
    [SerializeField] private int radius;
    [SerializeField] private int maxDepth;
    [SerializeField] private List<int> radii;
    [SerializeField] private List<float> volumes;
    [SerializeField] private List<float> evaporationRates;
    [SerializeField] private List<float> frictionRates;
    [SerializeField] private List<float> changeAmounts;
    [SerializeField] private TerrainController terrainController;
    [SerializeField] private GameObject sphere;
    [SerializeField] private TextMeshProUGUI passText;
    [SerializeField] private TextMeshProUGUI iterationText;
    [SerializeField] private TextMeshProUGUI dropsText;
    [SerializeField] private Toggle layerToggle;
    [SerializeField] private Toggle timelapseToggle;
    Vector3[] neighbors = new Vector3[8];
    private Mesh mesh;
    private int size;
    private int rowLegnth;
    private Vector3[] vertices;
    private List<int> neighbourPoints;
    private int depth;
    private List<GameObject> dots;
    private int dropsSimulated;
    private class Drop
    {
        public Vector3 velocity { get; set; }
        public float sediment { get; set; }
        public float volume { get; set; }
        public Vector3 currentPos { get; set; }
        public int currentIndex { get; set; }
        //public List<GameObject> dots { get; set; }

        public Drop(Vector3 Velocity, float Sediment, float Volume, Vector3 CurrentPos, int CurrentIndex, List<GameObject> Dots)
        {
            velocity = Velocity;
            sediment = Sediment;
            volume = Volume;
            currentPos = CurrentPos;
            currentIndex = CurrentIndex;
            //dots = Dots;
        }
    }
    void Start()
    {
        neighbourPoints = new List<int>(Mathf.RoundToInt(Mathf.Pow((radius*2)+1,2)));
        dots = new List<GameObject>();
        iterationText.text = "0/" + radii.Count;
        passText.text = "0/" + passes;
    }

    public void OnClick()
    {
        timelapseEnabled = timelapseToggle.isOn;
        mesh = terrainController.mesh;
        vertices = mesh.vertices;
        size = mesh.vertexCount;
        rowLegnth = Mathf.RoundToInt(Mathf.Sqrt(size));
        if (layerToggle.isOn)
        {
            StartCoroutine(Erode());
        }
        else
        {
            for (int k = 0; k < passes; k++)
            {
                for (int i = 0; i < rainCount; i++)
                {
                    DrawPoint();
                    depth = 0;
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }
    }
    private IEnumerator Erode()
    {
        double startTime2 = Time.realtimeSinceStartup;
        for (int j = 0; j < radii.Count; j++)
        {
            iterationText.text = (j + 1) + "/" + radii.Count;
            radius = radii[j];
            volume = volumes[j];
            evaporationRate = evaporationRates[j];
            frictionRate = frictionRates[j];
            changeAmount = changeAmounts[j];
            for (int k = 0; k < passes; k++)
            {
                dropsSimulated += rainCount;
                passText.text = (k + 1) + "/" + passes;
                dropsText.text = dropsSimulated.ToString();

                for (int i = 0; i < rainCount; i++)
                {
                    DrawPoint();
                    depth = 0;
                }

                if (timelapseEnabled)
                {
                    mesh.vertices = vertices;
                    mesh.RecalculateBounds();
                    mesh.RecalculateNormals();
                    yield return new WaitForSecondsRealtime(0.001f);
                }
            }
            yield return new WaitForSecondsRealtime(0.001f);
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        double endTime2 = Time.realtimeSinceStartup;
        Debug.Log("Time Taken: " + (endTime2 - startTime2) * 1000 + "ms");
        Debug.Log("All passes complete");
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            mesh = terrainController.mesh;
            vertices = mesh.vertices;
            size = mesh.vertexCount;
            rowLegnth = Mathf.RoundToInt(Mathf.Sqrt(size));
            if (layeringEnabled)
            {
                StartCoroutine(Erode());
            }
            else
            {
                for (int i = 0; i < rainCount; i++)
                {
                    DrawPoint();
                    depth = 0;
                }
                mesh.vertices = vertices;
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
            }
           
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            ChangeBack();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveDots();
        }
    }

    private void ChangeBack()
    {
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    private void DrawPoint()
    {

        int index = Random.Range(0, size);
        Vector3 pos = vertices[index];
        Drop drop = new Drop(new Vector3(0,0,0),0,volume,pos,index,new List<GameObject>());
        //Debug.Log("Volume: " + drop.volume);
        if (displayPaths)
        {
            pos.y += 7;
            GameObject dot =Instantiate(sphere, pos, Quaternion.identity);
            dot.transform.GetComponent<Renderer>().material.color = new Color(0,1,0);
            dots.Add(dot);
        }
        NextPoint(drop);
    }
    

    private void NextPoint(Drop drop)
    {
        depth++;
        int index = drop.currentIndex;
        if (depth > maxDepth || index >= vertices.Length - rowLegnth - rowLegnth - 1 || index % rowLegnth == 0 || index % rowLegnth == rowLegnth - 1 )
        {
            return;
        }

        Vector3 selectedPoint = new Vector3(0, Mathf.Infinity, 0);
        int selectedIndex = -1;
        try
        {
            neighbors[0] = vertices[index - 1 + rowLegnth];
            neighbors[1] = vertices[index + rowLegnth];
            neighbors[2] = vertices[index + 1 + rowLegnth];
            neighbors[3] = vertices[index - 1];
            neighbors[4] = vertices[index + 1];
            neighbors[5] = vertices[index - 1 - rowLegnth];
            neighbors[6] = vertices[index - rowLegnth];
            neighbors[7] = vertices[index + 1 - rowLegnth];
            for (int i = 0; i < neighbors.Length; i++)
            {
                float adjustedVal = AccountForVelocity(neighbors[i].y, i, drop);
                if (adjustedVal < selectedPoint.y)
                {
                    selectedPoint = neighbors[i];
                    selectedIndex = i;
                }
            }
        }
        catch (Exception)
        {
            return;
        }
        float finalAdjustedVal = AccountForVelocity(selectedPoint.y, selectedIndex, drop);
        if (finalAdjustedVal < drop.currentPos.y && drop.volume > 0.001f)
        {
            Vector3 pos = selectedPoint;
            float change = drop.currentPos.y - pos.y;
            float adjustedChange = drop.currentPos.y - finalAdjustedVal;
            drop.currentPos = pos;
            pos.y += 7;
            if (erosionEnabled)
            {
                float velocity = Mathf.Abs(drop.velocity.x) + Mathf.Abs(drop.velocity.z);
                float maxSediment = drop.volume * velocity;
                float sedimentDeposit = 0;
                if (drop.sediment < maxSediment)
                {
                    // Eroding terrain
                    sedimentDeposit = Mathf.Clamp(change * changeAmount * velocity, 0, maxSediment - drop.sediment);
                }
                else
                {
                    // Depositing onto terrain
                    sedimentDeposit = -drop.sediment / Mathf.Clamp(velocity, 1, 1000);

                }
                if (drop.sediment < 0) { drop.sediment = 0; }
                drop.sediment += sedimentDeposit;
                drop.volume *= evaporationRate;
                List<int> indexes = NeighbourPoints(index);
                float sediment = sedimentDeposit / indexes.Count;
                foreach (int i in indexes)
                {
                    try
                    {
                        vertices[i].y -= sediment;
                    }
                    catch (Exception){}
                }
            }

            if (displayPaths)
            {
                GameObject dot = Instantiate(sphere, pos, Quaternion.identity);
                dot.transform.GetComponent<Renderer>().material.color =
                    new Color(adjustedChange / 5f, 0, 1 - adjustedChange / 1.5f, 1);
                dots.Add(dot);
            }

            int newIndex = Mathf.RoundToInt(selectedPoint.x + (selectedPoint.z * rowLegnth));
            drop.currentIndex = newIndex;
            SetVelocity(selectedIndex, change, drop);
            NextPoint(drop);
        }
        else
        {
            // drop is finished deposit last of sediment
            List<int> indexes = NeighbourPoints(index);
            float sediment = drop.sediment / indexes.Count;
            foreach (int i in indexes)
            {
                try
                {
                    vertices[i].y += sediment;
                }
                catch (Exception)
                {
                }
                        
            }
        }

    }

    private float AccountForVelocity(float yVal, int index, Drop drop)
    {
        float adjustedVal = 0;
        float adjustedVelocityX = drop.velocity.x * velocityExponent;
        float adjustedVelocityZ = drop.velocity.z * velocityExponent;
        switch (index)
        {
            case 0:
                adjustedVal = yVal  + adjustedVelocityX - adjustedVelocityZ;
                break;
            case 1:
                adjustedVal = yVal - adjustedVelocityZ;
                break;
            case 2:
                adjustedVal = yVal - adjustedVelocityX - adjustedVelocityZ;
                break;
            case 3:
                adjustedVal = yVal + adjustedVelocityX;
                break;
            case 4:
                adjustedVal = yVal - adjustedVelocityX;
                break;
            case 5:
                adjustedVal = yVal  + adjustedVelocityX + adjustedVelocityZ;
                break;
            case 6:
                adjustedVal = yVal + adjustedVelocityZ;
                break;
            case 7:
                adjustedVal = yVal - adjustedVelocityX + adjustedVelocityZ;
                break;
        }
        return adjustedVal;
       
    }

    private void  SetVelocity(int selectedIndex, float change, Drop drop)
    {
        if (selectedIndex == -1)
        {
            return;
        }
        if (change < 0)
        {
            change *= upwardsVelocityMulti;
        }
        switch (selectedIndex)
        {
            case 0:
                drop.velocity += new Vector3(-change/2, 0, change/2);
                break;
            case 1:
                drop.velocity += new Vector3(0, 0, change);
                break;
            case 2:
                drop.velocity += new Vector3(change/2, 0, change/2);
                break;
            case 3:
                drop.velocity += new Vector3(-change, 0,0);
                break;
            case 4:
                drop.velocity += new Vector3(change, 0, 0);
                break;
            case 5:
                drop.velocity += new Vector3(-change/2, 0, -change/2);
                break;
            case 6:
                drop.velocity += new Vector3(0, 0, -change);
                break;
            case 7:
                drop.velocity += new Vector3(change/2, 0, -change/2);
                break;
        }
        drop.velocity *= frictionRate;
    }

    private List<int> NeighbourPoints(int pointIndex)
    {
        
        neighbourPoints.Clear();
        for (int x = 0; x < (radius * 2) + 1; x++)
        {
            for (int y = 0; y < (radius * 2) + 1; y++)
            {
                neighbourPoints.Add(pointIndex-(radius-y)+(rowLegnth*(radius-x)));
            }
        }
        return neighbourPoints;
    }

    private void RemoveDots()
    {
        foreach (GameObject dot in dots)
        {
            Destroy(dot);
        }
    }
    
    
}