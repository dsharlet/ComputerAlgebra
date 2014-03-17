#include <iostream>
#include <Windows.h>

// This program is a companion to the LotkaVolterra project. It provides a C++ implementation
// of the SimulateNativeHardCoded test case.

// The same as SimulateNative from LotkaVolterra, but hardcoded in C++.
void SimulateNativeHardCoded(int N, double* x)
{
	// Loop over the sample range requested.
	for (int n = 1; n < N; ++n)
	{
		double x0 = x[0];
		double x1 = x[1];
		double x2 = x[2];
		double x3 = x[3];

		double dx0_dt = 1.0
			- 1 * x0
			- 1.09 * x1
			- 1.52 * x2
			- 0 * x3;
		double dx1_dt = 1.0
			- 0 * x0
			- 1 * x1
			- 0.44 * x2
			- 1.36 * x3;
		double dx2_dt = 1.0
			- 2.33 * x0
			- 0 * x1
			- 1 * x2
			- 0.47 * x3;
		double dx3_dt = 1.0
			- 1.21 * x0
			- 0.51 * x1
			- 0.35 * x2
			- 1 * x3;

		dx0_dt *= 0.001 * 1 * x0;
		dx1_dt *= 0.001 * 0.72 * x1;
		dx2_dt *= 0.001 * 1.53 * x2;
		dx3_dt *= 0.001 * 1.27 * x3;

		x += 4;

		x[0] = x0 + dx0_dt;
		x[1] = x1 + dx1_dt;
		x[2] = x2 + dx2_dt;
		x[3] = x3 + dx3_dt;
	}
}

int main(int argc, char ** argv)
{
	const int N = 100000;

	// Array of doubles representing the populations over time.
	double* data = new double[N * 4];
	// Initialize the system to have the population specified in the system.
	data[0] = 0.2;
	data[1] = 0.4586;
	data[2] = 0.1307;
	data[3] = 0.3557;

	// Start time of the simulation.
	__int64 start;
	QueryPerformanceCounter((LARGE_INTEGER *)&start);
	// run the simulation 100 times to avoid noise.
	for (int i = 0; i < 100; ++i)
		SimulateNativeHardCoded(N, data);

	std::cout << data[(N - 1) * 4 + 3] << std::endl;

	__int64 end;
	QueryPerformanceCounter((LARGE_INTEGER *)&end);
	__int64 freq;
	QueryPerformanceFrequency((LARGE_INTEGER *)&freq);
	std::cout << "SimulateNativeHardCoded time: " << (double)(end - start) / (double)freq << std::endl;

	return 0;
}
