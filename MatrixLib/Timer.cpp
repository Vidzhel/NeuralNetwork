#include "Timer.h"
#include <iostream>

int Timer::number = 0;

Timer::Timer()
{
	m_stoped = false;
	m_num = ++number;

	m_StartTimepoing = std::chrono::high_resolution_clock::now();
}

Timer::~Timer() {
	Stop();
}

void Timer::Stop() {
	if (m_stoped)
		return;

	auto endTimepoint = std::chrono::high_resolution_clock::now();

	auto duration = endTimepoint - m_StartTimepoing;
	auto microSec = std::chrono::duration_cast<std::chrono::microseconds>(duration).count();
	double ms = microSec * 0.001;
	double s = microSec * 0.000001;

	//auto start = std::chrono::time_point_cast<std::chrono::microseconds>(m_StartTimepoing).time_since_epoch().count();
	//auto end = std::chrono::time_point_cast<std::chrono::microseconds>(endTimepoint).time_since_epoch().count();

	//auto duration = end - end;

	std::cout << "\n\n" << m_num << ") " << microSec << "micro s ( " << ms << "ms, " << s << "s )";
	m_stoped = true;
}
