using UnityEngine;
using System.Collections;

public class TileSpawnAnimation : MonoBehaviour
{
    public float animationDuration = 0.4f;
    public Vector3 finalScale = Vector3.one;

    void Start()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleUp());
    }

    IEnumerator ScaleUp()
    {
        float time = 0f;

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = finalScale;
    }
}
