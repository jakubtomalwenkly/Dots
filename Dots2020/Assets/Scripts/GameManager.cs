using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class GameManager : MonoBehaviour
{
    EntityManager entityManager;
    EntityQuery query;
    public static int points;

    // Start is called before the first frame update
    void Start()
    {
        //entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(points);
    }
}
