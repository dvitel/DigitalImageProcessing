#ifndef LIB_H
#define LIB_H

#if defined(_MSC_VER)
    //  Microsoft 
    #define EXPORT __declspec(dllexport)
    #define IMPORT __declspec(dllimport)
#elif defined(__GNUC__)
    //  GCC
    #define EXPORT __attribute__((visibility("default"))) 
    #define IMPORT
#else
    //  do nothing and hope for the best?
    #define EXPORT
    #define IMPORT
    #pragma warning Unknown dynamic link import/export semantics.
#endif

#if defined(_DEBUG)
    #define LOG(fmt, ...) printf((fmt), ##__VA_ARGS__)
#else 
    #define LOG(fmt, ...)
#endif

#include <sstream>
#include <math.h>

extern "C" {
    struct EXPORT IMG {
        unsigned char* ch;
        int width; 
        int height;
        int stride;
    };

    struct EXPORT ROI {
        int x1; 
        int x2;
        int y1;
        int y2;
    };

    enum GRAY_WAY { R=0, G=1, B=2, L1=3, L2=4};
    void EXPORT add(IMG src, IMG tgt, int value);
    void EXPORT addROI(IMG src, IMG tgt, ROI roi, int value);
    void EXPORT gray(IMG src, IMG tgt, GRAY_WAY grayWay);
    void EXPORT grayROI(IMG src, IMG tgt, ROI roi, GRAY_WAY grayWay);
    void EXPORT binarize(IMG src, IMG tgt, ROI roi, unsigned char t1, unsigned char t2);
    int EXPORT gausFilter2D(IMG src, IMG tgt, ROI roi, double gs, int br);
    int EXPORT gausFilter1Dx2(IMG src, IMG tgt, ROI roi, double gs, int br);
    void EXPORT binarizeColor(IMG src, IMG tgt, ROI roi, int dist, unsigned char r, unsigned char g, unsigned char b);
}

#endif

