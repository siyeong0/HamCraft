#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

static const float2 GRAVITY = float2(0.0, -9.8);
static const float PI = 3.14159265358979323846;

struct Parcel
{
    float2 Position;
    float2 Velocity;
};

float smoothKernel(float dist, float radius)
{
	float volume = PI * pow(radius, 8.0) / 4.0;
	float value = max(0, radius * radius - dist * dist);
	return value * value * value / volume;
}

#endif