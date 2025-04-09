using UnityEngine;

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

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {

	}

    // Update is called once per frame
    void Update()
    {
        foreach (BackgroundElement element in backgroundElements)
        {
			element.backgroundSprite.material.mainTextureOffset = 
                new Vector2(transform.position.x * (1 - element.parallexEffect) * scrollFactor, 0);
		}
    }
}
