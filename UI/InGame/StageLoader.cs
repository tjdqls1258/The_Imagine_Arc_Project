using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using VContainer;

public class StageLoader
{
    private readonly UIManager uiManager;
    private readonly AddressableManager addressableManager;

    public StageLoader(UIManager uiManager, AddressableManager addressableManager)
    {
        this.uiManager = uiManager;
        this.addressableManager = addressableManager;
    }

    public async UniTask InitTile(MapData mapData, SpriteAtlas spriteAtlas, CancellationToken destoryToken, Transform parent, InGameManager gameManager)
    {
        List<UniTask> tileCreationTasks = new();

        foreach (var data in mapData.tileDatas)
        {
            if (data.type == MapData.MapObject.Delete) continue;

            tileCreationTasks.Add(TileCreationTaskHelper(data, spriteAtlas, destoryToken, parent, gameManager));
        }

        await UniTask.WhenAll(tileCreationTasks).AttachExternalCancellation(destoryToken);

        var tileMaxX = mapData.tileDatas.Max(t => t.x);
        var tileMaxY = mapData.tileDatas.Max(t => t.y);

        GameUtil.mainCamera.transform.position = new Vector3(tileMaxX * 0.5f, tileMaxY * 0.5f, -10);
        GameUtil.mainCamera.orthographicSize = System.Math.Max(tileMaxX + 1, tileMaxY + 1) * 0.5f;

        GameData.Instance.DefaulteCameraPos = GameUtil.mainCamera.transform.position;
    }

    private async UniTask TileCreationTaskHelper(MapData.TileData tileData, SpriteAtlas spAtlas, CancellationToken cancelToken, Transform parent, InGameManager gameManager)
    {
        string prefabName = tileData.type == MapData.MapObject.None
            ? MapData.MapObject.Wall.ToString()
            : tileData.type.ToString();

        string tileAddress = $"Tile/{prefabName}.prefab";

        var tileObject = await addressableManager.InstantiateComponentAsync<TileBase>(tileAddress, parent).AttachExternalCancellation(cancelToken);

        if (tileObject != null)
        {
            tileObject.Init(tileData, uiManager);
            tileObject.SetTileSprite(spAtlas.GetSprite(tileData.spriteName));
            gameManager.dragCharacter += tileObject.SpawnableCharacter;
        }
    }
}
