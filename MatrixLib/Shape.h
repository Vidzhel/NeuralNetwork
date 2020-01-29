#pragma once
#include "Exportable.h"
#include <vector>

struct STORING_ATTR Shape
{
public:
	const std::vector<int>& dimensionSizes;
	const int size;

	Shape(std::vector<int>& dimensionSizes);
	Shape();
	inline std::size_t GetDimentionsCount() const { return dimensionSizes.size(); }
	inline bool Compare(const Shape& other) const { return other.dimensionSizes == dimensionSizes; }
	bool operator == (const Shape& other) const { return other.dimensionSizes == dimensionSizes; }
	bool operator != (const Shape& other) const { return other.dimensionSizes != dimensionSizes; }

private:
	int Size() const;
};
