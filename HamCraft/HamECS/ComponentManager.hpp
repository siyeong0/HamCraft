#pragma once
#include <unordered_map>

#include "../Common/Common.h"
#include "Entity.h"
#include "Component.h"
#include "ComponentArray.hpp"

namespace ham
{
	class ComponentManager
	{
	public:
		template<typename T>
		void RegisterComponent();

		template<typename T>
		ComponentType GetComponentType();

		template<typename T>
		void AddComponent(Entity entity, T component);

		template<typename T>
		void RemoveComponent(Entity entity);

		template<typename T>
		T& GetComponent(Entity entity);

		void EntityDestroyed(Entity entity);

	private:
		template<typename T>
		std::shared_ptr<ComponentArray<T>> GetComponentArray();

	private:
		std::unordered_map<const char*, ComponentType> mComponentTypes{};
		std::unordered_map<const char*, std::shared_ptr<IComponentArray>> mComponentArrays{};
		ComponentType mNextComponentType{};
	};

	template<typename T>
	void ComponentManager::RegisterComponent()
	{
		const char* typeName = typeid(T).name();

		ASSERT(mComponentTypes.find(typeName) == mComponentTypes.end());

		mComponentTypes.insert({ typeName, mNextComponentType });
		mComponentArrays.insert({ typeName, std::make_shared<ComponentArray<T>>() });

		++mNextComponentType;
	}

	template<typename T>
	ComponentType ComponentManager::GetComponentType()
	{
		const char* typeName = typeid(T).name();
		ASSERT(mComponentTypes.find(typeName) != mComponentTypes.end() && "Component not registered before use.");

		return mComponentTypes[typeName];
	}

	template<typename T>
	void ComponentManager::AddComponent(Entity entity, T component)
	{
		GetComponentArray<T>()->InsertData(entity, component);
	}

	template<typename T>
	void ComponentManager::RemoveComponent(Entity entity)
	{
		GetComponentArray<T>()->RemoveData(entity);
	}

	template<typename T>
	T& ComponentManager::GetComponent(Entity entity)
	{
		return GetComponentArray<T>()->GetData(entity);
	}

	void ComponentManager::EntityDestroyed(Entity entity)
	{
		for (auto const& pair : mComponentArrays)
		{
			auto const& component = pair.second;
			component->EntityDestroyed(entity);
		}
	}

	template<typename T>
	std::shared_ptr<ComponentArray<T>> ComponentManager::GetComponentArray()
	{
		const char* typeName = typeid(T).name();

		ASSERT(mComponentTypes.find(typeName) != mComponentTypes.end());

		return std::static_pointer_cast<ComponentArray<T>>(mComponentArrays[typeName]);
	}
}