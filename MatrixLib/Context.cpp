#include "Context.h"

std::string localVarAppendix("L_");
std::string globalVarAppendix("G_");
std::string loopVarAppendix("L_L");
std::string arrayVarAppendix("A_");
std::string constantVarAppendix("C_");
//std::string iterableVarAppendix("I_");

Context::Context() : variablesCount(0), currentLoop(nullptr) {}

void Context::AddParam(std::string& appendix) {
	// set type
	std::string variable;
	createVariable(appendix, &variable);
	params.push_back(variable);
}

void Context::AddIterable(const Shape& iterableShape) {
	AddParam(arrayVarAppendix);
	if (currentLoop == nullptr)
		createLoop(iterableShape);

	evaluationStack.push(params.back());
}

void Context::AddConstant() {
	AddParam(constantVarAppendix);
	// As we add constant as a pointer, we need to add * to use its value
	evaluationStack.push("*" + params.back());
}

void Context::AddBinOp(std::string& op) {
	AddBinOp(op.c_str());
}

void Context::AddBinOp(const char* op) {
	std::string rightOp;
	getVariable(rightOp);
	std::string leftOp;
	getVariable(leftOp);
	std::string evaluate;

	if (!(isLocalVar(leftOp) || isLocalVar(rightOp)) && !freeLocalVariables.empty())
		freeLocalVariables.pop();

	if (isArray(leftOp))
	{
		std::string elementAccessPart;
		getLoopElementAccess(&elementAccessPart);
		leftOp += elementAccessPart;
	}
	if (isArray(rightOp))
	{
		std::string elementAccessPart;
		getLoopElementAccess(&elementAccessPart);
		rightOp += elementAccessPart;
	}

	if ((isGlobalVar(leftOp) || isGlobalVar(rightOp)) && !freeGlobalVariable.empty()) {
		globalVariables.push_back(freeGlobalVariable);
		evaluate = freeGlobalVariable;
		evaluationStack.push(evaluate);
		freeGlobalVariable = "";
	}
	else
		createOrGetLocalVariable(&evaluate);


	derectives.push_back(evaluate + " = " + leftOp + op + rightOp + ";");
}

bool Context::isLocalVar(std::string& variable) {
	for (auto ptr = localVariables.begin(); ptr != localVariables.end(); ptr++) {
		if (ptr->compare(variable) == 0)
			return true;
	}

	return false;
}

bool Context::isGlobalVar(std::string& variable) {
	if (*variable.c_str() == 'G')
		return true;

	return false;
}

void Context::AddSingOp(std::string& op) {
	AddSingOp(op.c_str());
}

void Context::AddSingOp(const char* op) {
	std::string rightOp;
	getVariable(rightOp);
	std::string leftOp;
	getVariable(leftOp);
	evaluationStack.push(leftOp);

	if (isArray(leftOp))
	{
		std::string elementAccessPart;
		getLoopElementAccess(&elementAccessPart);
		leftOp += elementAccessPart;
	}
	if (isArray(rightOp))
	{
		std::string elementAccessPart;
		getLoopElementAccess(&elementAccessPart);
		rightOp += elementAccessPart;
	}

	derectives.push_back(leftOp + op + rightOp + ";");
}

void Context::GenerateFile(std::string* output) {
	//Assign result to output var
	AddSingOp("=");

	*output = std::string("__kernel void executable(");

	for (int i = 0; i < params.size(); i++) {
		*output += "__global float* " + params[i];

		if (i != params.size() - 1)
			*output += ", ";
	}

	*output += "){\n";

	for (auto elemPtr = globalVariables.begin(); elemPtr != globalVariables.end(); elemPtr++)
		*output += "float " + *elemPtr + " = 0;\n";

	for (auto elemPtr = localVariables.begin(); elemPtr != localVariables.end(); elemPtr++)
		*output += "float " + *elemPtr + ";\n";

	if (currentLoop != nullptr)
		CloseLoop();

	for (int i = 0; i < derectives.size(); i++)
		*output += derectives[i] + "\n";

	*output += "}";
}

void Context::CloseLoop() {
	derectives.push_back("}");
	freeLocalVar();
	currentLoop = nullptr;
}

void Context::Swap() {
	auto temp = evaluationStack.top();
	evaluationStack.pop();
	auto temp2 = evaluationStack.top();
	evaluationStack.pop();

	evaluationStack.push(temp);
	evaluationStack.push(temp2);
}

inline void Context::getVariable(std::string& varOut) {
	varOut = evaluationStack.top();
	evaluationStack.pop();
}

void Context::createVariable(std::string& appendix, std::string* stringOut) {
	*stringOut = appendix + std::to_string(variablesCount);
	variablesCount++;
}

void Context::createOrGetLocalVariable(std::string* stringOut) {
	if (freeLocalVariables.empty())
		if (freeGlobalVariable.empty())
		{
			createVariable(localVarAppendix, stringOut);
			//initVariable(*stringOut);
			localVariables.push_back(*stringOut);
			freeLocalVariables.push(*stringOut);
		}
		else
			*stringOut = freeGlobalVariable;
	else
		*stringOut = freeLocalVariables.top();

	evaluationStack.push(*stringOut);
}

void Context::CreateOrGetGlobalVariable(std::string* stringOut) {
	if (freeGlobalVariable.empty()) {
		createVariable(globalVarAppendix, stringOut);
		//initVariable(*stringOut);
		freeGlobalVariable = *stringOut;
	}

	evaluationStack.push(*stringOut);
}

void Context::initVariable(std::string& variable) {
	std::string derective = "float " + variable + ";";
	derectives.push_back(derective);
}

void Context::createLoop(const Shape& loopShape) {

	if (currentLoop != nullptr) {
		derectives.push_back("}");
		freeLocalVar();
	}

	loopVariables.clear();
	currentLoop = &loopShape;

	int elementsCount = 1;
	for (std::size_t i = 0; i < loopShape.dimensionSizes.size(); i++) {
		elementsCount = 1;

		for (int j = i + 1; j < loopShape.dimensionSizes.size(); j++)
			elementsCount *= loopShape.dimensionSizes[j];

		std::string variable;
		createVariable(loopVarAppendix, &variable);
		loopVariables.push_back(std::pair<std::string, std::string >(variable, std::to_string(elementsCount)));
		derectives.push_back("for(int " + variable + " = 0; " + variable + " < " + std::to_string(loopShape.dimensionSizes[i]) + "; " + variable + "++)");
	}

	derectives.push_back("{");
}

void Context::freeLocalVar() {
	while (!freeLocalVariables.empty())
		freeLocalVariables.pop();

	for (auto elementPtr = localVariables.begin(); elementPtr != localVariables.end(); elementPtr++) {
		freeLocalVariables.push(*elementPtr);
	}
}

void Context::getLoopElementAccess(std::string* out) {

	*out = "[";

	for (std::size_t i = 0; i < loopVariables.size() - 1; i++)
		*out += loopVariables[i].first + "*" + loopVariables[i].second + " + ";

	*out += loopVariables[loopVariables.size() - 1].first;
	*out += "]";
}

bool Context::isArray(std::string& variable) {
	if (*variable.c_str() == 'A')
		return true;

	return false;
}