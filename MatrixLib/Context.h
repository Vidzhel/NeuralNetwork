#pragma once
#include "Shape.h"
#include "Exportable.h"
#include <vector>
#include <stack>
#include <string>

class STORING_ATTR Context {
private:
	std::stack<std::string> evaluationStack;
	std::vector<std::string> globalVariables;
	std::string freeGlobalVariable;
	std::vector<std::string> localVariables;
	std::stack<std::string> freeLocalVariables;
	std::vector<std::string> params;
	std::vector<std::string> derectives;
	const Shape* currentLoop;
	std::vector<std::pair<std::string, std::string>> loopVariables;

	int variablesCount;

public:

	Context();
	void AddParam(std::string& appendix);
	void AddIterable(const Shape& iterableShape);
	void AddConstant();
	void AddBinOp(const char* action);
	void AddSingOp(const char* action);
	void AddBinOp(std::string& action);
	void AddSingOp(std::string& action);
	void Swap();
	void CreateOrGetGlobalVariable(std::string* stringOut);
	void GenerateFile(std::string* output);
	void CloseLoop();

private:
	void createVariable(std::string& appendix, std::string* stringOut);
	void createOrGetLocalVariable(std::string* stringOut);
	void createLoop(const Shape& loopShape);
	void getLoopElementAccess(std::string* stringOut);
	void getVariable(std::string&);
	void initVariable(std::string& variable);
	bool isLocalVar(std::string& variable);
	bool isGlobalVar(std::string& variable);
	void freeLocalVar();
	bool isArray(std::string& variable);
};