#pragma once
#include "Assert.h"

class BaseAllocator
{
public:
	static void* Alloc(size_t size)
	{
		return malloc(size);
	}
	static void Free(void* ptr)
	{
		free(ptr);
	}
};

template<typename T, typename... Args>
inline T* Alloc(Args&&... args)
{
	T* ptr = static_cast<T*>(BaseAllocator::Alloc(sizeof(T)));	// TODO: malloc을 커스텀 메모리 관리자로 변경
	ASSERT(ptr != nullptr);
	new(ptr) T(std::forward<Args>(args)...);		// **placement new	p에 할당된 메모리에 생성자 호출

	return ptr;
}

template<typename T>
inline void Free(T* ptr)
{
	ASSERT(ptr != nullptr);

	ptr->~T();
	BaseAllocator::Free(ptr);

	return;
}