using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TextureAtlasCreator
{
    public class TextureAtlasCreator : EditorWindow
    {
        private List<Texture2D> textures;
        private Texture2D atlasAsset;
        private MeshRenderer[] meshRenderers;
        private List<MeshFilter> meshFilters;
        private Dictionary<MeshRenderer, int> rendererToTextureIndex;
        
        private string shaderName = "Mobile/Diffuse";
        private int maxAtlasSize = 2048;

        [MenuItem("Window/TextureAtlasCreator")]
        private static void Init()
        {
            var window = (TextureAtlasCreator) GetWindow(typeof(TextureAtlasCreator));
            window.Show();
        }
        
        private void OnEnable()
        {
            meshFilters = new List<MeshFilter>();
            textures = new List<Texture2D>();
            UpdateSelection();
        }

        private void OnSelectionChange()
        {
            UpdateSelection();
        }

        private void OnGUI()
        {
            AtlasSizeDropdown();

            if (textures.Count == 0)
            {
                EditorGUILayout.HelpBox(string.Format("Can't find any material using {0} shader on selected objects", 
                    shaderName), MessageType.Error);
                return;
            }
            
            GUILayout.Label("Textures in selected meshes: ", EditorStyles.boldLabel);
            foreach (var texture in textures)
            {
                GUILayout.Label(texture.name);
            }
        
            if (GUILayout.Button("CreateTextureAtlas"))
            {
                Rect[] newUvs;
                atlasAsset = PackTextures(out newUvs);
                var atlasMaterial = CreateNewMaterial();
                UpdateMeshes(newUvs, atlasMaterial);
                
                AtlasTexturePreview.Init(atlasAsset);
            }
        }

        private void AtlasSizeDropdown()
        {
            maxAtlasSize = EditorGUILayout.IntPopup("Max Atlas Size", 
                maxAtlasSize, new[] {"256", "512", "1024", "2048", "4096", "8192"},
                new[] {256, 512, 1024, 2048, 4096, 8192});
        }

        private void UpdateSelection()
        {
            textures.Clear();
            meshFilters.Clear();
            foreach (var selectedObject in Selection.gameObjects)
            {
                meshRenderers = selectedObject.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshFilter in meshRenderers)
                {
                    meshFilters.Add(meshFilter.GetComponent<MeshFilter>());
                }

                UpdateSelectedTextures();
            }
        }

        private void UpdateMeshes(Rect[] newUvs, Material material)
        {
            for (int i = 0; i < meshFilters.Count; i++)
            {
                var renderer = meshRenderers[i];
                var texture = renderer.sharedMaterial.mainTexture;
                var textIndex = textures.FindIndex(tex => tex == texture);

                var mesh = meshFilters[i].mesh;
                var uvRectOnAtlas = newUvs[textIndex];
                var transformedUv = ComputeUvsOnAtlas(mesh, uvRectOnAtlas);

                mesh.uv = transformedUv;
                renderer.material = material;
            }
        }

        private static Vector2[] ComputeUvsOnAtlas(Mesh mesh, Rect uvRectOnAtlas)
        {
            var transformedUv = new Vector2[mesh.uv.Length];
            for (var uvIndex = 0; uvIndex < mesh.uv.Length; uvIndex++)
            {
                var oldUv = mesh.uv[uvIndex];
                var newUv = new Vector2
                {
                    x = Mathf.Lerp(uvRectOnAtlas.xMin, uvRectOnAtlas.xMax, oldUv.x),
                    y = Mathf.Lerp(uvRectOnAtlas.yMin, uvRectOnAtlas.yMax, oldUv.y)
                };
                transformedUv[uvIndex] = newUv;
            }
            return transformedUv;
        }

        private void UpdateSelectedTextures()
        {
            foreach (var meshRenderer in meshRenderers)
            {
                var material = meshRenderer.sharedMaterial;
                if (material.shader.name != shaderName)
                {
                    continue;
                }
			
                var texture = material.mainTexture as Texture2D;
                if (!textures.Contains(texture))
                {
                    textures.Add(texture);
                }
            }
        }

        private Texture2D PackTextures(out Rect[] uvs)
        {
            var atlas = new Texture2D(maxAtlasSize, maxAtlasSize);

            foreach (var texture in textures)
            {
                SetTextureAsReadable(texture, true);
            }
        
            AssetDatabase.Refresh();

            uvs = atlas.PackTextures(textures.ToArray(), 2, maxAtlasSize);

            var uncompressedAtlas = new Texture2D(atlas.width, atlas.height);
            uncompressedAtlas.SetPixels(atlas.GetPixels());

            var path = EditorUtility.SaveFilePanelInProject("Atlas Name", "", "png", "");
            File.WriteAllBytes(path, uncompressedAtlas.EncodeToPNG());
        
            foreach (var texture in textures)
            {
                SetTextureAsReadable(texture, false);
            }
        
            AssetDatabase.Refresh();

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void SetTextureAsReadable(Texture2D texture, bool importerIsReadable)
        {
            var texturePath = AssetDatabase.GetAssetPath(texture);
            var importer = (TextureImporter) AssetImporter.GetAtPath(texturePath);
            importer.isReadable = importerIsReadable;
            AssetDatabase.ImportAsset(texturePath);
        }

        private Material CreateNewMaterial()
        {
            var material = new Material(Shader.Find(shaderName));
            var path = EditorUtility.SaveFilePanelInProject("Material Name", "", "mat", "");
            material.mainTexture = atlasAsset;
        
            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.Refresh();
            return material;
        }
    }
}
