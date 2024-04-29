using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DestructibleObject : MonoBehaviour
{
    //stat fields
    [SerializeField]
    private float health;
    [SerializeField]
    private int pointWorth;

    public bool hasBeenHit = false;
    private WaitForSeconds damageTakeDelay = new WaitForSeconds(0.1f);

    //explosion fields
    private int cubesPerAxis = 8;
    [SerializeField]
    private float force = 300f;
    private float radius = 2f;

    //screen shake
    private CinemachineImpulseSource source;

    private void Awake()
    {
        source = GetComponent<CinemachineImpulseSource>();
    }

    public void TakeDamage(float damage)
    {
        if (hasBeenHit)
            return;

        hasBeenHit = true;

        health -= Mathf.Clamp(damage, 0f, 1f);

        if (health <= 0)
            Invoke("Break", 0.1f);

        StartCoroutine(ResetHasBeenHit());
    }

    private void Break()
    {
        ExplodeCube();
        CameraShake();
        Destroy(gameObject);
    }

    private IEnumerator ResetHasBeenHit()
    {
        yield return damageTakeDelay;
        hasBeenHit = false;
    }

    public void ExplodeCube()
    {
        for (int x = 0; x < cubesPerAxis; x++)
            for (int y = 0; y < cubesPerAxis; y++)
                for (int z = 0; z < cubesPerAxis; z++)
                    CreateCube(new Vector3(x, y, z));

        Destroy(gameObject);
    }

    private void CreateCube(Vector3 coordinates)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        Renderer rd = cube.GetComponent<Renderer>();
        rd.material = GetComponent<Renderer>().material;

        cube.transform.localScale = transform.localScale / cubesPerAxis;

        Vector3 firstCube = transform.position - transform.localScale / 2 + cube.transform.localScale / 2;
        cube.transform.position = firstCube + Vector3.Scale(coordinates, cube.transform.localScale);

        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.AddExplosionForce(force, transform.position, radius);

        cube.AddComponent<DestroyCube>();
    }
    private void CameraShake()
    {
        source.GenerateImpulse();
    }
}
