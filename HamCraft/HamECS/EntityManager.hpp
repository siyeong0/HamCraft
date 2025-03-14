#pragma once
#include <array>
#include <queue>

#include "../Common/Common.h"
#include "Entity.h"
#include "Signature.h"

namespace ham
{
	class EntityManager
	{
	public:
		EntityManager();
		~EntityManager();

		Entity CreateEntity();

		void DestroyEntity(Entity entity);

		void SetSignature(Entity entity, Signature signature);

		Signature GetSignature(Entity entity);

	private:
		std::queue<Entity> mAvailableEntities;
		std::array <Signature , MAX_ENTITIES> mSignatures;
		uint32_t mLivingEntityCount;
	};

	EntityManager::EntityManager()
		: mAvailableEntities()
		, mSignatures()
		, mLivingEntityCount(0)
	{
		for (Entity entity = 0; entity < MAX_ENTITIES; ++entity)
		{
			mAvailableEntities.push(entity);
		}
	}

	EntityManager::~EntityManager()
	{

	}

	Entity EntityManager::CreateEntity()
	{
		ASSERT(mLivingEntityCount < MAX_ENTITIES);

		Entity id = mAvailableEntities.front();
		mAvailableEntities.pop();
		++mLivingEntityCount;

		return id;
	}

	void EntityManager::DestroyEntity(Entity entity)
	{
		ASSERT(entity < MAX_ENTITIES);

		mSignatures[entity].reset();

		mAvailableEntities.push(entity);
		--mLivingEntityCount;
	}

	void EntityManager::SetSignature(Entity entity, Signature signature)
	{
		ASSERT(entity < MAX_ENTITIES);

		mSignatures[entity] = signature;
	}

	Signature EntityManager::GetSignature(Entity entity)
	{
		ASSERT(entity < MAX_ENTITIES);

		return mSignatures[entity];
	}
}