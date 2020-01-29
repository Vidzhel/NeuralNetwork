#include "Matrix.h"
#include <iostream>

int Matrix::matrices = 0;

Matrix::Matrix(const Shape& shape, bool random) : Operand(shape) {
	matrices++;
	data = (float*)calloc(shape.size, sizeof(float));

	if (random) {
		std::default_random_engine generator(matrices);
		std::normal_distribution<float> distribution;

		for (int i = 0; i < shape.size; i++)
			data[i] = distribution(generator);
	}
};

Matrix::Matrix(const Shape& shape, float* data) : Operand(shape), data(data) {}

Matrix::~Matrix() {
	delete	data;
}

void Matrix::Evaluate(Context& context, std::vector<Operand*>& operands) const {
	context.AddIterable(shape);
	operands.push_back(const_cast<Matrix*>(this));
}

inline float* Matrix::GetData() const {
	return data;
}

inline int Matrix::Size() const {
	return shape.size;
}

void Matrix::Fill(float content) {
	for (int i = 0; i < shape.size; i++)
		data[i] = content;
}

void Matrix::Print() const {
	for (int i = 0; i < shape.size; i++) {
		std::cout << data[i] << " ";
	}

	std::cout << "\n";
}