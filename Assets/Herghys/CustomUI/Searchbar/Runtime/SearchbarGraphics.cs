using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Herghys.CustomUI.Searchbar.Runtime
{
    [Serializable]
    public struct OpenCloseSpriteState : IEquatable<OpenCloseSpriteState>
    {
        [SerializeField]
        private Sprite m_openedSprite;

        [SerializeField]
        private Sprite m_closedSprite;

        public Sprite OpenedSprite { get => m_openedSprite; set => m_openedSprite = value; }
        public Sprite ClosedSprite { get => m_closedSprite; set => m_closedSprite = value; }

        public bool Equals(OpenCloseSpriteState other)
        {
            return OpenedSprite == other.OpenedSprite &&
                ClosedSprite == other.ClosedSprite;
        }
    }

    [Serializable]
    public struct SearchbarSelectedBackground : IEquatable<SearchbarSelectedBackground>
    {
        [SerializeField]
        private Color m_normalColor;

        [SerializeField]
        private Color m_selectedColor;

        public Color NormlaSprite { get => m_normalColor; set => m_normalColor = value; }
        public Color SelectedSprite { get => m_selectedColor; set => m_selectedColor = value; }

        public bool Equals(SearchbarSelectedBackground other)
        {
            return NormlaSprite == other.NormlaSprite &&
                SelectedSprite == other.SelectedSprite;
        }
    }
}
