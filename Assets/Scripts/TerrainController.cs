using UnityEngine;

public class TerrainController : MonoBehaviour
{
    
    [SerializeField] public int terrainSizeX;
    [SerializeField] private int terrainSizeZ;
    [SerializeField] private float heightScale;
    [SerializeField] private PerlinNoise _perlinNoise;
    [SerializeField] private int smoothingPasses;
    [SerializeField] private GameObject mainCam;
    [SerializeField] private float exponential;
    [SerializeField] private Material terrainMat;
    private Vector3[] gridVerts;
    private int[] gridTriangles;
    public Mesh mesh;
   
    void Awake()
    {
        SetCamPos();
        gameObject.AddComponent<MeshRenderer>().material = terrainMat;
        CreateVerticies();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetHeights();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SmoothingSweep(smoothingPasses);
        }
    }

    public void OnClick()
    {
        SetHeights();
    }

    private void SetCamPos()
    {
        mainCam.transform.position = new Vector3(terrainSizeX / 2, heightScale*2, -175);
    }

    private void CreateVerticies()
    {
        gridVerts = new Vector3[terrainSizeX*terrainSizeZ];
        gridTriangles = new int[(terrainSizeX*terrainSizeZ*6)-((terrainSizeX+terrainSizeZ-1)*6)];
        int i = 0;
        for (int z = 0; z < terrainSizeZ; z++)
        {
            for (int x = 0; x < terrainSizeX; x++)
            {
                gridVerts[i] = new Vector3(x, 0, z);
                i++;
            }
        }

        int j = 0;
        for (int z = 0; z < terrainSizeZ-1; z++)
        {
            for (int x = 0; x < terrainSizeX-1; x++)
            {
                gridTriangles[j] = x+(z*terrainSizeX);
                gridTriangles[j + 1] = x+(z*terrainSizeX) + terrainSizeX;
                gridTriangles[j + 2] = x+(z*terrainSizeX) + 1;
    
            
                gridTriangles[j+3] = x+(z*terrainSizeX)+1;
                gridTriangles[j + 4] = x+(z*terrainSizeX) + terrainSizeX;
                gridTriangles[j + 5] = x+(z*terrainSizeX) + terrainSizeX +1;
                
                j += 6;
            }
        }
        
        Debug.Log("Verts: "+gridVerts.Length);
        Debug.Log("Triangle points: "+gridTriangles.Length);
        gameObject.AddComponent<MeshFilter>(); 
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = gridVerts;
        mesh.triangles = gridTriangles;
        CreateUVs();
    }

    private void CreateUVs()
    {
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x/terrainSizeX, vertices[i].z/terrainSizeZ);
        }
        mesh.uv = uvs;
    }

    private void SetHeights()
    {
        Debug.Log("HEIGHT CHANGES");
        Vector3[] vertices = mesh.vertices;
        int i = 0;
        for (int x = 0; x < terrainSizeX; x++)
        {
            for (int z = 0; z < terrainSizeZ;z++)
            {
                float data =  _perlinNoise.noiseData[x, z];
                vertices[i].y = Mathf.Pow(data*heightScale,exponential);
                i++;
            }
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void SmoothingSweep(int passes)
    {
        Vector3[] vertices = mesh.vertices;
        for (int x = 0; x < passes; x++)
        {
            for (int i = terrainSizeX+1; i < vertices.Length-terrainSizeX-terrainSizeX-1; i++)
            {
                if (i%terrainSizeX==0 || i%terrainSizeX==terrainSizeX-1 )
                {
                    continue;
                }
                float totalHeight = 0;
                totalHeight += vertices[i].y;
                totalHeight += vertices[i - terrainSizeX - 1].y;
                totalHeight +=  vertices[i - terrainSizeX].y;
                totalHeight +=  vertices[i - terrainSizeX + 1].y;
                totalHeight +=  vertices[i - 1].y;
                totalHeight +=  vertices[i + 1].y;
                totalHeight += vertices[i +terrainSizeX- 1].y;
                totalHeight += vertices[i +terrainSizeX].y;
                totalHeight += vertices[i +terrainSizeX + 1].y;
                float newY = totalHeight / 9;
                vertices[i].y = newY;
            }
        }
        mesh.vertices = vertices;
        Debug.Log("SMOOTH");
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}