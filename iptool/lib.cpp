#include "lib.h"

#define MAXRGB 255
#define MINRGB 0

unsigned char checkValue(int value) {
	if (value > 255) return 255;
	if (value < 0) return 0;
	return value;
}
/*-----------------------------------------------------------------------**/
void add(IMG src, IMG tgt, int value)
{
	//LOG("Width: %d, height: %d, stride1: %d, stride2: %d\n", width, height, srcStride, tgtStride);
	ROI roi = { .x1 = 0, .x2=src.width, .y1=0, .y2=src.height};
	addROI(src, tgt, roi, value);
}

void addROI(IMG src, IMG tgt, ROI roi, int value)
{
	//LOG("Width: %d, height: %d, stride1: %d, stride2: %d\n", width, height, srcStride, tgtStride);
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

void grayROI(IMG src, IMG tgt, ROI roi, GRAY_WAY grayWay)
{
	//LOG("Width: %d, height: %d, stride1: %d, stride2: %d\n", width, height, srcStride, tgtStride);
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

void gray(IMG src, IMG tgt, GRAY_WAY grayWay)
{
	ROI roi = { .x1 = 0, .x2=src.width, .y1=0, .y2=src.height};
	grayROI(src, tgt, roi, grayWay);
}


inline void checkPixelChannelAndSet(IMG src, IMG tgt, int address, unsigned char t1, unsigned char t2) {
	int ch = src.ch[address];
	if ((ch < t1) || (ch > t2)) tgt.ch[address] = 0;
	else tgt.ch[address] = 255;
}

void binarize(IMG src, IMG tgt, ROI roi, unsigned char t1, unsigned char t2){
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

inline double gaus2DFunc(double r, double gs)
{
	double gs2 = 2*gs*gs;
	return exp(-r*r/gs2)/(3.14159265358979323846*gs2);
}

inline double gaus1DFunc(double x, double gs)
{
	return exp(-x*x/(2*gs*gs))/(sqrt(2*3.14159265358979323846)*gs);
}

int gausFilter2D(IMG src, IMG tgt, ROI roi, double gs, int br)
{		
	int ws = gs * 3;
	int s = 2*ws + 1;
	int roiY1 = roi.y1 + ws; 
	int roiY2 = roi.y2 - ws;
	int roiX1 = roi.x1 + ws;
	int roiX2 = roi.x2 - ws;
	LOG("[gaus2D] gs=%f, br=%d, ws=%d, s=%d\n", gs, br, ws, s);
	if ((roiY2 <= roiY1) || (roiX2 <= roiX1)) return s;
	double* w = (double*)malloc(sizeof(double) * s * s);
	w[0]=10;
	double norm = 0;
	//window init	
	LOG("WINDOW:\n");
	for (int i = -ws; i <= ws; i++)
	{				
		for (int j = -ws; j <= ws; j++)
		{
			double r = sqrt(i*i+j*j);
			double gv = gaus2DFunc(r, gs);		
			LOG("%6.4f  ", gv);
			norm += (w[(ws + i)*s + ws + j] = gv);
		}
		LOG("\n");
	}

	LOG("ROI: \n");
	for (int y = roi.y1; y < roi.y2; y++)
	{
		for (int x = roi.x1; x < roi.x2; x++) {
			LOG("%3d  ",  src.ch[src.stride * y + 3*x]);
		}
		LOG("\n");
	}	
	//LOG("\n");
	//LOG("\n");
	//window normalization
	// LOG("-------\n");
	// for (int i = -ws; i <= ws; i++)
	// {
	// 	for (int j = -ws; j <= ws; j++)
	// 	{
	// 		int index = (ws + i)*s + ws + j;
	// 		w[index] /= norm;
	// 		LOG("%f ", w[index]);
	// 	}
	// 	LOG("\n");
	// }	
	LOG("ROI*: \n");
	for (int y = roiY1; y < roiY2; y++)
	{
		//edges		
		int srcShift = src.stride * y;
		int tgtShift = tgt.stride * y;
		for (int x = roiX1; x < roiX2; x++) {
			//edges 2
			int srcBaseAddress = srcShift + 3*x;
			int tgtBaseAddress = tgtShift + 3*x;
			double totalSum = br;
			for (int i = -ws; i <= ws; i++)
			for (int j = -ws; j <= ws; j++)
			{
				totalSum += w[(ws + i)*s + ws + j] * src.ch[src.stride * (y+i) + 3*(x+j)];
			}
			tgt.ch[tgtBaseAddress] = tgt.ch[tgtBaseAddress + 1] = tgt.ch[tgtBaseAddress + 2] = checkValue(totalSum);
			LOG("%6.4f  ", totalSum);
		}
		LOG("\n");
	}
	LOG("RESULT: \n");
	for (int y = roi.y1; y < roi.y2; y++)
	{
		for (int x = roi.x1; x < roi.x2; x++) {
			LOG("%3d  ",  tgt.ch[src.stride * y + 3*x]);
		}
		LOG("\n");
	}	
	free(w);
	return s;
}

int gausFilter1Dx2(IMG src, IMG tgt, ROI roi, double gs, int br)
{
	int ws = gs * 3; //windowSize(gs);
	int s = 2*ws + 1;
	int roiY1 = roi.y1 + ws; 
	int roiY2 = roi.y2 - ws;
	int roiX1 = roi.x1 + ws;
	int roiX2 = roi.x2 - ws;
	LOG("[gaus2D] gs=%f, br=%d, ws=%d, s=%d\n", gs, br, ws, s);
	if ((roiY2 <= roiY1) || (roiX2 <= roiX1)) return s;
	double* w = (double*)malloc(sizeof(double) * s);
	double sum = 0;
	//window init	
	LOG("WINDOW: \n");
	for (int i = -ws; i <= ws; i++)
	{
		double gv = gaus1DFunc(i, gs);
		sum += (w[ws + i] = gv);
		LOG("%6.4f  ", gv);
	}
	//LOG("\n");
	//LOG("\n");
	//window normalization
	// for (int i = -ws; i <= ws; i++)
	// {
	// 	w[ws + i] /= sum;
	// }
	//if ((roiY2 <= roiY1) || (roiX2 <= roiX1)) return s; //NOOP
	LOG("\nROI: \n");
	for (int y = roi.y1; y < roi.y2; y++)
	{
		for (int x = roi.x1; x < roi.x2; x++) {
			LOG("%3d  ",  src.ch[src.stride * y + 3*x]);
		}
		LOG("\n");
	}	


	int iWidth = roi.y2 - roi.y1;
	int iHeight = roiX2 - roiX1;
	double* intermediate = (double*)malloc(sizeof(double) * iHeight * iWidth);
	//first pass: src --> intermediate
	for (int y = roi.y1; y < roi.y2; y++)
	{
		int srcShift = src.stride * y;		
		int yi = y - roi.y1;		
		for (int x = roiX1; x < roiX2; x++) {			
			//int srcBaseAddress = srcShift + 3*x;
			double sum = 0;
			for (int i = -ws; i <= ws; i++)
			{
				sum += w[ws + i] * src.ch[srcShift + 3*(x+i)];
			}
			intermediate[iWidth * (x - roiX1) + yi] = sum;
		}
	}	

	LOG("INTERMEDIATE: \n");
	for (int x = roiX1; x < roiX2; x++) {
		for (int y = roi.y1; y < roi.y2; y++)
		{
			int yi = y - roi.y1;			
			LOG("%6.4f  ",  intermediate[iWidth * (x - roiX1) + yi]);			
		}	
		LOG("\n");
	}
	//second pass: intermediate --> target
	LOG("\nROI*: \n");
	for (int y = roiY1; y < roiY2; y++)
	{
		int tgtShift = tgt.stride * y;
		int yi = y-roi.y1;
		for (int x = roiX1; x < roiX2; x++) {			
			int tgtBaseAddress = tgtShift + 3*x;
			int iindex = iWidth* (x-roiX1) + yi;
			double sum = br;
			for (int i = -ws; i <= ws; i++)
			{
				sum += w[ws + i] * intermediate[iindex + i];
			}
			tgt.ch[tgtBaseAddress] = tgt.ch[tgtBaseAddress + 1] = tgt.ch[tgtBaseAddress + 2] = checkValue(sum);
			LOG("%6.4f  ", sum);
		}
		LOG("\n");
	}	

	LOG("RESULT: \n");
	for (int y = roi.y1; y < roi.y2; y++)
	{
		for (int x = roi.x1; x < roi.x2; x++) {
			LOG("%3d  ",  tgt.ch[src.stride * y + 3*x]);
		}
		LOG("\n");
	}	
		
	free(intermediate);
	free(w);
	return s;
}

inline float l2(float x, float y, float z) {
	return sqrt(x*x + y*y + z*z);
}

void binarizeColor(IMG src, IMG tgt, ROI roi, int dist, unsigned char r, unsigned char g, unsigned char b) {
	for (int y=roi.y1; y < roi.y2; y++)
	{
		int srcShift = src.stride * y;
		int tgtShift = tgt.stride * y;
		for (int x = roi.x1; x < roi.x2; x++) {
			int shift = srcShift + 3*x;			
			float pr = src.ch[shift];
			float pg = src.ch[shift + 1];
			float pb = src.ch[shift + 2];
			float d = l2(pr - r, pg - g, pb - b);
			int shift2 = tgtShift + 3*x;
			if (d < dist) 
			{				
				tgt.ch[shift2] = 255;
				tgt.ch[shift2 + 1] = 255;
				tgt.ch[shift2 + 2] = 255;
			} else {
				tgt.ch[shift2] = 0;
				tgt.ch[shift2 + 1] = 0;
				tgt.ch[shift2 + 2] = 0;				
			}
		}
	}	
}