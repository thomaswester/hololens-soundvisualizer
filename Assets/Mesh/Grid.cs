using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

	public int xSize, ySize;

	private Mesh mesh;
	private Vector3[] vertices;
    private Gradient g;

	private void Awake () {

        g = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0].color = Color.blue;
        gck[0].time = 0.0f;
        gck[1].color = Color.red;
        gck[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0f;
        gak[0].time = 0.0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 1.0f;

        g.SetKeys(gck, gak);

        Generate();
	}

    public void UpdateSpectrum( float [] spectrumData )
    {
        //Debug.Log("UpdateSpectrum " + spectrumData.Length);
        
        Vector3[] prevVertices = mesh.vertices;
        Vector3[] vertices = mesh.vertices;

        Color32[] prevcolors = mesh.colors32;
        Color32[] colors = new Color32[vertices.Length];

        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                if (y > 0) {            
                    vertices[i] = new Vector3(vertices[i].x, vertices[i].y, prevVertices[i-xSize-1].z);
                    colors[i] = prevcolors[i - xSize - 1];
                }
            }
        }

        for (int i = 0; i <= xSize; i++)
        {
            float amp = Mathf.Log(spectrumData[i]);
            
            if (!float.IsInfinity(amp) && !float.IsNaN(amp)) { 
                
                vertices[i] = new Vector3(vertices[i].x, vertices[i].y, amp );
                float pct = (float)i / (float)xSize;
                colors[i] = g.Evaluate(pct);
                
            }
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
    }

	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		Vector2[] uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        Color32[] colors = new Color32[vertices.Length];
        for (int i = 0, y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++, i++) {
                int posx = x;
                int posy = y;
				vertices[i] = new Vector3(posx, posy );
                colors[i] = new Color32(255, 255, 255, 255);

                uv[i] = new Vector2((float)posx / xSize, (float)posy / ySize);
				tangents[i] = tangent;
			}
		}

		mesh.vertices = vertices;
        mesh.colors32 = colors;
        //mesh.uv = uv;
        //mesh.tangents = tangents;

        int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}
}