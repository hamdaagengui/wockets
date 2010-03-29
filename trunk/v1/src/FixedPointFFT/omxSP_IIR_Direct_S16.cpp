/**
 * 
 * File Name:  omxSP_IIR_Direct_S16.c
 * OpenMAX DL: v1.0.2
 * Revision:   8916
 * Date:       Wednesday, October 31, 2007
 * 
 * (c) Copyright 2007 ARM Limited. All Rights Reserved.
 * 
 * 
 *
 * Description:
 * This file contains module for block IIR filtering
 *
 */

#include "stdafx.h"
#include "omxtypes.h"
#include "armOMX.h"
#include "omxSP.h"

#include "armCOMM.h"


/**
 * Function: omxSP_IIR_Direct_S16
 *
 * Description:
 * Block IIR filtering for 16-bit data type.
 *
 * Remarks:
 * This function applies the direct form II IIR filter defined by the
 * coefficient vector pTaps to a vector of input data.The output will saturate to 0x8000
 *(-32768) for a negative overflow or 0x7fff (32767) for a positive overflow.
 *
 * Parameters:
 * [in]  pSrc       	pointer to the vector of input samples to which
 *				the filter is applied.
 * [in]  len	     	the number of samples contained in both the
 *				input and output vectors.
 * [in]  pTaps      	pointer to the 2L+2 element vector that
 *				contains the combined numerator and denominator
 *				filter coefficients from the system transfer
 *				function,H(z). Coefficient scaling and c
 *				oefficient vector organization should follow
 *				the conventions described above. The value of
 *				the coefficient scalefactor exponent must be
 *              non-negative(sf>=0).
 * [in]  order    	the maximum of the degrees of the numerator and
 *				denominator coefficient polynomials from the
 *				system transfer function,H(z), that is:
 *              order=max(K,M)-1=L-1.
 * [in]  pDelayLine       pointer to the L element filter memory
 *				buffer(state). The user is responsible for
 *				allocation, initialization, and de-allocation.
 *				The filter memory elements are initialized to
 *				zero .
 * [out] pDst		pointer to the filtered output samples.
 *
 * Return Value:
 * Standard OMXResult result. See enumeration for possible result codes.
 *
 */ 
 

 
OMXResult omxSP_IIR_Direct_S16(
     const OMX_S16 * pSrc,
     OMX_S16 * pDst,
     OMX_INT len,
     const OMX_S16 * pTaps,
     OMX_INT order,
     OMX_S32 * pDelayLine
 )
{
    /* Argument Check */
    armRetArgErrIf( pSrc       == NULL, OMX_Sts_BadArgErr);
    armRetArgErrIf( pDst       == NULL, OMX_Sts_BadArgErr);
    armRetArgErrIf( pDelayLine == NULL, OMX_Sts_BadArgErr);
    armRetArgErrIf( pTaps      == NULL, OMX_Sts_BadArgErr);

    armRetArgErrIf( len   <= 0, OMX_Sts_BadArgErr);
    armRetArgErrIf( order <  0, OMX_Sts_BadArgErr);
    armRetArgErrIf( pTaps[order + 2]  <  0 , OMX_Sts_BadArgErr);

    /* Processing */

    while(len != 0)
    {
        omxSP_IIROne_Direct_S16(
                    *pSrc,
                    pDst,
                    pTaps,
                    order,
                    pDelayLine);
        
        pSrc++;
        pDst++;
        len--;
    }
    
    return OMX_Sts_NoErr;
    
}
