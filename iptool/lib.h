#ifndef LIB_H
#define LIB_H

#include "image.h"
#include <sstream>
#include <math.h>

struct __declspec(dllexport) IMG {
    unsigned char* ch;
    int width; 
    int height;
    int stride;
};

struct __declspec(dllexport) ROI {
    int x1; 
    int x2;
    int y1;
    int y2;
};

enum GRAY_WAY { R=0, G=1, B=2, L1=3, L2=4};
//int allocatePpm(char* ppm);
//int allocatePpm(int ppmId); //create copy
//void dealocatePpm(int ppmId);
//char* getPpm(int ppmId);
extern "C" void __declspec(dllexport) add(IMG src, IMG tgt, int value);
extern "C" void __declspec(dllexport) addROI(IMG src, IMG tgt, ROI roi, int value);
extern "C" void __declspec(dllexport) gray(IMG src, IMG tgt, GRAY_WAY grayWay);
extern "C" void __declspec(dllexport) grayROI(IMG src, IMG tgt, ROI roi, GRAY_WAY grayWay);
extern "C" void __declspec(dllexport) binarize(IMG src, IMG tgt, ROI roi, unsigned char t1, unsigned char t2);
extern "C" int __declspec(dllexport) gausFilter2D(IMG src, IMG tgt, ROI roi, float gs, int br);
//extern "C" int __declspec(dllexport) gausFilter1Dx2(IMG src, IMG tgt, ROI roi, float gs, int br);

//void binarize(int srcPpmId, int tgtPpmId, int threshold);
//void binarize(int srcPpmId, int tgtPpmId, int thresholdLow, int thresholdHigh);
//void scale(int srcPpmId, int tgtPpmId, float ratio);

#endif

