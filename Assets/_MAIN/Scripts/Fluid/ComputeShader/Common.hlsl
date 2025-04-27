#ifndef COMMON_INCLUDED
#define COMMON_INCLUDED

static const float2 GRAVITY = float2(0.0, -9.8);
static const float PI = 3.14159265358979323846;

float spikyKernel(float dist, float radius)
{
    if (dist > radius)
        return 0.0;
    float coeff = 15.0 / (PI * pow(radius, 6.0));
    return coeff * pow(radius - dist, 3.0);
}

float2 spikyGradient(float2 vec, float dist, float radius)
{
    if (dist > radius || dist < 0.001f)
        return float2(0.0, 0.0);
    float coeff = -45.0 / (PI * pow(radius, 6.0));
    return coeff * pow(radius - dist, 2.0) * (vec / (dist));
}

#endif