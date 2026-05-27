using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpawnDragState : IButtonState
{
    private UnitButton m_btn;

    public SpawnDragState(UnitButton button)
    {
        m_btn = button;
    }

    public void Enter()
    {
        m_btn.PreviewCharacter.gameObject.SetActive(true);
        m_btn.PreviewCharacter.SetSpawn(false);
        m_btn.PreviewCharacter.AtkAreaActive(true);
    }

    public void OnDrag(PointerEventData e)
    {
        UpdatePreviewPosition(e.position);

        TrySnapToTile(
            onHit: (hitPos) => m_btn.PreviewCharacter.transform.position = hitPos,
            onFail: () => UpdatePreviewPosition(e.position)
        );
    }

    public void OnEndDrag(PointerEventData e)
    {
        TrySnapToTile(
            onHit: (hitPos) => TrySpawnCharacter(),
            onFail: () => CancelSpawn()
        );
    }

    public void Exit()
    {
        if (m_btn.PreviewCharacter != null)
        {
            m_btn.PreviewCharacter.AtkAreaActive(false);
        }
    }

    private void UpdatePreviewPosition(Vector2 screenPosition)
    {
        Vector3 worldPos = m_btn.MainCamera.ScreenToWorldPoint(screenPosition);
        worldPos.z = 0;
        m_btn.PreviewCharacter.transform.position = worldPos;
    }

    private void TrySnapToTile(Action<Vector3> onHit, Action onFail)
    {
        Vector3 currentPos = m_btn.PreviewCharacter.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(currentPos, Vector3.forward, float.MaxValue, m_btn.TileMask);

        if (hit.collider != null)
        {
            var spawnTile = hit.collider.GetComponent<SpawnPlayerCharacterTile>();
            if (spawnTile != null && !spawnTile.CheckSpawn())
            {
                onHit?.Invoke(hit.transform.position);
                return;
            }
        }
        onFail?.Invoke();
    }

    private void TrySpawnCharacter()
    {
        Vector3 currentPos = m_btn.PreviewCharacter.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(currentPos, Vector3.forward, float.MaxValue, m_btn.TileMask);

        if (hit.collider == null)
        {
            CancelSpawn();
            return;
        }

        var spawnTile = hit.collider.GetComponent<SpawnPlayerCharacterTile>();

        if (spawnTile == null || spawnTile.CheckSpawn() || !spawnTile.CheckSpawnPoint(false) ||
            !m_btn.InGameUIManager.m_inGameManager.UseCost(m_btn.CharacterData.characterData.cost))
        {
            CancelSpawn();
            return;
        }

        spawnTile.SpawnUnit(m_btn.PreviewCharacter);
        m_btn.PreviewCharacter.enabled = true;
        m_btn.PreviewCharacter.SetSpawn(true);

        m_btn.ChangeState(m_btn.SkillIdle);
    }

    private void CancelSpawn()
    {
        m_btn.PreviewCharacter.gameObject.SetActive(false);
        m_btn.ChangeState(m_btn.SpawnReady);
    }

    public void OnPointerDown(PointerEventData e) { }
    public void OnPointerUp(PointerEventData e) { }
    public void OnBeginDrag(PointerEventData e) { }
    public void Update() { }
}