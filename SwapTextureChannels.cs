using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class SwapTextureChannels : MonoBehaviour
{
    [MenuItem("Assets/Greenf1re/Split Texture Channels")]
    static void SplitChannels(){
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Selected object is not a texture.");
            return;
        }
        Color[] pixels = texture.GetPixels();
        Color[] r = new Color[pixels.Length];
        Color[] g = new Color[pixels.Length];
        Color[] b = new Color[pixels.Length];
        Color[] a = new Color[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            r[i] = new Color(pixel.r, pixel.r, pixel.r, 1);
            g[i] = new Color(pixel.g, pixel.g, pixel.g, 1);
            b[i] = new Color(pixel.b, pixel.b, pixel.b, 1);
            a[i] = new Color(pixel.a, pixel.a, pixel.a, 1);
        }
        // make new textures
        Texture2D newTextureR = new Texture2D(texture.width, texture.height);
        newTextureR.SetPixels(r);
        newTextureR.Apply();
        Texture2D newTextureG = new Texture2D(texture.width, texture.height);
        newTextureG.SetPixels(g);
        newTextureG.Apply();
        Texture2D newTextureB = new Texture2D(texture.width, texture.height);
        newTextureB.SetPixels(b);
        newTextureB.Apply();
        Texture2D newTextureA = new Texture2D(texture.width, texture.height);
        newTextureA.SetPixels(a);
        newTextureA.Apply();
        // save new textures
        string path = AssetDatabase.GetAssetPath(texture);
        string newPathR = path.Replace(".png", "_r.png");
        byte[] bytesR = newTextureR.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPathR, bytesR);
        string newPathG = path.Replace(".png", "_g.png");
        byte[] bytesG = newTextureG.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPathG, bytesG);
        string newPathB = path.Replace(".png", "_b.png");
        byte[] bytesB = newTextureB.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPathB, bytesB);
        string newPathA = path.Replace(".png", "_a.png");
        byte[] bytesA = newTextureA.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPathA, bytesA);
        AssetDatabase.Refresh();
    }
    
    [MenuItem("Assets/Greenf1re/Swap Normal Channels")]
    static void SwapChannels(){
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Selected object is not a texture.");
            return;
        }
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            pixels[i] = new Color(pixel.a, pixel.g, pixel.b, pixel.r);
        }
        // make new texture
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        // save new texture
        string path = AssetDatabase.GetAssetPath(texture);
        string newPath = path.Replace(".png", "_swapped.png");
        byte[] bytes = newTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPath, bytes);
        AssetDatabase.Refresh();
    }
    [MenuItem("Assets/Greenf1re/Invert Color")]
    static void InvertTexture(){
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Selected object is not a texture.");
            return;
        }
        Color[] pixels = texture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            pixels[i] = new Color(1 - pixel.r, 1 - pixel.g, 1 - pixel.b, pixel.a);
        }
        // make new texture
        Texture2D newTexture = new Texture2D(texture.width, texture.height);
        newTexture.SetPixels(pixels);
        newTexture.Apply();
        // save new texture
        string path = AssetDatabase.GetAssetPath(texture);
        string newPath = path.Replace(".png", "_inverted.png");
        byte[] bytes = newTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPath, bytes);
        AssetDatabase.Refresh();
    }
    [MenuItem("Assets/Greenf1re/NormalFromHeight")]
    static void NormalFromHeight(){
        // RGB to greyscale, then heightmap to normal map
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Selected object is not a texture.");
            return;
        }
        Color[] pixels = texture.GetPixels();
        Color[] greyscale = new Color[pixels.Length];
        Color[] normals = new Color[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            float grey = pixel.grayscale;
            greyscale[i] = new Color(grey, grey, grey, 1);
        }
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                int i = x + y * texture.width;
                float height = greyscale[i].r;
                float heightL = greyscale[Mathf.Max(0, x - 1) + y * texture.width].r;
                float heightR = greyscale[Mathf.Min(texture.width - 1, x + 1) + y * texture.width].r;
                float heightD = greyscale[x + Mathf.Max(0, y - 1) * texture.width].r;
                float heightU = greyscale[x + Mathf.Min(texture.height - 1, y + 1) * texture.width].r;
                Vector3 dx = new Vector3(1, 0, (heightR - heightL) * 0.5f);
                Vector3 dy = new Vector3(0, 1, (heightU - heightD) * 0.5f);
                Vector3 normal = Vector3.Cross(dx, dy).normalized;
                normals[i] = new Color(normal.x * 0.5f + 0.5f, normal.y * 0.5f + 0.5f, normal.z * 0.5f + 0.5f, 1);
            }
        }
       
        Texture2D newTextureNormal = new Texture2D(texture.width, texture.height);
        newTextureNormal.SetPixels(normals);
        newTextureNormal.Apply();
        // save new textures
        string path = AssetDatabase.GetAssetPath(texture);
        string newPathNormal = path.Replace(".png", "_normal.png");
        byte[] bytesNormal = newTextureNormal.EncodeToPNG();
        System.IO.File.WriteAllBytes(newPathNormal, bytesNormal);
        AssetDatabase.Refresh();
    }

}
#endif