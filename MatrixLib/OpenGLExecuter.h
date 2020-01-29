#pragma once

#define CL_HPP_TARGET_OPENCL_VERSION 120
#define CL_USE_DEPRECATED_OPENCL_1_2_APIS

#ifdef __APPLE__
#include <OpenCL/opencl.hpp>
#else
#include <CL/cl.hpp>
#endif 

#include <string>
#include <vector>
#include "Operand.h"

class OpenGLExecuter {
private:
	cl::Context* context;
	cl::Device device;
	
	cl::CommandQueue* command_queue;

public:
	OpenGLExecuter(bool use_gpu);
	~OpenGLExecuter();
	void Run(std::string* programSrc, std::vector<Operand*> operands);

private:
	void execute(std::string* programSrc, cl::Program* programOut);
	void prepareBuffer(cl::Program& program, std::vector<Operand*> operands);
};