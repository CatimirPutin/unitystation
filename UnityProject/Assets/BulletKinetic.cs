﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BulletKinetic : BulletBehaviour
{
	public float maxBulletDistance;

	private bool isOnDespawn = false;
	public override void Shoot(Vector2 dir, GameObject controlledByPlayer, Gun fromWeapon, BodyPartType targetZone = BodyPartType.Chest)
	{
		StartShoot(dir, controlledByPlayer, fromWeapon, targetZone);
		StartCoroutine(countTiles());
	}

	private IEnumerator countTiles()
	{
		Vector2 startPos = gameObject.AssumedWorldPosServer();
		//List<Vector3Int> positionList = MatrixManager.GetTiles(startPos, dir, 3);
		float time = maxBulletDistance / weapon.ProjectileVelocity;
		yield return WaitFor.Seconds(time);
		ReturnToPool();
	}

	public override void HandleCollisionEnter2D(Collision2D coll)
	{
		GetComponent<BulletMineOnHit>()?.BulletHitInteract(coll,Direction);
		ReturnToPool(coll);
	}

	

	protected override void ReturnToPool()
	{
		if (!isOnDespawn)
		{

			isOnDespawn = true;
			if (trailRenderer != null)
			{
				trailRenderer.ShotDone();
			}

			rigidBody.velocity = Vector2.zero;
			StartCoroutine(KineticAnim());
		}

	}

	protected void ReturnToPool(Collision2D coll)
	{
		if (!isOnDespawn)
		{
			isOnDespawn = true;
			if (trailRenderer != null)
			{
				trailRenderer.ShotDone();
			}

			rigidBody.velocity = Vector2.zero;
			StartCoroutine(KineticAnim(coll));
		}
	}

	public IEnumerator KineticAnim()
	{

		Transform cellTransform = rigidBody.gameObject.transform;
		MetaTileMap layerMetaTile = cellTransform.GetComponentInParent<MetaTileMap>();
		var position = layerMetaTile.WorldToCell(Vector3Int.RoundToInt(rigidBody.gameObject.AssumedWorldPosServer()));

		TileChangeManager tileChangeManager = transform.GetComponentInParent<TileChangeManager>();

		// Store the old effect
		LayerTile oldEffectLayerTile = tileChangeManager.GetLayerTile(position, LayerType.Effects);

		tileChangeManager.UpdateTile(position, TileType.Effects, "KineticAnimation");

		yield return WaitFor.Seconds(.4f);

		tileChangeManager.RemoveTile(position, LayerType.Effects);

		// Restore the old effect if any (ex: cracked glass, does not work)
		if (oldEffectLayerTile)
			tileChangeManager.UpdateTile(position, oldEffectLayerTile);
		isOnDespawn = false;
		Despawn.ClientSingle(gameObject);
	}

	public IEnumerator KineticAnim(Collision2D coll)
	{

		Transform cellTransform = rigidBody.gameObject.transform;
		MetaTileMap layerMetaTile = cellTransform.GetComponentInParent<MetaTileMap>();

		ContactPoint2D firstContact = coll.GetContact(0);
		Vector3 hitPos = firstContact.point;
		Vector3 forceDir = Direction;
		forceDir.z = 0;
		Vector3 bulletHitTarget = hitPos + (forceDir * 0.2f);
		Vector3Int cellPos = layerMetaTile.WorldToCell(Vector3Int.RoundToInt(bulletHitTarget));

		TileChangeManager tileChangeManager = transform.GetComponentInParent<TileChangeManager>();

		// Store the old effect
		LayerTile oldEffectLayerTile = tileChangeManager.GetLayerTile(cellPos, LayerType.Effects);

		tileChangeManager.UpdateTile(cellPos, TileType.Effects, "KineticAnimation");

		yield return WaitFor.Seconds(.4f);

		tileChangeManager.RemoveTile(cellPos, LayerType.Effects);

		// Restore the old effect if any (ex: cracked glass, does not work)
		if (oldEffectLayerTile)
		{
			tileChangeManager.UpdateTile(cellPos, oldEffectLayerTile);
		}
		isOnDespawn = false;
		Despawn.ClientSingle(gameObject);
	}

}
