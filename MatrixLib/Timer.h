#pragma once
#include <chrono>

class Timer {
	static int number;
	std::chrono::time_point<std::chrono::high_resolution_clock> m_StartTimepoing;
	int m_num;
	bool m_stoped;

public:
	Timer();
	~Timer();
	void Stop();
};
