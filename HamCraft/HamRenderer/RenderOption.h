#pragma once

struct ResolutionDesc
{
	size_t Width;
	size_t Height;
};

const ResolutionDesc _1280_720 = { 1280, 720 };
const ResolutionDesc _1920x1080 = { 1920, 1080 };
const ResolutionDesc _2560x1440 = { 2560, 1440 };

const ResolutionDesc DEFAULT_RESOLUTION = _1920x1080;

struct RenderOption
{
	bool bFullScreen;
	ResolutionDesc Resolution;

	// TODO: ¹à±â, °¨¸¶ ...

	RenderOption()
		: bFullScreen(false)
		, Resolution(DEFAULT_RESOLUTION)
	{ }
};