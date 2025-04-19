using UnityEngine;

namespace HamCraft
{

    [System.Serializable]
    public class BackgroundElement
    {
        public Renderer backgroundSprite;
        [Range(-1, 1)] public float parallexEffect;
    }

    public class BackgroundScrolling : MonoBehaviour
    {
        [SerializeField] private float scrollFactor = 0.02f;
        [SerializeField] private BackgroundElement[] backgroundElements;

        void Start()
        {

        }

        void Update()
        {
            foreach (BackgroundElement element in backgroundElements)
            {
                element.backgroundSprite.material.mainTextureOffset =
                    new Vector2(transform.position.x * (1 - element.parallexEffect) * scrollFactor, 0);
            }
        }
    }
}