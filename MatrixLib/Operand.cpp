#include "Operations.h"
#include "Operand.h"
#include "OpenGLExecuter.h"
#include "Constant.h"
#include "Timer.h"
#include <memory>

Operand::Operand(Shape shape) : shape(shape) {}
void Operand::AssignTo(Operand* operand) const {
	std::string generatedFunc;
	std::vector<Operand*> operands;

	Context ctx;

	operand->Evaluate(ctx, operands);
	Evaluate(ctx, operands);

	ctx.GenerateFile(&generatedFunc);

	// TODO Optimize
	OpenGLExecuter executer(false);
	
	// TODO Optimize
	executer.Run(&generatedFunc, operands);
}
//
//Operand& Operand::ElementwiceMultiplication(const Operand& first, const Operand& second) {
//	return *(new MultiplicationOp(first, second);
//}
//Operand& Operand::ElementwiceMultiplication(const Operand& other) const {
//	return *(new MultiplicationOp(*this, other);
//}
Operand& Operand::operator * (const Operand& other) const {
	return *(new MultiplicationOp(*this, other));
}

//
//Operand& Operand::OperandMultiplication(const Operand& first, const Operand& second) {
//	return *(new MatrixMultiplicationOp(first, second);
//}
//Operand& Operand::OperandMultiplication(const Operand& other) const {
//	return *(new MatrixMultiplicationOp(this, other);
//}
//
//template<typename T>
//static Operand& Operand::Multiplication(const Operand& operand, const T constant) {
//	// TODO if we pass int an exception will be occur as int is not Operand&
//	return *(new MultiplicationOp(this, constant);
//}
//
//template<typename T>
//Operand& Operand::Multiplication(const T constant) const {
//	return *(new MultiplicationOp(this, constant);
//}
Operand& Operand::operator * (const int constant) const {
	return *(new MultiplicationOp(*this, constant));
}
Operand& Operand::operator * (const float constant) const {
	return *(new MultiplicationOp(*this, constant));
}

//Operand& Operand::ElementwiceDivision(const Operand& first, const Operand& second) {
//	return *(new DivisionOp(first, second);
//}
//
//Operand& Operand::ElementwiceDivision(const Operand& other) const {
//	return *(new DivisionOp(*this, other);
//}

Operand& Operand::operator / (const Operand& other) const {
	return *(new DivisionOp(*this, other));
}

//
//template<typename T>
//Operand& Operand::Division(const Operand& operand, const T constant) {
//	return *(new DivisionOp(operand, constant);
//}
//
//template<typename T>
//Operand& Operand::Division(const T constant) const {
//	return *(new DivisionOp(*this, constant);
//}

Operand& Operand::operator / (const int constant) const {
	return *(new DivisionOp(*this, constant));
}
Operand& Operand::operator / (const float constant) const {
	return *(new DivisionOp(*this, constant));
}

//
//template<typename T>
//Operand& Operand::Sum(const Operand& operand, const T constant) {
//	return *(new AdditionOp(*this, constant);
//}
//template<typename T>
//Operand& Operand::Sum(const T constant) const {
//	return *(new AdditionOp(*this, *(new Constant(constant));
//}
Operand& Operand::operator + (const int constant) const {
	return *(new AdditionOp(*this, constant));
}
Operand& Operand::operator + (const float constant) const {
	return *(new AdditionOp(*this, constant));
}
//
//template<>
//Operand& Operand::Sum(const Operand& first, const Operand& second) {
//	return *(new AdditionOp(first, second);
//}
//template<>
//Operand& Operand::Sum(const Operand& other) const {
//	return *(new AdditionOp(*this, other);
//}
Operand& Operand::operator + (const Operand& other) const {
	return *(new AdditionOp(*this, other));
}

//TODO sum returns Operand& with template paramener of double (constant not matrix) 
Operand& Operand::Sum() const {
	return *(new SumOp(*this));
}
//
//Operand& Operand::Sub(const Operand& first, const Operand& second) {
//	return *(new SubtractionOp(first, second);
//}
//Operand& Operand::Sub(const Operand& other) const {
//	return *(new SubtractionOp(*this, other);
//}
Operand& Operand::operator - (const Operand& other) const {
	return *(new SubtractionOp(*this, other));
}
//
//template<typename T>
//Operand& Operand::Sub(const Operand& operand, const T constant) {
//	return *(new SubtractionOp(operand, constant);
//}
//template<typename T>
//Operand& Operand::Sub(const T constant) const {
//	return *(new SubtractionOp(*this, constant);
//}
Operand& Operand::operator - (const int constant) const {
	return *(new SubtractionOp(*this, constant));
}
Operand& Operand::operator - (const float constant) const {
	return *(new SubtractionOp(*this, constant));
}

//Operand& Operand::Map(const Operand Operand, void* (*mapFunction) (const void* const)) {
//	return nullptr;
//}
//Operand& Operand::Map(const Operand Operand, void* (*mapFunction) (const void* const, const int)) {
//	return nullptr;
//
//}
//Operand& Operand::Map(const void* (*mapFunction) (const void* const)) const {
//	return nullptr;
//
//}
//Operand& Operand::Map(const void* (*mapFunction) (const void* const, const int)) const {
//	return nullptr;
//
//}
//
//Operand& Operand::Transpose() {
//	return nullptr;
//}