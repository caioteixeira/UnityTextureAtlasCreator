using UnityEditor;
using UnityEngine;

namespace TextureAtlasCreator
{
    public class AtlasTexturePreview : EditorWindow
    {
        private Texture2D texture;
        
        public static void Init(Texture2D texture)
        {
            var window = GetWindowWithRect<AtlasTexturePreview>(new Rect(0, 0, 512, 512));
            window.texture = texture;
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUI.DrawPreviewTexture(new Rect(0, 0, 512, 512), texture);
        }
    }
}