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

		std::vector<int> dimensions{ 28, 28, 10};
		Shape matrixShape(dimensions);

		Matrix m1 = Matrix(matrixShape, true);
		//m1.Fill(1);
		//m1.Print();
		Matrix m2 = Matrix(matrixShape, true);
		//m2.Fill(2);
		//m2.Print();
		Matrix m3 = Matrix(matrixShape, true);
		//m3.Fill(0);
		//m3.Print();
		Matrix res(matrixShape, false);

		{
			//Timer timer2;
			//Operand& op = ((m1 * 10)-m2*2+m3).Sum();
			Operand& op = m3 * m1 * m2;
			//timer2.Stop();
			//Constant res;
			//res.value = 10;
			//Timer timer3;
			op.AssignTo(&res);
			//std::cout << res.value;
		}
		//res.Print();
	}

	std::getchar();
}