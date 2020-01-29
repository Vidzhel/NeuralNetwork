#include "Shape.h"

Shape::Shape(std::vector<int>& dimensionSizes) : dimensionSizes(dimensionSizes), size(Size()) {}
Shape::Shape() : dimensionSizes(std::vector<int>()), size(0) {}
int Shape::Size() const {
	int size = 1;

	for (std::size_t i = 0; i < dimensionSizes.size(); i++)
		size *= dimensionSizes[i];

	return size;
}
