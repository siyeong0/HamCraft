#pragma once
#include "../Common/Common.h"
#include "Entity.h"

namespace ham
{
	class IComponentArray
	{
	public:
		virtual ~IComponentArray() = default;
		virtual void EntityDestroyed(Entity entity) = 0;
	};


	template<typename T>
	class ComponentArray : public IComponentArray
	{
	public:
		ComponentArray() = default;
		virtual ~ComponentArray() = default;

		void InsertData(Entity entity, T component);
		void RemoveData(Entity entity);

		T& GetData(Entity entity);

		void EntityDestroyed(Entity entity) override;

	private:
		std::array<T, MAX_ENTITIES> mComponentArray;
		std::unordered_map<Entity, size_t> mEntityToIndexMap;
		std::unordered_map<size_t, Entity> mIndexToEntityMap;
		size_t mSize;
	};

	template<typename T>
	void ComponentArray<T>::InsertData(Entity entity, T component)
	{
		ASSERT(mEntityToIndexMap.find(entity) == mEntityToIndexMap.end());

		// Put new entry at end and update the maps
		size_t newIndex = mSize;
		mEntityToIndexMap[entity] = newIndex;
		mIndexToEntityMap[newIndex] = entity;
		mComponentArray[newIndex] = component;
		++mSize;
	}

	template<typename T>
	void ComponentArray<T>::RemoveData(Entity entity)
	{
		ASSERT(mEntityToIndexMap.find(entity) != mEntityToIndexMap.end());

		// Copy element at end into deleted element's place to maintain density
		size_t indexOfRemovedEntity = mEntityToIndexMap[entity];
		size_t indexOfLastElement = mSize - 1;
		mComponentArray[indexOfRemovedEntity] = mComponentArray[indexOfLastElement];

		// Update map to point to moved spot
		Entity entityOfLastElement = mIndexToEntityMap[indexOfLastElement];
		mEntityToIndexMap[entityOfLastElement] = indexOfRemovedEntity;
		mIndexToEntityMap[indexOfRemovedEntity] = entityOfLastElement;

		mEntityToIndexMap.erase(entity);
		mIndexToEntityMap.erase(indexOfLastElement);

		--mSize;
	}

	template<typename T>
	T& ComponentArray<T>::GetData(Entity entity)
	{
		ASSERT(mEntityToIndexMap.find(entity) != mEntityToIndexMap.end() && "Retrieving non-existent component.");

		// Return a reference to the entity's component
		return mComponentArray[mEntityToIndexMap[entity]];
	}

	template<typename T>
	void ComponentArray<T>::EntityDestroyed(Entity entity)
	{
		if (mEntityToIndexMap.find(entity) != mEntityToIndexMap.end())
		{
			// Remove the entity's component if it existed
			RemoveData(entity);
		}
	}
}