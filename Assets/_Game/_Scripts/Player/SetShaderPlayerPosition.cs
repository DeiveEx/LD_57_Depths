using UnityEngine;

public class SetShaderPlayerPosition : MonoBehaviour
{
    private static readonly int PlayerPos = Shader.PropertyToID("_PlayerPos");

    private void Update()
    {
        Shader.SetGlobalVector(PlayerPos, transform.position);
    }
}
