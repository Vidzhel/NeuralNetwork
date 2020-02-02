#include "Matrix.h"
#include "Constant.h"
#include <iostream>
#include <chrono>
#include "Timer.h"


int main() {
	//int f = 0;

	//for (int i = 0; i < 2; i++)
	//	for (int j = 0; j < 2; j++)
	//		for (int k = 0; k < 2; k++)
	//			std::cout << i*4+j*2+k << "\n";
	{
		Timer timer1;

		std::vector<int> dimensions{ 280, 280, 100 };
		Shape matrixShape(dimensions);

		Matrix m1 = Matrix(matrixShape, true);
		Matrix m2 = Matrix(matrixShape, true);
		Matrix m3 = Matrix(matrixShape, true);
		Matrix res(matrixShape, false);

		Operand& op = (m3 + m1 * m2).Sum() / m2;
		op.AssignTo(&res);

	}

	std::getchar();
}