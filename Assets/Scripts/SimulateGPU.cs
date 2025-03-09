using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using Toggle = UnityEngine.UI.Toggle;

public class SimulateGPU : MonoBehaviour
{
    public ComputeShader computeShader;
    private Vector3[] vertices;
    private Mesh mesh;
    private bool firstRunDone;
    private int dropCount;
    [SerializeField] private bool timeLapse;
    [SerializeField] private int repeats;
    [SerializeField] private int meshSize;
    [SerializeField] private int radius;
    [SerializeField] private float frictionRate;
    [SerializeField] private float changeAmount;
    [SerializeField] private float evaporationRate;
    [SerializeField] private float volume;
    [SerializeField] private List<int> radii;
    [SerializeField] private List<float> volumes;
    [SerializeField] private List<float> evaporationRates;
    [SerializeField] private List<float> frictionRates;
    [SerializeField] private List<float> changeAmounts;
    [SerializeField] private TextMeshProUGUI passText;
    [SerializeField] private TextMeshProUGUI iterationText;
    [SerializeField] private TextMeshProUGUI dropsText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Toggle layerToggle;
    [SerializeField] private Toggle timelapseToggle;
    [SerializeField] private TerrainController terrainController;
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine( ComputeSimulate(false));
            Debug.Log("Erosion Simulated");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            StartCoroutine(ComputeSimulate(true));
            Debug.Log("Layered Erosion Simulated");
        }
    }

    public void OnClick()
    {
        timeLapse = timelapseToggle.isOn;
        if (layerToggle.isOn)
        {
            StartCoroutine(ComputeSimulate(true));
            Debug.Log("Layered Erosion Simulated");
        }
        else
        {
            StartCoroutine( ComputeSimulate(false));
            Debug.Log("Erosion Simulated");
        }

    }

    private IEnumerator ComputeSimulate(bool layeringEnabled)
    {
        double startTime = Time.realtimeSinceStartup;
        typeText.text = "GPU";
        dropCount = 0;
        mesh = terrainController.mesh;
        vertices = mesh.vertices;

        // Create compute buffers
        ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, 12); // 12 bytes for Vector3
        verticesBuffer.SetData(vertices);
        
        // Set the kernel
        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetBuffer(kernelHandle, "verticesBuffer", verticesBuffer);

        if (layeringEnabled)
        {
            for (int x = 0; x < radii.Count; x++)
            {
                iterationText.text = (x+1) + "/" + radii.Count;
                computeShader.SetInt("radius", radii[x]);
                computeShader.SetFloat("frictionRate", frictionRates[x]);
                computeShader.SetFloat("changeAmount", changeAmounts[x]);
                computeShader.SetFloat("evaporationRate", evaporationRates[x]);
                computeShader.SetFloat("volume", volumes[x]);
                computeShader.SetInt("size",meshSize*meshSize);
                for (int i = 0; i < repeats; i++)
                {
                    passText.text =(i+1)+"/"+repeats;
                    dropCount += 32*32*32;
                    dropsText.text = dropCount.ToString();
                    float randomStart = Random.Range(0,100000);
                    computeShader.SetFloat("randomStart", randomStart); 
                    // Dispatch the compute shader
                    computeShader.Dispatch(kernelHandle, 32, 1, 1);
                    if (timeLapse)
                    {
                        Vector3[] layerVerts = new Vector3[vertices.Length];
                        verticesBuffer.GetData(layerVerts);
                        mesh.vertices = layerVerts;
                        mesh.RecalculateBounds();
                        mesh.RecalculateNormals();
                        yield return new WaitForSecondsRealtime(0.001f);
                    }
                }
            }
        }
        else
        {
            computeShader.SetInt("radius", radius);
            computeShader.SetFloat("frictionRate", frictionRate);
            computeShader.SetFloat("changeAmount", changeAmount);
            computeShader.SetFloat("evaporationRate", evaporationRate);
            computeShader.SetFloat("volume", volume);
            computeShader.SetInt("size",meshSize*meshSize);
            for (int i = 0; i < repeats; i++)
            {
                float randomStart = Random.Range(0,100000);
                computeShader.SetFloat("randomStart", randomStart); 
                // Dispatch the compute shader
                computeShader.Dispatch(kernelHandle, 32, 1, 1);
            }
        }
        Vector3[] vertexes = new Vector3[vertices.Length];
        verticesBuffer.GetData(vertexes);
        int totalDrops = 32 * 32 * 32 * repeats*radii.Count;
        float averageSteps = vertexes[1].z / totalDrops;
        Debug.Log(totalDrops);
        Debug.Log(vertexes[1]);
        Debug.Log("Average Steps: "+averageSteps);
        mesh.vertices = vertexes;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Release the buffers
        verticesBuffer.Release(); 
        double endTime = Time.realtimeSinceStartup;
        Debug.Log("Time Taken: " + (endTime-startTime)*1000+"ms");
        yield break;
    }
    
}