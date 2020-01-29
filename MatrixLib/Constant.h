#pragma once
#include "Operand.h"
#include "Exportable.h"
#include "Shape.h"

class STORING_ATTR Constant : public Operand {
public:
	float value;

	Constant(int value) : Operand() {
		this->value = static_cast<float>(value);
	};
	Constant(float value) : Operand() {
		this->value = value;
	};
	Constant(double value) : Operand() {
		this->value = static_cast<float>(value);
	};
	Constant() = default;

	inline void Evaluate(Context& context, std::vector<Operand*>& operands) const override {
		context.AddConstant();
		operands.push_back(const_cast<Constant*>(this));
	}

	inline int Size() const override {
		return 1;
	}

	inline float* GetData() const override {
		return const_cast<float*>(&value);
	}

};