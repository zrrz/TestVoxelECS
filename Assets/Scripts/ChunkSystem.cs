using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class ChunkSystem : MonoBehaviour
{
    public GameObject[] blockGameObjectPrefabs;

    EntityManager entityManager;
    //Entity[] blockEntityPrefabs;

    //Dictionary<Vector3Int, >

    public int chunkSize = 100;

    NativeArray<Entity> chunk;

    Entity blockEntityInstance;

    void Start()
    {
        entityManager = World.Active.EntityManager;
        //blockEntityPrefabs = new Entity[blockGameObjectPrefabs.Length];
        //for(int i = 0; i < blockGameObjectPrefabs.Length; i++)
        //{
        //    blockEntityPrefabs[i] = GameObjectConversionUtility.ConvertGameObjectHierarchy(blockGameObjectPrefabs[i], World.Active);
        //}

        chunk = new Unity.Collections.NativeArray<Entity>(chunkSize * chunkSize * chunkSize, Unity.Collections.Allocator.Persistent);

        blockEntityInstance = entityManager.CreateEntity(
            typeof(Translation),
            typeof(LocalToWorld)
            );

        Unity.Rendering.RenderMesh renderMesh = new Unity.Rendering.RenderMesh
        {
            mesh = blockGameObjectPrefabs[0].GetComponent<MeshFilter>().sharedMesh,
            material = blockGameObjectPrefabs[0].GetComponent<MeshRenderer>().sharedMaterial
        };
        entityManager.AddSharedComponentData(blockEntityInstance, renderMesh);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            UnityEngine.Profiling.Profiler.BeginSample("Clear");
            entityManager.DestroyEntity(chunk);
            UnityEngine.Profiling.Profiler.EndSample();
            UnityEngine.Profiling.Profiler.BeginSample("Create");
            CreateChunk();
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    void OnDestroy()
    {
        if (chunk != null)
            chunk.Dispose();
    }

    void CreateChunk()
    {
        UnityEngine.Profiling.Profiler.BeginSample("Make Entities");
        NativeArray<Entity> blocks = new NativeArray<Entity>(chunkSize * chunkSize * chunkSize, Allocator.TempJob);
        entityManager.Instantiate(blockEntityInstance, blocks);
        UnityEngine.Profiling.Profiler.EndSample();

        UnityEngine.Profiling.Profiler.BeginSample("Set Positions");

        int i = 0;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    entityManager.SetComponentData(blocks[i], new Translation { Value = new Unity.Mathematics.float3(x, y, z) });

                    chunk[i] = blocks[i];                    
                    i++;

                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();

        blocks.Dispose();
    }
}
