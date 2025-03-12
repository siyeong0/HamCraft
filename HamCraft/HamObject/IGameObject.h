#pragma once
#include "../Common/Common.h"

namespace ham
{
	class IGameObject
	{
	public:
		IGameObject() = default;
		virtual ~IGameObject() = default;

		virtual bool Initialize() = 0;
		virtual void Finalize() = 0;

		virtual void Update(float dt) = 0;

	private:
		// TODO: GUIDÃß°¡
	};
}