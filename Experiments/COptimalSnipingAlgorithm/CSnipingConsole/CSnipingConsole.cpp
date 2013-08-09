// CSnipingConsole.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "SnipingDistanceCalculator.h"
#include <iostream>
#include <Windows.h>

int _tmain(int argc, _TCHAR* argv[])
{
	int repetitions = 10000000;
	std::cout << "Testing " << repetitions << " times" << std::endl << std::endl;
	LARGE_INTEGER start, end, freq;
	
	QueryPerformanceFrequency(&freq);
	QueryPerformanceCounter(&start); 
	
	for (int i = 0; i < repetitions; i++)
	{
		bool walls[32] = {0,0,0,0,1,0,0,1,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0};
		int ticksTaken = SnipingDistanceCalculator::Calculate(walls, 32);
		if (!i)
		{
			std::cout << "Turns required to shoot the target: " << ticksTaken << std::endl;
		}
	}

	QueryPerformanceCounter(&end);
    
	std::cout << std::endl << "The resolution of this timer is: " << freq.QuadPart << " Hz." << std::endl;
	std::cout << "Time to calculate: " 
			<< (end.QuadPart - start.QuadPart) * 1000000 / freq.QuadPart 
			<< " microseconds" << std::endl;
	std::cout << "                 : " 
			<< (double) (end.QuadPart - start.QuadPart) / freq.QuadPart 
			<< " seconds" << std::endl;
}

