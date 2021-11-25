using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ScrCamera : NetworkBehaviour
{
    private Vector3 StartingPosition;

    // Start is called before the first frame update
    void Start()
    {
        StartingPosition = transform.position;
    }

    [Command(requiresAuthority = false)]
    public void CmdPushCamera(Vector3 direction, float intensity, float duration)
    {
        RpcPushInDirection(direction, intensity, duration);
    }

    [ClientRpc]
    private void RpcPushInDirection(Vector3 direction, float intensity, float duration)
    {
        StartCoroutine(Push(direction, intensity, duration));
    }

    IEnumerator Push(Vector3 direction, float intensity, float duration)
    {
        for (float elapsedTime = 0; elapsedTime < duration; elapsedTime += Time.deltaTime)
        {
            transform.position += direction * intensity * Time.deltaTime;
            yield return null;
        }

        StartCoroutine(Return(0.1f));
    }


    IEnumerator Return(float duration)
    {
        for (float elapsedTime = 0; elapsedTime < duration; elapsedTime += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(transform.position, StartingPosition, elapsedTime / duration);
            yield return null;
        }
    }
}
