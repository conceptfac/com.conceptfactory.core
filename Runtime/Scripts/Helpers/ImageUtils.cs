using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Concept.Helpers
{

public static class ImageUtils
{
        public static Sprite LoadSpriteFromProjectURL(string projectUrl)
        {
            // Extrai GUID e Sprite Name
            string guid = ExtractGuid(projectUrl);
            string spriteName = ExtractSpriteName(projectUrl);

            if (string.IsNullOrEmpty(guid) || string.IsNullOrEmpty(spriteName))
            {
                Debug.LogError("[SpriteLoader] GUID ou SpriteName ausente.");
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"[SpriteLoader] GUID inválido: {guid}");
                return null;
            }

            // Carrega todos os Sprites no Atlas
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                                            .OfType<Sprite>()
                                            .ToArray();

            // Busca a sprite pelo nome
            Sprite sprite = sprites.FirstOrDefault(s => s.name == spriteName);
            if (sprite == null)
            {
                Debug.LogError($"[SpriteLoader] Sprite '{spriteName}' não encontrada em {assetPath}");
            }

            return sprite;
        }

        private static string ExtractGuid(string projectUrl)
        {
            int guidIndex = projectUrl.IndexOf("guid=");
            if (guidIndex == -1) return null;

            int ampIndex = projectUrl.IndexOf("&", guidIndex);
            if (ampIndex == -1) ampIndex = projectUrl.Length;

            return projectUrl.Substring(guidIndex + 5, ampIndex - (guidIndex + 5));
        }

        private static string ExtractSpriteName(string projectUrl)
        {
            int hashIndex = projectUrl.IndexOf("#");
            if (hashIndex == -1) return null;

            return projectUrl.Substring(hashIndex + 1);
        }

    }

}