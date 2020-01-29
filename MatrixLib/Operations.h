#pragma once
#include "Operand.h"
#include "Context.h"
#include "Exportable.h"

class STORING_ATTR OperationNode : public Operand
{
public:
	using Operand::Operand;
	virtual void Apply(Context& context, std::vector<Operand*>& operands) const = 0;
	float* GetData() const override;
};

class STORING_ATTR BinaryOperation : public OperationNode
{
	bool _deleteRightOp;
	Operand* deletePointer;

	const Operand& LeftOp;
	const Operand& RightOP;

public:
	BinaryOperation(const Operand& leftOp, const Operand& rightOp);
	BinaryOperation(const Operand& leftOp, const float constant);
	~BinaryOperation() override;

	void Evaluate(Context& context, std::vector<Operand*>& operands) const override;
	int Size() const override;
};

class STORING_ATTR AdditionOp : public BinaryOperation
{
public:
	AdditionOp(const Operand& leftOp, const Operand& rightOp);
	AdditionOp(const Operand& leftOp, const float constant);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;
};

class STORING_ATTR SubtractionOp : public BinaryOperation
{
public:
	SubtractionOp(const Operand& leftOp, const Operand& rightOp);
	SubtractionOp(const Operand& leftOp, const float constant);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;

};

class STORING_ATTR MultiplicationOp : public BinaryOperation
{
public:
	MultiplicationOp(const Operand& leftOp, const Operand& rightOp);
	MultiplicationOp(const Operand& leftOp, const float constant);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;

};

//class STORING_ATTR MatrixMultiplicationOp : public BinaryOperation
//{
//public:
//	MatrixMultiplicationOp(const Operand &leftOp, const Operand &rightOp);
//	void Apply(Context& context) const override;
//
//};

class STORING_ATTR DivisionOp : public BinaryOperation
{
public:
	DivisionOp(const Operand& leftOp, const Operand& rightOp);
	DivisionOp(const Operand& leftOp, const float constant);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;

};

class STORING_ATTR SingularOperation : public OperationNode
{
protected:
	const Operand& operand;
public:
	SingularOperation(const Operand& operand);

	void Evaluate(Context& context, std::vector<Operand*>& operands) const override;
	int Size() const override;
};

class STORING_ATTR ElelmentsSumOp : public SingularOperation
{
public:
	ElelmentsSumOp(const Operand& operand);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;

};

class STORING_ATTR MapOp : public SingularOperation
{
public:
	MapOp(const Operand& operand);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;

};

class STORING_ATTR SumOp : public SingularOperation
{
public:
	SumOp(const Operand& operand);
	void Apply(Context& context, std::vector<Operand*>& operands) const override;

};