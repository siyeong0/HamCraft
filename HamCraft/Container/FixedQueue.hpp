#pragma once
#include <iostream>
#include <iomanip>

#include "../Common/Common.h"

namespace ham
{
	template <typename T, size_t capacity>
	class FixedQueue
	{
	public:
		FixedQueue()
			: mDataArr(nullptr)
			, mFrontIdx(0)
			, mRearIdx(0)
			, CAPACITY(capacity)
			, mSize(0)
		{
			mDataArr = Alloc<T, capacity>();
			ASSERT(mDataArr != nullptr);
		}

		~FixedQueue()
		{
			Free<T, capacity>(mDataArr);
			mDataArr = nullptr;
		}

		FixedQueue(const FixedQueue& rhs)
		{
			this.mDataArr = Alloc<T, CAPACITY>();
			ASSERT(mDataArr != nullptr);

			std::memcpy(this.mDataArr, rhs.mDataArr, sizeof(T) * CAPACITY);
			this.mFrontIdx = rhs.mFrontIdx;
			this.mRearIdx = rhs.mRearIdx;
			this.CAPACITY = rhs.CAPACITY;
			this.mSize = rhs.mSize;
		}

		FixedQueue(const FixedQueue&& rhs)
		{
			this.mDataArr = rhs.mDataArr;
			this.mFrontIdx = rhs.mFrontIdx;
			this.mRearIdx = rhs.mRearIdx;
			this.CAPACITY = rhs.CAPACITY;
			this.mSize = rhs.mSize;

			rhs.mDataArr = nullptr;
		}

		void Push(const T& val)
		{
			mDataArr[mRearIdx] = val;
			mRearIdx = calcCircularIdx(mRearIdx + 1);
			mSize++;

			ASSERT(mSize <= CAPACITY);
		}

		T Pop()
		{
			ASSERT(mSize > 0);

			T rv = mDataArr[mFrontIdx];
			std::memset(&mDataArr[mFrontIdx], 0, sizeof(T));
			mFrontIdx = calcCircularIdx(mFrontIdx + 1);
			mSize--;

			return rv;
		}

		const T& GetFront() 
		{
			ASSERT(mSize > 0);
			return mDataArr[mFrontIdx];
		};

		bool IsEmpty() { return mSize == 0; }
		size_t GetSize() { return mSize; }
		
		void Print()
		{
			std::cout << "\n#########################################" << std::endl;
			for (int i = 0; i < CAPACITY; i++)
			{
				std::cout.width(4);
				std::cout << std::right << mDataArr[calcCircularIdx(mFrontIdx + i)];
			}
			std::cout << "\n#########################################" << std::endl;
		}

	private:
		int calcCircularIdx(const int idx)
		{
			ASSERT(idx + mSize >= 0);
			int iCap = static_cast<int>(CAPACITY);
			return (idx + iCap) % iCap;
		}

	private:
		T* mDataArr;
		int mFrontIdx;
		int mRearIdx;

		const size_t CAPACITY;
		size_t mSize;
	};
}