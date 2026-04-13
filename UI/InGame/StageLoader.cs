using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;

public class StageLoader
{
    public async UniTask InitTile(MapData mapData, SpriteAtlas spriteAtlas, CancellationToken destoryToken, Transform parent)
    {
        List<UniTask> tileCreationTasks = new();

        foreach (var data in mapData.tileDatas)
        {
            // 'Delete' 타입은 맵 툴에서 지워진 타일이므로 무시
            if (data.type == MapData.MapObject.Delete) continue;

            // 각 타일을 비동기로 인스턴스화하는 작업을 리스트에 적재
            tileCreationTasks.Add(TileCreationTaskHelper(data, spriteAtlas, destoryToken, parent));
        }

        // 4. 모든 타일 오브젝트가 한꺼번에 생성 완료될 때까지 대기 (로딩 멈춤 현상 최소화)
        await UniTask.WhenAll(tileCreationTasks).AttachExternalCancellation(destoryToken);

        // 5. 생성된 맵의 최대 X, Y 범위를 계산하여 카메라 뷰포트 자동 조절 (정중앙 포커싱 및 크기 맞춤)
        var tileMaxX = mapData.tileDatas.Max(t => t.x);
        var tileMaxY = mapData.tileDatas.Max(t => t.y);

        GameUtil.mainCamera.transform.position = new Vector3(tileMaxX * 0.5f, tileMaxY * 0.5f, -10);
        GameUtil.mainCamera.orthographicSize = System.Math.Max(tileMaxX + 1, tileMaxY + 1) * 0.5f;

        // UI에서 유닛을 해제했을 때 돌아갈 기본 카메라 위치 저장
        GameData.Instance.DefaulteCameraPos = GameUtil.mainCamera.transform.position;
    }

    private async UniTask TileCreationTaskHelper(MapData.TileData tileData, SpriteAtlas spAtlas, CancellationToken cancelToken, Transform parent)
    {
        // 타일 타입이 지정되지 않은 경우(None) 기본 'Wall' 프리팹 경로를 사용
        string prefabName = tileData.type == MapData.MapObject.None
            ? MapData.MapObject.Wall.ToString()
            : tileData.type.ToString();

        string tileAddress = $"Tile/{prefabName}.prefab";

        // Addressables를 통해 해당 타일 프리팹을 로드하고 게임 월드(m_mapObject 하위)에 배치
        var tileObject = await GameMaster.Instance.addressableManager.InstantiateComponentAsync<TileBase>(tileAddress, parent).AttachExternalCancellation(cancelToken);

        if (tileObject != null)
        {
            // 타일의 좌표 정보와 시각적 요소(아틀라스에서 잘라온 스프라이트) 초기 설정
            tileObject.Init(tileData);
            tileObject.SetTileSprite(spAtlas.GetSprite(tileData.spriteName));
        }
    }
}
