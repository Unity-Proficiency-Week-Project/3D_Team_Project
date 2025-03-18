using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] enemyPrefabs;
    public GameObject[] resourcePrefabs;

    public int maxEnemy = 6;
    public int maxResource = 15;

    public Bounds spawnArea;
    public LayerMask objectLayer;
    public float minDistance = 3f;
    public float respawnInterval = 3f;
    public float spawnCheckRadius = 2.0f;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<GameObject> activeResources = new List<GameObject>();

    private void Start()
    {
        SpawnObjects(enemyPrefabs, maxEnemy, activeEnemies, true);
        SpawnObjects(resourcePrefabs, maxResource, activeResources, false);

        StartCoroutine(CheckAndRespawn()); // 지속적으로 리스폰 체크
    }

    private void SpawnObjects(GameObject[] prefabs, int count, List<GameObject> activeList, bool requiredNav)
    {
        int maxAttempts = 10; // 최대 시도 횟수 (무한 루프 방지)

        for (int i = 0; i < count; i++)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < maxAttempts)
            {
                Vector3 randomPosition;
                if(requiredNav)
                {
                    if (!GetRandomNavMeshPosition(out randomPosition))
                    {
                        attempts++;
                        continue;
                    }
                }
                else
                {
                    randomPosition = GetRandomCubeSpawnPosition();
                }

                if (!IsPositionOccupied(randomPosition))
                {
                    GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                    GameObject spawnedObject = Instantiate(prefab, randomPosition, Quaternion.identity);

                    if (requiredNav)
                    {
                        // NavMeshAgent 초기화
                        NavMeshAgent agent = spawnedObject.GetComponent<NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.Warp(randomPosition); // NavMesh 위에서 정확한 위치 설정
                        }
                    }
                    activeList.Add(spawnedObject); // 리스트에 추가
                    placed = true;
                }
                else
                {
                    attempts++;
                }
            }
        }
    }

    private Vector3 GetRandomCubeSpawnPosition()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("스폰 가능한 큐브가 없습니다!");
            return Vector3.zero;
        }

        Transform randomCube = spawnPoints[Random.Range(0, spawnPoints.Count)];
        Vector3 spawnPosition = randomCube.position;
        spawnPosition.y += 1.0f; // 큐브 위로 약간 올려서 배치

        return spawnPosition;
    }

    private bool GetRandomNavMeshPosition(out Vector3 result)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("❌ 스폰 가능한 큐브가 없습니다!");
            result = Vector3.zero;
            return false;
        }

        for (int i = 0; i < 10; i++) // 최대 10번 시도
        {
            Transform randomCube = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Vector3 randomPosition = randomCube.position;
            randomPosition.y += 1.0f; // ✅ 큐브 위로 올려서 배치

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, spawnCheckRadius, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, minDistance, objectLayer);
        return colliders.Length > 0;
    }

    private IEnumerator CheckAndRespawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnInterval);

            RespawnIfNeeded(enemyPrefabs, maxEnemy, activeEnemies, true);
            RespawnIfNeeded(resourcePrefabs, maxResource, activeResources, false);
        }
    }

    private void RespawnIfNeeded(GameObject[] prefabs, int maxCount, List<GameObject> activeList, bool requiredNav)
    {
        // 리스트에서 삭제된 오브젝트 정리
        activeList.RemoveAll(item => item == null);

        int missingCount = maxCount - activeList.Count;
        if (missingCount > 0)
        {
            SpawnObjects(prefabs, missingCount, activeList, requiredNav);
        }
    }
}
