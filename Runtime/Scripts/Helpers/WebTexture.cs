using Concept.Helpers;
using UnityEngine;
using UnityEngine.UIElements;

namespace Concept.Helpers
{

public class WebTexture : MonoBehaviour
{

       [SerializeField] private Renderer _renderer;
       [SerializeField] private int _materialIndex;
       [SerializeField] private string _textureURL = "https://bigtex.com/wp-content/uploads/2018/05/placeholder-1920x1080.png";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            if(_renderer ==  null) _renderer = GetComponentInChildren<Renderer>();
            LoadWebTexture( _textureURL );

    }

        public async void LoadWebTexture(string url)
        {
            Texture2D asyncTexture = await NetworkHelper.LoadTextureFromUrlAsync(url);

            Material[] mats = _renderer.materials;
            mats[_materialIndex].mainTexture = asyncTexture;
            _renderer.materials = mats;

        }

    }

}