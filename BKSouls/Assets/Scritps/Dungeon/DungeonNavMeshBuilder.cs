using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

namespace BK
{
    /// <summary>
    /// 런타임에 동적으로 생성된 던전 지형에 대해 NavMesh를 재빌드한다.
    ///
    /// - NavMesh는 네트워크 동기화 불필요: 모든 머신이 동일한 룸 프리팹을 로드하므로
    ///   각자 로컬에서 계산해도 결과가 동일하다.
    /// - 서버: Rebuild() → 완료 콜백에서 적 스폰 (NavMesh 없이는 NavMeshAgent 오류 발생)
    /// - 클라이언트: Rebuild() fire-and-forget (AI는 서버에서만 실행됨)
    ///
    /// Inspector 설정:
    ///   NavMeshSurface 컴포넌트의 Collect Objects = All Game Objects 로 설정하면
    ///   씬에 배치된 모든 룸 지오메트리를 자동으로 수집한다.
    /// </summary>
    public class DungeonNavMeshBuilder : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface surface;

        /// <summary>NavMesh가 현재 사용 가능한 상태인지</summary>
        public bool IsReady { get; private set; }

        // ─────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// NavMesh를 비동기로 재빌드한다.
        /// 한 프레임을 기다려 지오메트리가 씬에 완전히 배치된 후 빌드한다.
        /// </summary>
        /// <param name="onComplete">빌드 완료 시 호출될 콜백 (선택)</param>
        public void Rebuild(Action onComplete = null)
        {
            if (surface == null)
            {
                Debug.LogError("[DungeonNavMeshBuilder] NavMeshSurface가 할당되지 않았습니다.");
                onComplete?.Invoke();
                return;
            }

            StopAllCoroutines();
            StartCoroutine(RebuildCoroutine(onComplete));
        }

        /// <summary>
        /// 기존 NavMesh 데이터를 제거한다.
        /// 룸을 파괴하기 전에 호출하면 이전 데이터가 남지 않는다.
        /// </summary>
        public void Clear()
        {
            IsReady = false;

            if (surface == null)
                return;

            if (surface.navMeshData != null)
                surface.RemoveData();
        }

        // ─────────────────────────────────────────────────────────
        //  Internal
        // ─────────────────────────────────────────────────────────

        private IEnumerator RebuildCoroutine(Action onComplete)
        {
            IsReady = false;

            // 지오메트리가 씬에 완전히 배치될 때까지 한 프레임 대기
            yield return null;

            float startTime = Time.realtimeSinceStartup;

            // BuildNavMesh()는 동기 호출이지만 소규모 던전 룸에서는 충분히 빠르다.
            // 대형 맵이라면 NavMeshBuilder.UpdateNavMeshDataAsync()로 대체 가능.
            surface.BuildNavMesh();

            float elapsed = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log($"[DungeonNavMeshBuilder] NavMesh 재빌드 완료 ({elapsed:F1}ms)");

            IsReady = true;
            onComplete?.Invoke();
        }
    }
}
