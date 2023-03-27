using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWay : MonoBehaviour
{
    [SerializeField] private int _textureSize = 128;
    [SerializeField] private TextureWrapMode _textureWrapMode;
    [SerializeField] private FilterMode _filterMode;
    [SerializeField] private Texture2D _texture;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Material _material;
    [SerializeField] private Color _color;
    [SerializeField] private int _brushSize;
    

    private void OnValidate()
    {
        if (_texture == null)
        {
            _texture = new Texture2D(_textureSize, _textureSize);
        }
        if(_texture.width != _textureSize)
        {
            _texture.Reinitialize(_textureSize, _textureSize);
        }
        _texture.wrapMode = _textureWrapMode;
        _texture.filterMode = _filterMode;
        _material.mainTexture = _texture;
        _texture.Apply();
    }

    public void getMovedTouchData(RaycastHit hit)
    {
        if (hit.transform.tag != "Wall")
        {
            int rayX = (int)(hit.textureCoord.x * _textureSize);
            int rayY = (int)(hit.textureCoord.y * _textureSize);
            DrawCircle(rayX, rayY);
            _texture.Apply();
        }
        else
        {
            _texture.Reinitialize(_textureSize, _textureSize);
            _texture.Apply();
        }
    }

    public void getEndTouchData()
    {
        _texture.Reinitialize(_textureSize, _textureSize);
        _texture.Apply();
    }

    private void DrawCircle(int rayX, int rayY)
    {
        for (int y = 0; y < _brushSize; y++)
        {
            for (int x = 0; x < _brushSize; x++)
            {
                float x2 = Mathf.Pow(x - _brushSize / 2, 2);
                float y2 = Mathf.Pow(y - _brushSize / 2, 2);
                float r2 = Mathf.Pow(_brushSize / 2 - 0.5f, 2);

                if (x2 + y2 < r2)
                {
                    _texture.SetPixel(rayX + x - _brushSize / 2, rayY + y - _brushSize / 2, _color);
                }
            }
        }
    }
}
