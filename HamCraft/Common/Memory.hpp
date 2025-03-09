#pragma once
#include "Assert.h"

// TODO: malloc을 커스텀 메모리 관리자로 변경

inline void* Alloc(size_t size)
{
	return malloc(size);
}

inline void Free(void* ptr)
{
	free(ptr);
}

template<typename T, typename... Args>
inline T* Alloc(Args&&... args)
{
	T* ptr = static_cast<T*>(Alloc(sizeof(T)));	// TODO: malloc을 커스텀 메모리 관리자로 변경
	ASSERT(ptr != nullptr);
	new(ptr) T(std::forward<Args>(args)...);		// **placement new	p에 할당된 메모리에 생성자 호출

	return ptr;
}

template<typename T, size_t numElement, typename... Args>
inline T* Alloc(Args&&... args)
{
	T* ptr = static_cast<T*>(Alloc(sizeof(T) * numElement));
	ASSERT(ptr != nullptr);
	for (int i = 0; i < numElement; i++)
		new(ptr + i) T(std::forward<Args>(args)...);

	return ptr;
}

template<typename T>
inline void Free(T* ptr)
{
	ASSERT(ptr != nullptr);

	ptr->~T();
	Free(ptr);

	return;
}

template<typename T, size_t numElement>
inline void Free(T* ptr)
{
	ASSERT(ptr != nullptr);

	for (int i = 0; i < numElement; i++)
		(ptr+i)->~T();
	Free(ptr);

	return;
}