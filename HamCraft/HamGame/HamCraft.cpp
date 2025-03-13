#include "HamCraft.h"

namespace ham
{
	HamCraft::HamCraft()
		: mRenderer()
		, mEvent()
		, mChunkManager()
	{

	}

	HamCraft::~HamCraft()
	{

	}

	bool HamCraft::Initialize()
	{
		if (!mRenderer.Initialize())
			return false;

		if (!mEvent.Initialize())
			return false;

		if (!mChunkManager.Initialize(Vec2i{ 0,0 }))
			return false;

		if (!mTexManager.Initialize(GetSDLRenderer()))
			return false;

		mTexManager.LoadTexture(1, "C:\\Dev\\HamCraft\\HamCraft\\Resource\\Image\\Tile\\dirt.png", true);
		return true;
	}

	void HamCraft::Run()
	{
		bool quit = false;
		while (!quit)
		{
			mRenderer.Update(0.16f);
			mChunkManager.Update(0.16f);
			mEvent.Update(0.16f);

			const Rect rtRect = mRenderer.GetRTPos();
			std::vector<std::pair<Chunk*, Vec2i>> targetChunks = mChunkManager.GetIntersectChunks(rtRect);
			drawTiles(targetChunks);

			mRenderer.Render();
		}
	}

	void HamCraft::Finalize()
	{
		mRenderer.Finalize();
		mEvent.Finalize();
	}

	void HamCraft::drawTiles(const std::vector<std::pair<Chunk*, Vec2i>>& targetChunks)
	{
		Rect rtRect = mRenderer.GetRTPos();
		for (auto& chunkTup : targetChunks)
		{
			Chunk* chunk = chunkTup.first;
			Vec2i chunkPos = chunkTup.second;

			Rect chunkRect = { chunkPos.X, chunkPos.Y, ChunkManager::CHUNK_PX_SIZE.X, ChunkManager::CHUNK_PX_SIZE.Y };
			ASSERT(mRenderer.GetRTPos().DoIntersect(chunkRect));

			Rect intersectRect = mRenderer.GetRTPos().Intersect(chunkRect);

			Vec2i startLocalPos = Vec2i{ intersectRect.X, intersectRect.Y } - chunkPos;
			Vec2i endLocalPos = Vec2i{ intersectRect.X + intersectRect.W, intersectRect.Y + intersectRect.H } - chunkPos;
			Vec2i startLocalIdx = startLocalPos / CELL_PX_SIZE;
			Vec2i endLocalIdx = endLocalPos / CELL_PX_SIZE;
			Vec2i numCells = endLocalIdx - startLocalIdx;

			Vec2i cellBasePos = chunkPos + startLocalIdx * CELL_PX_SIZE;

			for (int y = 0; y < numCells.Y; ++y)
			{
				for (int x = 0; x < numCells.X; ++x)
				{
					Cell& cell = chunk->Map(Vec2i{ startLocalIdx.X + x, startLocalIdx.Y + y });
					Vec2i cellPos = cellBasePos + Vec2i{ x,y } * CELL_PX_SIZE;

					const Cell& up = mChunkManager.GetCell(cellPos + Vec2i{ 0, CELL_PX_SIZE });
					const Cell& left = mChunkManager.GetCell(cellPos + Vec2i{ -CELL_PX_SIZE,0 });
					const Cell& down = mChunkManager.GetCell(cellPos + Vec2i{ 0,-CELL_PX_SIZE });;
					const Cell& right = mChunkManager.GetCell(cellPos + Vec2i{ CELL_PX_SIZE,0 });;

					TILE_FLAG_TYPE foreNeighborFlag = (up.IsForeFilled() << 3) | (left.IsForeFilled() << 2) | (down.IsForeFilled() << 1) | (right.IsForeFilled() << 0);
					TILE_FLAG_TYPE reaRNeighborFlag = (up.IsRearFilled() << 3) | (left.IsRearFilled() << 2) | (down.IsRearFilled() << 1) | (right.IsRearFilled() << 0);
					TILE_FLAG_TYPE foreVariationFlag = 0x2 << 4;
					TILE_FLAG_TYPE rearVariationFlag = 0x2 << 4;
					TILE_FLAG_TYPE forrFlag = foreNeighborFlag | foreVariationFlag;
					TILE_FLAG_TYPE rearFlag = reaRNeighborFlag | rearVariationFlag;

					if (cell.IsForeEmpty())
						continue;

					const Tile& tile = cell.ForeTile;

					SDL_Texture* tex = GetTexManager()->GetTexture(Tile::CvtIdTile2Tex(tile.GetId()));
					ASSERT(tex != nullptr);

					// Render Target 정보
					const Vec2i camPos = GetRenderer()->GetCameraPos();
					const Rect rtRect = GetRenderer()->GetRTPos();
					const Rect cellRect = { cellPos.X, cellPos.Y, CELL_PX_SIZE, CELL_PX_SIZE };
					// Flag 처리
					const Vec2i texOffset = Tile::ExtractTexOffset(forrFlag);

					// 화면과 타일 교차
					Rect intersectRect = rtRect.Intersect(cellRect);

					if (!intersectRect.IsValid())
						return;

					// Draw Rectangle 정보
					// Destination Rectangle
					Rect dstRect =
					{
						intersectRect.X - rtRect.X,
						intersectRect.Y - rtRect.Y,
						intersectRect.W, intersectRect.H
					};
					// To Screen Space
					const Vec2i SCREEN_SIZE = GetRenderer()->GetRTSize();
					SDL_Rect sdlDstRect =
					{
						dstRect.X,
						SCREEN_SIZE.Y - dstRect.Y - dstRect.H,
						dstRect.W, dstRect.H
					};
					// Source Rectangle
					bool bSrcRectXFlag = intersectRect.X == cellPos.X;
					bool bSrcRectYFlag = intersectRect.Y != cellPos.Y; // Screen Space y flip

					SDL_Rect sdlSrcRect =
					{
						texOffset.X * CELL_PX_SIZE + (bSrcRectXFlag ? 0 : CELL_PX_SIZE - intersectRect.W),
						texOffset.Y * CELL_PX_SIZE + (bSrcRectYFlag ? 0 : CELL_PX_SIZE - intersectRect.H),
						intersectRect.W, intersectRect.H
					};

					// Render
					SDL_RenderCopy(mRenderer.GetSDLRenderer(), tex, &sdlSrcRect, &sdlDstRect);
				}
			}
		}
	}
}