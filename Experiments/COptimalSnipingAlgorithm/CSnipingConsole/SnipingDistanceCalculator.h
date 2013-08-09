#pragma once
class SnipingDistanceCalculator
{
public:
	SnipingDistanceCalculator(void);
	~SnipingDistanceCalculator(void);

	static int Calculate(bool walls[], int wallLength);
};

