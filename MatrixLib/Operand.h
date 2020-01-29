#pragma once
#include <vector>
#include <string>
#include "Exportable.h"
#include "Context.h"

class STORING_ATTR Operand{
public:
	Shape shape;

	Operand(Shape shape);
	Operand() = default;
	virtual ~Operand() {}

	virtual void Evaluate(Context& context, std::vector<Operand*>& operands) const = 0;
	void AssignTo(Operand* operand) const;
	virtual int Size() const = 0;
	virtual float* GetData() const = 0;

	//static Operand* ElementwiceMultiplication(const Operand& first, const Operand& second);
	//Operand* ElementwiceMultiplication(const Operand& other) const;

	//static Operand& OperandMultiplication(const Operand& first, const Operand& second);
	//Operand& OperandMultiplication(const Operand& other) const;
	Operand& operator * (const Operand& other) const;

	//template<typename T>
	//static Operand* Multiplication(const Operand& operand, const T constant);
	//template<typename T>
	//Operand* Multiplication(const T constant) const;
	Operand& operator * (const int other) const;
	Operand& operator * (const float other) const;

	//static Operand* ElementwiceDivision(const Operand& first, const Operand& second);
	//Operand* ElementwiceDivision(const Operand& other) const;
	Operand& operator / (const Operand& other) const;

	//template<typename T>
	//static Operand* Division(const Operand& operand, const T constant);
	//template<typename T>
	//Operand* Division(const T constant) const;
	Operand& operator / (const int constant) const;
	Operand& operator / (const float constant) const;

	//template<typename T>
	//static Operand* Sum(const Operand& operand, const T constant);
	//template<typename T>
	//Operand* Sum(const T contant) const;
	Operand& operator + (const int constant) const;
	Operand& operator + (const float constant) const;

	//static Operand* Sum(const Operand& first, const Operand& second);
	//Operand* Sum(const Operand& other) const;
	Operand& operator + (const Operand& other) const;

	//TODO sum returns Operand& with template paramener of double (constant not matrix) 
	Operand& Sum() const;

	//static Operand* Sub(const Operand& first, const Operand& second);
	//Operand* Sub(const Operand& other) const;
	Operand& operator - (const Operand& other) const;

	//template<typename T>
	//static Operand* Sub(const Operand& operand, const T constant);
	//template<typename T>
	//Operand* Sub(const T constant) const;
	Operand& operator - (const int constant) const;
	Operand& operator - (const float constant) const;

	/*tatic Operand& Map(const Operand& Operand, void*(*mapFunction) (const void* const));
	static Operand& Map(const Operand& Operand, void*(*mapFunction) (const void* const, const int));
	Operand& Map(const void* (*mapFunction) (const void* const)) const;
	Operand& Map(const void* (*mapFunction) (const void* const, const int)) const;

	Operand& Transpose();*/

};