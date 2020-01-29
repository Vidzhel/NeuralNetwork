// Defines an matrix class with some opeartions
#pragma once

#include "Operand.h"
#include "Exportable.h"
#include <random>

class STORING_ATTR Matrix : public Operand {
	static int matrices;
public:
	float* data;

	Matrix(const Shape& shape, bool random);

	Matrix(const Shape& shape, float* data);
	~Matrix();

	void Evaluate(Context& context, std::vector<Operand*>& operands) const override;
	void Fill(float content);
	void Print() const;

	inline float* GetData() const override;

	inline int Size() const override;
}; 