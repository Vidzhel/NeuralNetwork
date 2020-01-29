#include "Operand.h"
#include "Constant.h"
#include "Operations.h"
#include <algorithm>

float* OperationNode::GetData() const {
	return nullptr;
}

BinaryOperation::BinaryOperation(const Operand& leftOp, const Operand& rightOp) : OperationNode(leftOp.shape), LeftOp(leftOp), RightOP(rightOp) {
	if (leftOp.shape != rightOp.shape)
		throw "Operands doesn't have same shapes";
}

BinaryOperation::BinaryOperation(const Operand& leftOp, const float constant) : OperationNode(leftOp.shape), deletePointer(new Constant(constant)), LeftOp(leftOp), RightOP(static_cast<Operand&>(*deletePointer)) {
	_deleteRightOp = true;
}

BinaryOperation::~BinaryOperation() {
	if (_deleteRightOp)
		delete deletePointer;
}

void BinaryOperation::Evaluate(Context& context, std::vector<Operand*>& operands) const{
	LeftOp.Evaluate(context, operands);
	RightOP.Evaluate(context, operands);
	Apply(context, operands);
}

int BinaryOperation::Size() const {
	return std::max(LeftOp.Size(), RightOP.Size());
}

AdditionOp::AdditionOp(const Operand& leftOp, const Operand& rightOp) : BinaryOperation(leftOp, rightOp) {}
AdditionOp::AdditionOp(const Operand& leftOp, const float constant) : BinaryOperation(leftOp, constant) {}
void AdditionOp::Apply(Context& context, std::vector<Operand*>& operands) const {
	context.AddBinOp("+");
}


SubtractionOp::SubtractionOp(const Operand& leftOp, const Operand& rightOp) : BinaryOperation(leftOp, rightOp) {}
SubtractionOp::SubtractionOp(const Operand& leftOp, const float constant) : BinaryOperation(leftOp, constant) {}
void SubtractionOp::Apply(Context& context, std::vector<Operand*>& operands) const {
	context.AddBinOp("-");
}

MultiplicationOp::MultiplicationOp(const Operand& leftOp, const Operand& rightOp) : BinaryOperation(leftOp, rightOp) {}
MultiplicationOp::MultiplicationOp(const Operand& leftOp, const float constant) : BinaryOperation(leftOp, constant) {}
void MultiplicationOp::Apply(Context& context, std::vector<Operand*>& operands) const {
	context.AddBinOp("*");
}

//MatrixMultiplicationOp::MatrixMultiplicationOp(const Operand& leftOp, const Operand& rightOp) : BinaryOperation(leftOp, rightOp) {}
//void MatrixMultiplicationOp::Apply(Context& context) const {
//	context.AddAction("-");
//}

DivisionOp::DivisionOp(const Operand& leftOp, const Operand& rightOp) : BinaryOperation(leftOp, rightOp) {}
DivisionOp::DivisionOp(const Operand& leftOp, const float constant) : BinaryOperation(leftOp, constant) {}
void DivisionOp::Apply(Context& context, std::vector<Operand*>& operands) const {
	context.AddBinOp("/");
}

SingularOperation::SingularOperation(const Operand& operand) : OperationNode(operand.shape), operand(operand) {}

void SingularOperation::Evaluate(Context& context, std::vector<Operand*>& operands) const {
	operand.Evaluate(context, operands);
	Apply(context, operands);
}

int SingularOperation::Size() const {
	return operand.Size();
}

ElelmentsSumOp::ElelmentsSumOp(const Operand& operand) : SingularOperation(operand) {}
void ElelmentsSumOp::Apply(Context& context, std::vector<Operand*>& operands) const {

}

MapOp::MapOp(const Operand& operand) : SingularOperation(operand) {}
void MapOp::Apply(Context& context, std::vector<Operand*>& operands) const {

}

SumOp::SumOp(const Operand& operand) : SingularOperation(operand) {}
void SumOp::Apply(Context& context, std::vector<Operand*>& operands) const {
	std::string variable;
	context.CreateOrGetGlobalVariable(&variable);
	context.Swap();
	context.AddBinOp("+");
	context.CloseLoop();
}