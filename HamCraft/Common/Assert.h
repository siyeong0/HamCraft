#pragma once
#include <iostream>

#ifdef _DEBUG
#define ASSERT(exp, ...)                                                        \
{                                                                               \
    if (!(exp))                                                                 \
    {                                                                           \
        std::cout << "================================================================================\n";  \
        std::cout << "Assertion failed: " << #exp << "\n";	                    \
        std::cout << "at [" << __FILE__ << "] (" << __func__ << ")\n";          \
        std::cout << "line " << __LINE__ << "\n";                               \
        std::cout << "================================================================================\n";  \
        __debugbreak();                                                         \
    }                                                                           \
}
#else
#define ASSERT(exp, ...) __assume(exp);
#endif