using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class UnityMeshData
{
    public Vector3[] vertices { get; set; }
    public Vector2[] uvs { get; set; }
    public Vector3[] normals { get; set; }
    public int[] triangleIndicies { get; set; }
    public Color32[] verticeColors { get; set; }

    public UnityMeshData() { }

    public UnityMeshData(int verticeCount)
    {
        this.vertices = new Vector3[verticeCount];
        this.uvs = new Vector2[verticeCount];
        this.normals = new Vector3[verticeCount];
        this.triangleIndicies = new int[verticeCount];
        this.verticeColors = new Color32[verticeCount];
    }

    public UnityMeshData(Vector3[] vertices, Vector2[] uvs, Vector3[] normals, int[] triangleIndicies, Color32[] verticeColors)
    {
        this.vertices = vertices;
        this.uvs = uvs;
        this.normals = normals;
        this.triangleIndicies = triangleIndicies;
        this.verticeColors = verticeColors;
    }
}

public class MeshFaceData
{
    public int verticeIndexX { get; set; }
    public int verticeIndexY { get; set; }
    public int verticeIndexZ { get; set; }
    public string materialName { get; set; }

    public MeshFaceData() { }

    public MeshFaceData(int verticeIndexX, int verticeIndexY, int verticeIndexZ, string materialName)
    {
        this.verticeIndexX = verticeIndexX;
        this.verticeIndexY = verticeIndexY;
        this.verticeIndexZ = verticeIndexZ;
        this.materialName = materialName;
    }
}

public class SmartObjImporter
{
    // Public class members.

    // The game object that contains the mesh.
    public GameObject sceneGameObject { get; set; }
    public float loadingProgress { get; set; }

    // Private class members.
    UnityMeshData[] unityMeshData;

    // Shader for the mesh.
    private Shader meshShader;

    // Class members for submesh materials
    private Dictionary<String, Material> matDic;
    private Material tempMaterial;

    // Class members for common filepaths.
    private string objFilePath;
    private string mtlFilePath;

    // Class members for informations about the obj and the mtl file.
    private FileInfo objFileInfo;

    // Class members for fast text parsing to work with string builders.
    private StringBuilder objSB;
    private StringBuilder objFloatSB;
    private StringBuilder mtlSB;
    private StringBuilder mtlFloatSB;

    //private string objFileText;
    private string mtlFileText;
    private string tempMaterialName;

    // private int objFileTextIndexOffset;
    private int mtlFileTextIndexOffset;
    private int triangleIndiciesCount;

    private const float MAX_TIME_PER_FRAME = 0.5f;
    private const int MAX_VERTICES_PER_MESH = 63999;
    private const int MIN_POW_10 = -16;
    private const int MAX_POW_10 = 16;
    private const int NUM_POWS_10 = MAX_POW_10 - MIN_POW_10 + 1;
    private static readonly float[] pow10 = GenerateLookupTable();

    public void InitImporter(string objFilePath, Shader meshShader)
    {
        this.sceneGameObject = new GameObject(Path.GetFileNameWithoutExtension(objFilePath));
        objFileInfo = new FileInfo(objFilePath);

        this.sceneGameObject.AddComponent<ControlObject>();
        this.objFilePath = objFilePath;
        loadingProgress = 0.0f;
        tempMaterialName = "UnityEngineDefaultMaterial";

        //objFileTextIndexOffset = 0;
        triangleIndiciesCount = 0;

        objSB = new StringBuilder();
        objFloatSB = new StringBuilder();

        this.meshShader = meshShader;

    }

    private void InitMaterialImporter()
    {
        mtlFileText = File.ReadAllText(mtlFilePath);

        mtlSB = new StringBuilder();
        mtlFloatSB = new StringBuilder();

        mtlFileTextIndexOffset = 0;
        tempMaterial = null;
        matDic = new Dictionary<string, Material>();

    }

    // Use this for initialization
    public IEnumerator ImportFile()
    {
        yield return LoadMeshData();
        yield return null;

        Material[] mat = new Material[1];
        mat[0] = new Material(meshShader);
        int meshCount = 1;

        if (triangleIndiciesCount > MAX_VERTICES_PER_MESH)
        {
            meshCount = triangleIndiciesCount / MAX_VERTICES_PER_MESH;

            if (triangleIndiciesCount % MAX_VERTICES_PER_MESH > 0)
            {
                meshCount++;
            }
        }

        for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
        {
            Mesh mesh = new Mesh();

            //mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = unityMeshData[meshIndex].vertices;
            mesh.uv = unityMeshData[meshIndex].uvs;
            mesh.normals = unityMeshData[meshIndex].normals;
            mesh.triangles = unityMeshData[meshIndex].triangleIndicies;
            mesh.colors32 = unityMeshData[meshIndex].verticeColors;
            
            //mesh.RecalculateBounds();

            GameObject childGameObject = new GameObject("Mesh" + meshIndex);

            MeshRenderer renderer = childGameObject.AddComponent<MeshRenderer>();
            MeshFilter filter = childGameObject.AddComponent<MeshFilter>();

            renderer.materials = mat;
            filter.mesh = mesh;

            childGameObject.transform.parent = sceneGameObject.transform;

        }
        //SaveUnityMeshToBinaryFile();

        loadingProgress = 100.0f;
        yield return null;
    }

    public IEnumerator LoadMaterialData()
    {
        for (int mtlFileTextIndex = 0; mtlFileTextIndex < mtlFileText.Length; mtlFileTextIndex++)
        {
            if (mtlFileText[mtlFileTextIndex] == '\n')
            {
                mtlSB.Remove(0, mtlSB.Length);
                mtlFloatSB.Remove(0, mtlFloatSB.Length);

                if (mtlFileText[mtlFileTextIndexOffset] == ' ' || mtlFileText[mtlFileTextIndexOffset] == '\n')
                {
                    mtlSB.Append(mtlFileText, mtlFileTextIndexOffset + 1, mtlFileTextIndex - mtlFileTextIndexOffset);
                }
                else
                {
                    mtlSB.Append(mtlFileText, mtlFileTextIndexOffset, mtlFileTextIndex - mtlFileTextIndexOffset);
                }

                mtlFileTextIndexOffset = mtlFileTextIndex;

                // Check for double whitespaces
                for (int k = 0; k < mtlSB.Length - 1; k++)
                {
                    if (mtlSB[k] == ' ' && mtlSB[k + 1] == ' ')
                    {
                        mtlSB.Remove(k, 1);
                    }
                }

                if (mtlSB[0] == 'n' && mtlSB[1] == 'e' && mtlSB[2] == 'w' && mtlSB[3] == 'm' && mtlSB[4] == 't' && mtlSB[5] == 'l' && mtlSB[6] == ' ')
                {
                    int j = 7;
                    tempMaterialName = "";
                    while (j < mtlSB.Length)
                    {
                        tempMaterialName += mtlSB[j];
                        j++;
                    }
                    tempMaterialName = tempMaterialName.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

                    tempMaterialName = tempMaterialName.Remove(tempMaterialName.Length - 1);

                    if (tempMaterial != null)
                    {
                        matDic.Add(tempMaterial.name, tempMaterial);
                    }
                    tempMaterial = new Material(meshShader);
                    tempMaterial.name = tempMaterialName;
                }
                else if (mtlSB[0] == 'K' && mtlSB[1] == 'd' && mtlSB[2] == ' ')
                {
                    int splitStart = 3;
                    float Kr = GetFloat(mtlSB, ref splitStart, ref mtlFloatSB);
                    float Kg = GetFloat(mtlSB, ref splitStart, ref mtlFloatSB);
                    float Kb = GetFloat(mtlSB, ref splitStart, ref mtlFloatSB);
                    tempMaterial.SetColor("_Color", new Color(Kr, Kg, Kb));
                }
            }

            if (mtlFileTextIndex % 1000000 == 0)
            {
                //Debug.Log(mtlFileTextIndex);
                yield return null;
            }
        }

        if (tempMaterial != null)
        {
            matDic.Add(tempMaterial.name, tempMaterial);
        }

        yield return null;
    }

    private void SaveUnityMeshToBinaryFile()
    {

        using (BinaryWriter writer = new BinaryWriter(
            File.Open(@"C:\test\" + sceneGameObject.name + ".bin", FileMode.Create)))
        {
            MeshFilter[] meshes = sceneGameObject.GetComponentsInChildren<MeshFilter>();

            int byteSize = 0;
            byteSize += sizeof(int);
            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                Mesh mesh = meshes[meshIndex].mesh;
                byteSize += sizeof(int);
                byteSize += ((mesh.vertices.Length * 3) * sizeof(float));
                byteSize += sizeof(int);
                byteSize += ((mesh.uv.Length * 2) * sizeof(float));
                byteSize += sizeof(int);
                byteSize += ((mesh.normals.Length * 3) * sizeof(float));
                byteSize += sizeof(int);
                byteSize += ((mesh.colors.Length * 3) * sizeof(float));
                byteSize += sizeof(int);
                byteSize += ((mesh.triangles.Length) * sizeof(int));
            }
            byte[] bytes = new byte[byteSize];
            Convert.ToByte(meshes.Length);
            for (int meshIndex = 0; meshIndex < meshes.Length; meshIndex++)
            {
                Mesh mesh = meshes[meshIndex].mesh;

                float[] verticesArr = new float[mesh.vertices.Length * 3];

                int verticesIndex = 0;
                for (int i = 0; i < mesh.vertices.Length; i++)
                {
                    verticesArr[verticesIndex] = mesh.vertices[i].x;
                    verticesArr[verticesIndex + 1] = mesh.vertices[i].y;
                    verticesArr[verticesIndex + 2] = mesh.vertices[i].z;
                    verticesIndex += 3;
                }

                float[] normalsArr = new float[mesh.normals.Length * 3];

                int normalsIndex = 0;
                for (int i = 0; i < mesh.normals.Length; i++)
                {
                    normalsArr[normalsIndex] = mesh.normals[i].x;
                    normalsArr[normalsIndex + 1] = mesh.normals[i].y;
                    normalsArr[normalsIndex + 2] = mesh.normals[i].z;
                    normalsIndex += 3;
                }

                float[] uvsArr = new float[mesh.uv.Length * 2];

                int uvsIndex = 0;
                for (int i = 0; i < mesh.uv.Length; i++)
                {
                    uvsArr[uvsIndex] = mesh.uv[i].x;
                    uvsArr[uvsIndex + 1] = mesh.uv[i].y;
                    uvsIndex += 2;
                }

                float[] colorsArr = new float[mesh.colors.Length * 3];

                int colorsIndex = 0;
                for (int i = 0; i < mesh.uv.Length; i++)
                {
                    colorsArr[colorsIndex] = mesh.colors[i].r;
                    colorsArr[colorsIndex + 1] = mesh.colors[i].g;
                    colorsArr[colorsIndex + 2] = mesh.colors[i].b;
                    colorsIndex += 3;
                }

                int[] trianglesArr = new int[mesh.triangles.Length];

                for (int i = 0; i < mesh.triangles.Length; i++)
                {
                    trianglesArr[i] = mesh.triangles[i];
                }


            }

            using (BinaryWriter b = new BinaryWriter(
                    File.Open(@"C:\test\" + sceneGameObject.name + ".bin", FileMode.Create)))
            {
                b.Write(bytes);
            }
        }
    }

    private IEnumerator LoadMeshData()
    {
        yield return null;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<MeshFaceData> faceData = new List<MeshFaceData>();
        using (StreamReader sr = File.OpenText(objFilePath))
        {
            string swapStr = "";
            Debug.Log(Time.realtimeSinceStartup);

            do
            {
                char[] buffer = new char[2000480];
                int len = sr.ReadBlock(buffer, 0, buffer.Length);
                string bufferStr = new string(buffer, 0, len);
                if (!String.IsNullOrEmpty(swapStr))
                {
                    bufferStr = swapStr + bufferStr;
                    swapStr = "";
                }

                int bufferStrLineReminder = bufferStr.LastIndexOf("\n");
                swapStr = bufferStr.Substring(bufferStrLineReminder + 1);
                if (String.IsNullOrEmpty(swapStr))
                    swapStr = "";

                bufferStr = bufferStr.Substring(0, bufferStrLineReminder + 1);
                ReadMeshDataBlock(bufferStr, vertices, uvs, normals, faceData);

                yield return null;
            }
            while (!sr.EndOfStream);

            Debug.Log(Time.realtimeSinceStartup);
        }
        InitMaterialImporter();
        yield return LoadMaterialData();
        yield return GetUnityMeshData(vertices, uvs, normals, faceData);
        GC.WaitForPendingFinalizers();
        yield return null;
    }

    private void ReadMeshDataBlock(String buffer, List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<MeshFaceData> faceData)
    {
        int objFileTextIndexOffset = 0;

        for (int objFileTextIndex = 0; objFileTextIndex < buffer.Length; objFileTextIndex++)
        {
            if (buffer[objFileTextIndex] == '\n')
            {
                objSB.Remove(0, objSB.Length);
                objFloatSB.Remove(0, objFloatSB.Length);

                objSB.Append(buffer, objFileTextIndexOffset, objFileTextIndex - objFileTextIndexOffset);
                objFileTextIndexOffset = objFileTextIndex + 1;

                if (!String.IsNullOrEmpty(objSB.ToString()))
                {
                    // Remove double whitespaces
                    for (int k = 0; k < objSB.Length - 1; k++)
                    {
                        if (objSB[k] == ' ' && objSB[k + 1] == ' ')
                        {
                            objSB.Remove(k, 1);
                        }
                    }

                    if (objSB[0] == 't' && objSB[1] == 'l' && objSB[2] == 'l' && objSB[3] == 'i' && objSB[4] == 'b' && objSB[5] == ' ')
                    {
                        int j = 6;
                        mtlFilePath = "";
                        while (j < objSB.Length)
                        {
                            mtlFilePath += objSB[j];
                            j++;
                        }
                        mtlFilePath = mtlFilePath.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                        mtlFilePath = objFileInfo.Directory.FullName + Path.DirectorySeparatorChar + mtlFilePath;
                    }
                    else if (objSB[0] == 'm' && objSB[1] == 't' && objSB[2] == 'l' && objSB[3] == 'l' && objSB[4] == 'i' && objSB[5] == 'b' && objSB[6] == ' ')
                    {
                        int j = 7;
                        mtlFilePath = "";
                        while (j < objSB.Length)
                        {
                            mtlFilePath += objSB[j];
                            j++;
                        }
                        mtlFilePath = mtlFilePath.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                        mtlFilePath = objFileInfo.Directory.FullName + Path.DirectorySeparatorChar + mtlFilePath;
                    }
                    else if (objSB[0] == 'u' && objSB[1] == 's' && objSB[2] == 'e' && objSB[3] == 'm' && objSB[4] == 't' && objSB[5] == 'l' && objSB[6] == ' ')
                    {
                        int j = 7;
                        tempMaterialName = "";
                        while (j < objSB.Length)
                        {
                            tempMaterialName += objSB[j];
                            j++;
                        }
                        tempMaterialName = tempMaterialName.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                    }
                    else if (objSB[0] == 'v' && objSB[1] == ' ')
                    {
                        int splitStart = 2;

                        vertices.Add(new Vector3(GetFloat(objSB, ref splitStart, ref objFloatSB),
                            GetFloat(objSB, ref splitStart, ref objFloatSB), GetFloat(objSB, ref splitStart, ref objFloatSB)));
                    }
                    else if (objSB[0] == 'v' && objSB[1] == 't' && objSB[2] == ' ')
                    {
                        int splitStart = 3;

                        uvs.Add(new Vector2(GetFloat(objSB, ref splitStart, ref objFloatSB),
                            GetFloat(objSB, ref splitStart, ref objFloatSB)));
                    }
                    else if (objSB[0] == 'v' && objSB[1] == 'n' && objSB[2] == ' ')
                    {
                        int splitStart = 3;

                        normals.Add(new Vector3(GetFloat(objSB, ref splitStart, ref objFloatSB),
                            GetFloat(objSB, ref splitStart, ref objFloatSB), GetFloat(objSB, ref splitStart, ref objFloatSB)));
                    }
                    else if (objSB[0] == 'f' && objSB[1] == ' ')
                    {
                        int splitStart = 2;

                        while (splitStart < objSB.Length && char.IsDigit(objSB[splitStart]))
                        {
                            faceData.Add(new MeshFaceData(GetInt(objSB, ref splitStart, ref objFloatSB),
                                GetInt(objSB, ref splitStart, ref objFloatSB), GetInt(objSB, ref splitStart, ref objFloatSB), tempMaterialName));

                            triangleIndiciesCount++;
                        }
                    }
                }

            }
        }

    }

    private IEnumerator GetUnityMeshData(List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<MeshFaceData> faceData)
    {
        int meshCount = 1;
        if (triangleIndiciesCount > MAX_VERTICES_PER_MESH)
        {
            meshCount = triangleIndiciesCount / MAX_VERTICES_PER_MESH;

            if (triangleIndiciesCount % MAX_VERTICES_PER_MESH > 0)
            {
                meshCount++;
            }
        }

        unityMeshData = new UnityMeshData[meshCount];

        for (int meshIndex = 0; meshIndex < meshCount; meshIndex++)
        {
            int meshFaceDataIndexOffset = 0;
            for (int i = 0; i < meshIndex; i++)
            {
                meshFaceDataIndexOffset += MAX_VERTICES_PER_MESH;
            }

            int meshFaceDataCount = 0;

            if (meshIndex == meshCount - 1)
            {
                meshFaceDataCount = triangleIndiciesCount % MAX_VERTICES_PER_MESH;
            }
            else
            {
                meshFaceDataCount = MAX_VERTICES_PER_MESH;
            }

            Vector3[] newVerts = new Vector3[meshFaceDataCount];
            Vector2[] newUVs = new Vector2[meshFaceDataCount];
            Vector3[] newNormals = new Vector3[meshFaceDataCount];
            Color32[] colors = new Color32[meshFaceDataCount];
            int[] newTriangles = new int[meshFaceDataCount];

            for (int i = 0; i < meshFaceDataCount; i++)
            {
                newVerts[i] = vertices[faceData[i + meshFaceDataIndexOffset].verticeIndexX - 1];
                newUVs[i] = uvs[faceData[i + meshFaceDataIndexOffset].verticeIndexZ - 1];
                newNormals[i] = normals[faceData[i + meshFaceDataIndexOffset].verticeIndexZ - 1];
                if (matDic.ContainsKey(faceData[i + meshFaceDataIndexOffset].materialName))
                {
                    colors[i] = matDic[faceData[i + meshFaceDataIndexOffset].materialName].color;
                }
                else
                {
                    colors[i] = Color.gray;
                }
                
                newTriangles[i] = i;
            }
            
            unityMeshData[meshIndex] = new UnityMeshData(newVerts, newUVs, newNormals, newTriangles, colors);
        }

        yield return null;
    }

    private float GetFloat(StringBuilder sb, ref int start, ref StringBuilder sbFloat)
    {
        sbFloat.Remove(0, sbFloat.Length);
        while (start < sb.Length &&
               (char.IsDigit(sb[start]) || sb[start] == '-' || sb[start] == '.'))
        {
            sbFloat.Append(sb[start]);
            start++;
        }
        start++;

        return ParseFloat(sbFloat);
    }

    private int GetInt(StringBuilder sb, ref int start, ref StringBuilder sbInt)
    {
        sbInt.Remove(0, sbInt.Length);
        while (start < sb.Length &&
               (char.IsDigit(sb[start])))
        {
            sbInt.Append(sb[start]);
            start++;
        }
        start++;

        return IntParseFast(sbInt);
    }

    private float GetFloat(String str, ref int start, ref StringBuilder sbFloat)
    {
        sbFloat.Remove(0, sbFloat.Length);

        while (start < str.Length && str[start] == ' ')
        {
            start++;
        }

        while (start < str.Length &&
               (char.IsDigit(str[start]) || str[start] == '-' || str[start] == '.'))
        {
            sbFloat.Append(str[start]);
            start++;
        }

        do
        {
            start++;
        }
        while (start < str.Length && str[start] == ' ');

        return ParseFloat(sbFloat);
    }

    private int GetInt(String str, ref int start, ref StringBuilder sbInt)
    {
        sbInt.Remove(0, sbInt.Length);

        while (start < str.Length && str[start] == ' ')
        {
            start++;
        }

        while (start < str.Length &&
               (char.IsDigit(str[start])))
        {
            sbInt.Append(str[start]);
            start++;
        }

        do
        {
            start++;
        }
        while (start < str.Length && str[start] == ' ');

        return IntParseFast(sbInt);
    }

    private float ParseFloat(StringBuilder value)
    {
        float result = 0;
        bool negate = false;
        int len = value.Length;
        int decimalIndex = value.Length;
        for (int i = len - 1; i >= 0; i--)
            if (value[i] == '.')
            { decimalIndex = i; break; }
        int offset = -MIN_POW_10 + decimalIndex;
        for (int i = 0; i < decimalIndex; i++)
            if (i != decimalIndex && value[i] != '-')
                result += pow10[(value[i] - '0') * NUM_POWS_10 + offset - i - 1];
            else if (value[i] == '-')
                negate = true;
        for (int i = decimalIndex + 1; i < len; i++)
            if (i != decimalIndex)
                result += pow10[(value[i] - '0') * NUM_POWS_10 + offset - i];
        if (negate)
            result = -result;
        return result;
    }

    private int IntParseFast(StringBuilder value)
    {
        // An optimized int parse method.
        int result = 0;
        for (int i = 0; i < value.Length; i++)
        {
            result = 10 * result + (value[i] - 48);
        }
        return result;
    }

    private static float[] GenerateLookupTable()
    {
        var result = new float[(-MIN_POW_10 + MAX_POW_10) * 10];
        for (int i = 0; i < result.Length; i++)
            result[i] = (float)((i / NUM_POWS_10) *
                    Mathf.Pow(10, i % NUM_POWS_10 + MIN_POW_10));
        return result;
    }

}
