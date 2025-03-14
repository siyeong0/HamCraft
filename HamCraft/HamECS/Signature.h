#pragma once
#include <bitset>

#include "../Common/Common.h"
#include "Component.h"

namespace ham
{
	// 컴포넌트 종류(Transform, RigidBody, ...)  하나는 하나의 Index를 가짐. 각 인덱스를 비트에 매핑
	// Transform 0, RigidBody 1, SpriteRenderer 2 ... 일때 
	// Transform, SpriteRenderer을 갖는 엔티티의 시그니처는 0b101
	using Signature = std::bitset<MAX_COMPONENTS>;
}