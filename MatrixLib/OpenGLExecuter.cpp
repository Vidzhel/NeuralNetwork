#include "OpenGLExecuter.h"
#include "Timer.h"
#include <iostream>

OpenGLExecuter::OpenGLExecuter(bool use_gpu) {
	cl::Platform platform;
	cl::Platform::get(&platform);

	std::vector<cl::Device> devices;
	platform.getDevices(use_gpu ? CL_DEVICE_TYPE_GPU : CL_DEVICE_TYPE_CPU, &devices);

	device = devices.front();
	context = new cl::Context(device);

	command_queue = new cl::CommandQueue(*context, device);
}
OpenGLExecuter::~OpenGLExecuter() {
	delete command_queue;
}

void OpenGLExecuter::Run(std::string* programSrc, std::vector<Operand*> operands) {
	cl::Program program;
	execute(programSrc, &program);

	prepareBuffer(program, operands);

}

void OpenGLExecuter::execute(std::string* programSrc, cl::Program* programOut) {
	cl_int err;

	cl::Program::Sources sources(1, std::make_pair(programSrc->c_str(), programSrc->length() + 1));
	*programOut = cl::Program(*context, sources, &err);

	err != 0 ? throw("OpenCL Error") : 0;

	err = programOut->build("-cl-std=CL1.2");

	if (err != 0) {
		cl_build_status status = programOut->getBuildInfo<CL_PROGRAM_BUILD_STATUS>(device);

		if (status != CL_BUILD_ERROR)
			throw("OpenCL Error");

		std::string name = device.getInfo<CL_DEVICE_NAME>();
		std::string buildlog = programOut->getBuildInfo<CL_PROGRAM_BUILD_LOG>(device);

		throw("OpenCL Error");
	}

}

void OpenGLExecuter::prepareBuffer(cl::Program& program, std::vector<Operand*> operands) {
	std::vector<cl::Buffer> buffers;
	cl_int err;

	Timer timer1;

	cl::Buffer outBuff(*context, CL_MEM_HOST_READ_ONLY | CL_MEM_WRITE_ONLY | CL_MEM_ALLOC_HOST_PTR | CL_MEM_COPY_HOST_PTR,
		sizeof(float) * (operands.front()->Size()),
		operands[0]->GetData(), &err);
	err != 0 ? throw("OpenCL Error") : 0;

	buffers.push_back(outBuff);
	for (int i = 1; i < operands.size(); i++) {
		cl::Buffer inBuff(*context, CL_MEM_HOST_NO_ACCESS | CL_MEM_READ_ONLY | CL_MEM_ALLOC_HOST_PTR | CL_MEM_COPY_HOST_PTR, sizeof(float) * (operands[i]->Size()), operands[i]->GetData());
		err != 0 ? throw("OpenCL Error") : 0;
		buffers.push_back(inBuff);
	}
	timer1.Stop();
	Timer timer2;

	cl::Kernel kernel(program, "executable");

	for (int i = 0; i < buffers.size(); i++) {
		err = kernel.setArg(i, buffers[i]);
		err != 0 ? throw("OpenCL Error") : 0;
	}
	timer2.Stop();
	//auto mem = command_queue->enqueueMapBuffer(outBuff, CL_TRUE, CL_MAP_READ, 0, sizeof(float) * (operands.front()->Size()), nullptr, nullptr, &err);
	//err != 0 ? throw("OpenCL Error") : 0;

	Timer timer3;
	err = command_queue->enqueueNDRangeKernel(kernel, cl::NullRange, cl::NDRange(1024), cl::NDRange(16));
	err != 0 ? throw("OpenCL Error") : 0;

	timer3.Stop();
	//err = command_queue->enqueueUnmapMemObject(outBuff, mem, nullptr, nullptr);
	//err != 0 ? throw("OpenCL Error") : 0;

	Timer timer4;
	err = command_queue->enqueueReadBuffer(outBuff, CL_TRUE, 0, sizeof(float) * operands[0]->Size(), operands[0]->GetData());
	err != 0 ? throw("OpenCL Error") : 0;

	cl::finish();

}