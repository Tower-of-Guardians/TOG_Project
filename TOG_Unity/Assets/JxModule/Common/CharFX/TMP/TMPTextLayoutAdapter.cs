using System;
using TMPro;
using UnityEngine;

namespace JxModule.CharFX
{
    public class TMPTextLayoutAdapter : ICharFXTextLayout
    {
        private readonly TMP_Text _text;
        
        private TMP_TextInfo _textInfo;
        private Vector3[][] _baseVertices;
        private Vector3[][] _workVertices;
        private Color32[][] _baseColors;
        private Color32[][] _workColors;

        public int CharacterCount => _textInfo?.characterCount ?? 0;
        
        public TMPTextLayoutAdapter(TMP_Text text)
        {
            _text = text;
        }

        public void Rebuild()
        {
            if (_text == null)
            {
                return;
            }
            
            _text.ForceMeshUpdate();
            _textInfo = _text.textInfo;

            var meshCount = _textInfo.meshInfo.Length;
            _baseVertices = new Vector3[meshCount][];
            _workVertices = new Vector3[meshCount][];
            _baseColors = new Color32[meshCount][];
            _workColors = new Color32[meshCount][];

            for (var i = 0; i < meshCount; i++)
            {
                var sourceVertices = _textInfo.meshInfo[i].vertices;
                var sourceColors = _textInfo.meshInfo[i].colors32;
                
                _baseVertices[i] = new Vector3[sourceVertices.Length];
                _workVertices[i] = new Vector3[sourceVertices.Length];
                _baseColors[i] = new Color32[sourceColors.Length];
                _workColors[i] = new Color32[sourceColors.Length];
                
                Array.Copy(sourceVertices, _baseVertices[i], sourceVertices.Length);
                Array.Copy(sourceVertices, _workVertices[i], sourceVertices.Length);
                Array.Copy(sourceColors, _baseColors[i], sourceColors.Length);
                Array.Copy(sourceColors, _workColors[i], sourceColors.Length);
            }
        }

        public void ResetWorkToBase()
        {
            if (_baseVertices == null || _workVertices == null || _baseColors == null || _workColors == null)
            {
                return;
            }

            for (var i = 0; i < _baseVertices.Length; i++)
            {
                if (_baseVertices[i] == null || _workVertices[i] == null)
                {
                    continue;
                }

                if (_baseVertices[i].Length != _workVertices[i].Length)
                {
                    continue;
                }
                
                Array.Copy(_baseVertices[i], _workVertices[i], _baseVertices[i].Length);

                if (_baseColors[i] == null || _workColors[i] == null)
                {
                    continue;
                }

                if (_baseColors[i].Length != _workColors[i].Length)
                {
                    continue;
                }
                
                Array.Copy(_baseColors[i], _workColors[i], _baseColors[i].Length);
            }
        }

        public bool IsVisible(int charIndex)
        {
            if (_textInfo == null)
            {
                return false;
            }

            if (charIndex < 0 || charIndex >= CharacterCount)
            {
                return false;
            }
            
            return _textInfo.characterInfo[charIndex].isVisible;
        }

        public bool TryGetQuad(int charIndex, out CharQuad quad)
        {
            quad = default;

            if (_textInfo == null)
            {
                return false;
            }

            if (charIndex < 0 || charIndex >= CharacterCount)
            {
                return false;
            }

            var ch = _textInfo.characterInfo[charIndex];
            if (!ch.isVisible)
            {
                return false;
            }

            var materialIndex = ch.materialReferenceIndex;
            var vertexIndex = ch.vertexIndex;

            if (_workVertices == null)
            {
                return false;
            }

            if (materialIndex < 0 || materialIndex >= _workVertices.Length)
            {
                return false;
            }
            
            var vertices = _workVertices[materialIndex];
            var colors = _workColors[materialIndex];
            if (vertices == null || colors == null)
            {
                return false;
            }

            if (vertexIndex < 0 || vertexIndex + 3 >= vertices.Length)
            {
                return false;
            }
            
            quad = new CharQuad(vertices[vertexIndex + 0],
                                vertices[vertexIndex + 1],
                                vertices[vertexIndex + 2],
                                vertices[vertexIndex + 3],
                                colors[vertexIndex + 0],
                                colors[vertexIndex + 1],
                                colors[vertexIndex + 2],
                                colors[vertexIndex + 3]);

            return true;
        }

        public void SetQuad(int charIndex, in CharQuad quad)
        {
            if (_textInfo == null)
            {
                return;
            }

            if (charIndex < 0 || charIndex >= CharacterCount)
            {
                return;
            }
            
            var ch = _textInfo.characterInfo[charIndex];
            if (!ch.isVisible)
            {
                return;
            }
            
            var materialIndex = ch.materialReferenceIndex;
            var vertexIndex = ch.vertexIndex;

            if (_workVertices == null)
            {
                return;
            }

            if (materialIndex < 0 || materialIndex >= _workVertices.Length)
            {
                return;
            }
            
            var vertices =  _workVertices[materialIndex];
            var colors = _workColors[materialIndex];
            if (vertices == null || colors == null)
            {
                return;
            }

            if (vertexIndex < 0 || vertexIndex + 3 >= vertices.Length)
            {
                return;
            }

            vertices[vertexIndex + 0] = quad.V0;
            vertices[vertexIndex + 1] = quad.V1;
            vertices[vertexIndex + 2] = quad.V2;
            vertices[vertexIndex + 3] = quad.V3;
            colors[vertexIndex + 0] = quad.C0;
            colors[vertexIndex + 1] = quad.C1;
            colors[vertexIndex + 2] = quad.C2;
            colors[vertexIndex + 3] = quad.C3;
        }

        public void ApplyToText()
        {
            if (_text == null || _textInfo == null || _workVertices == null || _workColors == null)
            {
                return;
            }

            for (var i = 0; i < _textInfo.meshInfo.Length; i++)
            {
                var meshInfo =  _textInfo.meshInfo[i];

                if (i < 0 || i >= _workVertices.Length || i >= _workColors.Length)
                {
                    return;
                }
                
                meshInfo.mesh.vertices = _workVertices[i];
                meshInfo.mesh.colors32 = _workColors[i];
                _text.UpdateGeometry(meshInfo.mesh, i);
            }
        }
    }
}