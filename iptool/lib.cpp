#include "lib.h"

#define MAXRGB 255
#define MINRGB 0

unsigned char checkValue(int value) {
	if (value > 255) return 255;
	if (value < 0) return 0;
	return value;
}
/*-----------------------------------------------------------------------**/
extern "C" void __declspec(dllexport) add(IMG src, IMG tgt, int value)
{
	//printf("Width: %d, height: %d, stride1: %d, stride2: %d\r\n", width, height, srcStride, tgtStride);
	ROI roi = { .x1 = 0, .x2=src.width, .y1=0, .y2=src.height};
	addROI(src, tgt, roi, value);
}

extern "C" void __declspec(dllexport) addROI(IMG src, IMG tgt, ROI roi, int value)
{
	//printf("Width: %d, height: %d, stride1: %d, stride2: %d\r\n", width, height, srcStride, tgtStride);
	for (int y=roi.y1; y < roi.y2; y++)
	{
		int srcShift = src.stride * y;
		int tgtShift = tgt.stride * y;
		for (int x = roi.x1; x < roi.x2; x++) {
			tgt.ch[tgtShift + 3*x + 0] = checkValue(src.ch[srcShift + 3*x + 0] + value);  
			tgt.ch[tgtShift + 3*x + 1] = checkValue(src.ch[srcShift + 3*x + 1] + value);  
			tgt.ch[tgtShift + 3*x + 2] = checkValue(src.ch[srcShift + 3*x + 2] + value);  
		}
	}
}

extern "C" void __declspec(dllexport) grayROI(IMG src, IMG tgt, ROI roi, GRAY_WAY grayWay)
{
	//printf("Width: %d, height: %d, stride1: %d, stride2: %d\r\n", width, height, srcStride, tgtStride);
	switch (grayWay)
	{
		case(B): 		
			for (int y=roi.y1; y < roi.y2; y++)
			{
				int srcShift = src.stride * y;
				int tgtShift = tgt.stride * y;
				for (int x = roi.x1; x < roi.x2; x++) {
					tgt.ch[tgtShift + 3*x + 0] = src.ch[srcShift + 3*x + 0]; 
					tgt.ch[tgtShift + 3*x + 1] = src.ch[srcShift + 3*x + 0];
					tgt.ch[tgtShift + 3*x + 2] = src.ch[srcShift + 3*x + 0];
				}
			}
			break;
		case(G):
			for (int y=roi.y1; y < roi.y2; y++)
			{
				int srcShift = src.stride * y;
				int tgtShift = tgt.stride * y;
				for (int x = roi.x1; x < roi.x2; x++) {
					tgt.ch[tgtShift + 3*x + 0] = src.ch[srcShift + 3*x + 1];  
					tgt.ch[tgtShift + 3*x + 1] = src.ch[srcShift + 3*x + 1];  
					tgt.ch[tgtShift + 3*x + 2] = src.ch[srcShift + 3*x + 1];  
				}
			}
			break;	
		case(R):
			for (int y=roi.y1; y < roi.y2; y++)
			{
				int srcShift = src.stride * y;
				int tgtShift = tgt.stride * y;
				for (int x = roi.x1; x < roi.x2; x++) {
					tgt.ch[tgtShift + 3*x + 0] = src.ch[srcShift + 3*x + 2]; 
					tgt.ch[tgtShift + 3*x + 1] = src.ch[srcShift + 3*x + 2]; 
					tgt.ch[tgtShift + 3*x + 2] = src.ch[srcShift + 3*x + 2];
				}
			}
			break;		
		case(L1):
			for (int y=roi.y1; y < roi.y2; y++)
			{
				int srcShift = src.stride * y;
				int tgtShift = tgt.stride * y;
				for (int x = roi.x1; x < roi.x2; x++) {
					int sum = src.ch[srcShift + 3*x + 0] + src.ch[srcShift + 3*x + 1] + src.ch[srcShift + 3*x + 2];
					int avg = sum / 3;
					tgt.ch[tgtShift + 3*x + 0] = avg;
					tgt.ch[tgtShift + 3*x + 1] = avg; 
					tgt.ch[tgtShift + 3*x + 2] = avg; 
				}
			}
			break;		
		case(L2):
			for (int y=roi.y1; y < roi.y2; y++)
			{
				int srcShift = src.stride * y;
				int tgtShift = tgt.stride * y;
				for (int x = roi.x1; x < roi.x2; x++) {
					double sum = src.ch[srcShift + 3*x + 0]*src.ch[srcShift + 3*x + 0] + src.ch[srcShift + 3*x + 1]*src.ch[srcShift + 3*x + 1] + src.ch[srcShift + 3*x + 2]*src.ch[srcShift + 3*x + 2];
					int avg = (int)(sqrt(sum) * 255.0 / 441.673); //magic number - sqrt(255^2 + 255^2 + 255^2)
					tgt.ch[tgtShift + 3*x + 0] = avg;
					tgt.ch[tgtShift + 3*x + 1] = avg; 
					tgt.ch[tgtShift + 3*x + 2] = avg; 
				}
			}
			break;	
		default:
			break;									
	}
}

extern "C" void __declspec(dllexport) gray(IMG src, IMG tgt, GRAY_WAY grayWay)
{
	ROI roi = { .x1 = 0, .x2=src.width, .y1=0, .y2=src.height};
	grayROI(src, tgt, roi, grayWay);
}


inline void checkPixelChannelAndSet(IMG src, IMG tgt, int address, unsigned char t1, unsigned char t2) {
	int ch = src.ch[address];
	if ((ch < t1) || (ch > t2)) tgt.ch[address] = 0;
	else tgt.ch[address] = 255;
}

extern "C" void __declspec(dllexport) binarize2(IMG src, IMG tgt, ROI roi, unsigned char t1, unsigned char t2){
	for (int y=roi.y1; y < roi.y2; y++)
	{
		int srcShift = src.stride * y;
		int tgtShift = tgt.stride * y;
		for (int x = roi.x1; x < roi.x2; x++) {
			checkPixelChannelAndSet(src, tgt, srcShift + 3*x + 0, t1, t2);
			checkPixelChannelAndSet(src, tgt, srcShift + 3*x + 1, t1, t2);
			checkPixelChannelAndSet(src, tgt, srcShift + 3*x + 2, t1, t2);
		}
	}
}

inline float gaus2DFunc(float r, float gs)
{
	float gs2 = 2*gs*gs;
	return exp(-r*r/gs2)/(3.14159265358979323846*gs2);
}

inline float gaus1DFunc(float x, float gs)
{
	return exp(-x*x/(2*gs*gs))/(sqrt(2*3.14159265358979323846)*gs);
}

inline int windowSize(float gs) {
	return floor(gs * 3);// sqrt(2.0));
}

extern "C" int __declspec(dllexport) gausFilter2D(IMG src, IMG tgt, ROI roi, float gs, int br)
{
	int ws = windowSize(gs);
	int s = 2*ws + 1;
	float* w = (float*)malloc(sizeof(float) * s * s);
	float sum = 0;
	//window init	
	for (int i = -ws; i <= ws; i++)
	for (int j = -ws; j <= ws; j++)
	{
		float r = sqrt(i*i+j*j);
		float gv = gaus2DFunc(r, gs);
		sum += (w[(ws + i)*s + ws + j] = gv);
	}
	//window normalization
	for (int i = -ws; i <= ws; i++)
	for (int j = -ws; j <= ws; j++)
	{
		w[(ws + i)*s + ws + j] /= sum;
	}
	int roiY1 = roi.y1 + ws; 
	int roiY2 = roi.y2 - ws;
	int roiX1 = roi.x1 + ws;
	int roiX2 = roi.x2 + ws;
	for (int y = roiY1; y < roiY2; y++)
	{
		//edges		
		int srcShift = src.stride * y;
		int tgtShift = tgt.stride * y;
		for (int x = roiX1; x < roiX2; x++) {
			//edges 2
			int srcBaseAddress = srcShift + 3*x;
			int tgtBaseAddress = srcShift + 3*x;
			float sum = br;
			for (int i = -ws; i <= ws; i++)
			for (int j = -ws; j <= ws; j++)
			{
				sum += w[(ws + i)*s + ws + j] * src.ch[srcBaseAddress + j + src.stride * i];
			}
			tgt.ch[tgtBaseAddress] = tgt.ch[tgtBaseAddress + 1] = tgt.ch[tgtBaseAddress + 2] = checkValue(sum); //3 channels
		}
	}	
	free(w);
	return s;
}

/*
extern "C" int __declspec(dllexport) gausFilter1Dx2(IMG src, IMG tgt, ROI roi, float gs, int br)
{
	int ws = windowSize(gs);
	int s = 2*ws + 1;
	float* w = (float*)malloc(sizeof(float) * s);
	float sum = 0;
	//window init	
	for (int i = -ws; i <= ws; i++)
	{
		float gv = gaus1DFunc(i, gs);
		sum += (w[ws + i] = gv);
	}
	//window normalization
	for (int i = -ws; i <= ws; i++)
	{
		w[ws + i] /= sum;
	}
	int roiY1 = roi.y1 + ws; 
	int roiY2 = roi.y2 - ws;
	int roiX1 = roi.x1 + ws;
	int roiX2 = roi.x2 + ws;
	if ((roiY2 <= roiY1) || (roiX2 <= roiX1)) return s; //NOOP
	unsigned char* intermediate = (unsigned char*)malloc(sizeof(unsigned char) * (roi.y2-roi.y1) * (roi.x2 - roi.x1));
	for (int y = roi.y1; y < roi.y2; y++)
	{
		for (int x = roi.x1; x < roi.x2; x++) {
			//edges 2
			int srcBaseAddress = srcShift + 3*x;
			float sum = 0;
			for (int i = -ws; i <= ws; i++)
			{
				sum += w[ws + i] * src.ch[srcBaseAddress + i];
			}
			tgt1.ch[tgtBaseAddress] = tgt1.ch[tgtBaseAddress + 1] = tgt1.ch[tgtBaseAddress + 2] = checkValue(sum); //3 channels
		}

		if ((y >= (roi.y1 + ws)) || (y < (roi.y2 - ws)) 
		{			
			int srcShift = src.stride * y;
			//int tgtShift = tgt1.stride * y;

		} else {
			//edges		
			intermediate[]
		}
		//edges		
		int srcShift = src.stride * y;
		//int tgtShift = tgt1.stride * y;
		for (int x = roiX1; x < roiX2; x++) {
			//edges 2
			int srcBaseAddress = srcShift + 3*x;
			int tgtBaseAddress = tgtShift + 3*x;//tgt1.stride * x + 3*y;
			float sum = 0;
			for (int i = -ws; i <= ws; i++)
			{
				sum += w[ws + i] * src.ch[srcBaseAddress + i];
			}
			tgt1.ch[tgtBaseAddress] = tgt1.ch[tgtBaseAddress + 1] = tgt1.ch[tgtBaseAddress + 2] = checkValue(sum); //3 channels
		}
	}	
	for (int y = roiY1; y < roiY2; y++)
	{
		//edges		
		int tgt1Shift = tgt1.stride * y;
		int tgt2Shift = tgt2.stride * y;
		for (int x = roiX1; x < roiX2; x++) {
			//edges 2
			int tgt1BaseAddress = tgt1Shift + 3*x;
			float sum = br;
			for (int i = -ws; i <= ws; i++)
			{
				sum += w[ws + i] * tgt1.ch[tgt1BaseAddress + j + src.stride * i];
			}
			tgt.ch[baseAddress] = tgt.ch[baseAddress + 1] = tgt.ch[baseAddress + 2] = checkValue(sum); //3 channels
		}
	}	
	free(w);
	return s;
}
*/