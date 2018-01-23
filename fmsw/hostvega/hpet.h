
#include <Windows.h>

#pragma managed(push, off)

namespace hpet
{
	void AddEvent(HANDLE Event);
	void SetGuard(int msec);
}

#pragma managed(pop)