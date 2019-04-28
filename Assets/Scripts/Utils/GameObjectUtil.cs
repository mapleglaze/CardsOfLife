using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapleGlaze.CardsOfLife.Utils
{
    public static class GameObjectUtil
    {
        public static Mesh GenerateQuadMesh(float width, float height)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[]
                {
                        new Vector3(-width / 2f, -height / 2f, 0.0f), new Vector3(width / 2f, -height / 2f, 0.0f),
                        new Vector3(width / 2f, height / 2f, 0.0f), new Vector3(-width / 2f, height / 2f, 0.0f)
                };

            mesh.vertices = vertices;

            // create UV
            Vector2[] uvs = new Vector2[mesh.vertices.Length];
            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(1, 1);
            uvs[3] = new Vector2(0, 1);

            mesh.uv = uvs;

            mesh.triangles = new int[] { 3, 2, 0, 2, 1, 0 };

            mesh.RecalculateNormals();

            return mesh;
        }

        public static GameObject GenerateQuad(string name, Vector3 position, float width, float height, Material material)
        {
            GameObject gameObject = new GameObject(name);

            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();

            gameObject.GetComponent<MeshFilter>().sharedMesh = GenerateQuadMesh(width, height);
            gameObject.GetComponent<Renderer>().material = material;
            
            gameObject.transform.position = new Vector3(position.x, position.y, position.z);

            return gameObject;
        }

        public static void SetParentRetainLocal(GameObject obj, Transform parentTransform)
        {
            Vector3 localScale = obj.transform.localScale;
            Vector3 localPosition = obj.transform.localPosition;
            Quaternion localRotation = obj.transform.localRotation;

            obj.transform.parent = parentTransform;

            obj.transform.localScale = localScale;
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = localRotation;
        }
    }
}