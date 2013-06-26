/* Filename:    PasefUpdate.cs
 * Program:     Pasef
 * Version:     4.0
 * Programmers: Terry Beckhelm
 *              Daniel Flyte (C# revision)
 * Description: This class includes various utilities and tools that are 
 *              used with Pasef.
 * Methods:     
 *      checkBaseTotal()
 *      checkGrandtotal()
 *      countMatches()
 *      finish1()
 *      getFinal()
 *      getUpdateSum()
 *      update()
 *              
 *                      
 * Revision History
 * STR             Date       By    Description
 * --------------------------------------------------------------------------
 *                 06/07/98   tb    Initial coding
 *                 09/04/03   df    C# revision
 *                 01/13/06   tb    Version 4 Series 11
 * --------------------------------------------------------------------------
 */

using System;


namespace Sandag.TechSvcs.RegionalModels
{
  public class PasefUpdate
	{
        private static int rowSame, colSame;
  
        /*****************************************************************************/

        /* method checkBaseTotal() */
        /// <summary>
        /// Method to estimate values for rows with control totals and no base data.
        /// </summary>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 08/08/94   tb    Initial coding
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static void checkBaseTotal( int numRows, int numCols, int[,] matrx, int[] nuRowt, int[] nuColt, int colTotal )
        {
        int[] rowSum = new int[numRows];
        int[] colSum = new int[numCols];
        int total;
        int i,j;
        // -------------------------------------------------------------------------

        // Compute row and column totals.
        total = getUpdateSum( numRows, numCols, matrx, rowSum, colSum );
        for(j = 0; j < numCols; j++ )
          for(i = 0; i < numRows; i++ )
            if( rowSum[i] == 0 && nuRowt[i] > 0 )
              matrx[i,j] = ( int )( ( double )( nuRowt[i] * nuColt[j] ) / ( double )colTotal );
        }     // End method checkBaseTotal()

        /*****************************************************************************/

        /* method checkGrandTotal() */
        /// <summary>
        /// Method to compute new row and column totals and redistribute if unequal, 
        /// and return colTotal.
        /// </summary>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 08/08/94   tb    Initial coding
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static int checkGrandTotal( int numRows, int numCols, int[,] matrx,int[] nuRowt, int[] nuColt )
        {
          int rowTotal = 0, colTotal = 0, total = 0;
          int i,j;
          int[] rowSum = new int[numRows];
          int[] colSum = new int[numCols];
          // ---------------------------------------------------------------------

          // Compute total of row control vector.
          for(i = 0; i < numRows; i++ )
            rowTotal += nuRowt[i];

          total = getUpdateSum( numRows, numCols, matrx, rowSum, colSum );

          // Total col vectors
          for(j = 0; j < numCols; j++ )
            colTotal += nuColt[j];
          return colTotal;
        }     // End methed checkGrandTotal()

        /*****************************************************************************/

        /* method countMatches() */
        /// <summary>
        /// Method to count the matches in row and column totals.
        /// </summary>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 08/08/94   tb    Initial coding
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static void countMatches( int numRows, int numCols, int[] rowSum,int[] colSum, int[] nuRowt, int[] nuColt )
        {
          int i,j;
          // ---------------------------------------------------------------------
          rowSame = colSame = 0;
          for(i = 0; i < numRows; i++ )
            if( ( double )rowSum[i] / ( double )nuRowt[i] >= .999999 && 
                  ( double )rowSum[i] / ( double )nuRowt[i] <= 1.000003 )
                rowSame++;
        
          for(j = 0; j < numCols; j++ )
            if( ( double )colSum[j] / ( double )nuColt[j] >= .999999 && 
                  ( double )colSum[j] / ( double )nuColt[j] <= 1.000003 )
                colSame++;
        }     // End method countMatches()

        /*****************************************************************************/

        /* method finish1() */
        /// <summary>
        /// Method to perform final rounding for distribution estimates.  This is for
        /// sets with row totals not matching controls.
        /// </summary>
        /// <param name="dataArray">The data array being checked</param>
        /// <param name="colTarget">Column controls</param>
        /// <param name="rowTarget">Row controls</param>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 07/27/98   tb    Initial coding
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static void finish1( int numRows, int numCols, int[,] dataArray, int[] rowTarget, int[] colTarget )
        {
          int rowAdj = 0, colAdj = 0;
          int[] colDiff = new int[numCols];
          int[] colSum = new int[numCols];
          int[] rowSum = new int[numRows];
          int[] rowDiff = new int[numRows];
          int[] sortedData = new int[320];
          int i, j, id, k;
          int rowId;
          // -----------------------------------------------------------------------

          /* Compute differences in column (sector) sums and regional 
          * controls. */
          for( j = 0; j < numCols; j++ )
          {
            colSum[j] = 0;
            for( i = 0; i < numRows; i++ )
              colSum[j] += dataArray[i,j];
            colDiff[j] = colTarget[j] - colSum[j];
            colAdj += colDiff[j];
          }   // end for j
  
          for( i = 0; i < numRows; i++ )
          {
            rowSum[i] = 0;
            for( j = 0; j < numCols; j++ )
              rowSum[i] += dataArray[i,j];
            rowDiff[i] = rowTarget[i] - rowSum[i];
            rowAdj += rowDiff[i];
          }   // end for i
    
          /* Now run through dataArray, get negatives (subtract to meet 
          * dataArray total) and make adjustment in first column with negative
          * colTarget difference. */
          for( i = 0; i < numRows; i++ )
          {
            /* Find the smallest column value and make adjustment there 
            * (ideally, it would be negative, but we may run out of column 
            * negatives before we finish row negatives. */
            j = 0;     // Set the index for the column lookup.
            while( rowDiff[i] < 0 )
            {
              PasefUtils.sortAscending( 320, colDiff, numCols, sortedData );
              id = sortedData[j];
              if( dataArray[i,id] > 0 )     // Adjust cells > 0
              {
                dataArray[i,id]--;
                colDiff[id]++;
                rowDiff[i]++;
              }   // end if
              else     // Otherwise look for the next column to adjust.
                  if( ++j >= numCols )
                      return;
            }     // End while
          }     // End for i

          for( i = 0; i < numRows; i++ )
          {
            /* Now, find a dataArray with a positive rowDiff.  This means the 
            * total is < dataArray. */
            j = 0;
            while( rowDiff[i] > 0 )
            {
              // sortDescending( colDiff, numCols, sortedData );
              id = sortedData[j];
              dataArray[i,id]++;
              colDiff[id]--;
              rowDiff[i]--;
            }   // end while
          }   // end for i

          /* At this time, all the rowDiffs should be 0, but there will be
          * offsetting + and - in the colDiffs.  Start with negatives, find a
          * row for adjustment, make negative adjustment, then make offsetting
          * positive adjustment in column with positive difference. */
          for( j = 0; j < numCols; j++ )
          {
            while( colDiff[j] < 0 )
            {
              rowId = 999;
              for( i = 0; i < numRows; i++ )
              {
                if( dataArray[i,j] - 1 >= 0 )     // Subtract from cell > 0
                {
                  rowId = i;
                  dataArray[i,j]--;
                  colDiff[j]++;
                  break;
                }   // end if 
              }   // end for i

              // Now look for offset in the same row.
              if( rowId != 999 )
              {
                for( k = 0; k < numCols; k++ )
                {
                  if( colDiff[k] > 0 )
                  {
                    dataArray[rowId,k]++;
                    colDiff[k]--;
                    break;
                  }   // end if
                }  // end for k
              }   // end if  
              else
              {
                // printf("ERROR - NO ROW WITH POSITIVE CELL IN COL %d",j+1);
                return;
              }   // end else
            }     // End while
          }     // End for j

          // Recompute sums and compare to totals.
          rowAdj = colAdj = 0;

          /* Compute differences in column (sector) sums and regional 
          * colTargets. */
          for( j = 0; j < numCols; j++ )
          {
            colSum[j] = 0;
            for( i = 0; i < numRows; i++ )
              colSum[j] += dataArray[i,j];
            colDiff[j] = colTarget[j] - colSum[j];
            colAdj += colDiff[j];
          }   // end for j
  
          for( i = 0; i < numRows; i++ )
          {
            rowSum[i] = 0;
            for( j = 0; j < numCols; j++ )
              rowSum[i] += dataArray[i,j];
            rowDiff[i] = rowTarget[i] - rowSum[i];
            rowAdj += rowDiff[i];
          }   // end for i
        }     // End method finish1()

        /*****************************************************************************/

        /* method getFinal() */
        /// <summary>
        /// Method to get the flag to determine whether or not to finish routine.
        /// </summary>
        /// <param name="colTarget">Column controls</param>
        /// <param name="numCols">Number of columns in passed array</param>
        /// <param name="numRows">Number of rows in passed array</param>
        /// <param name="passer">Data array to control</param>
        /// <param name="rowTarget">Row controls</param>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 12/03/98   tb    Initial coding
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static bool getFinal( int numRows, int numCols, int[,] passer, int[] rowTarget, int[] colTarget )
        {
            int i,j;
            int[] rowTotal = new int[numRows];
            int[] colTotal = new int[numCols];
            // -------------------------------------------------------------------
            for(i = 0; i < numRows; i++ )
            {
              for(j = 0; j < numCols; j++ )
              {
                rowTotal[i] += passer[i,j];
                colTotal[j] += passer[i,j];
              }   // end for j
            }   // end for i

            for(i = 0; i < numRows; i++ )
              if( rowTotal[i] != rowTarget[i] )
                return true;

            for(j = 0; j < numCols; j++ )
              if( colTotal[j] != colTarget[j] )
                return true;
            return false;
        }     // End method getFinal()

        /*****************************************************************************/

        /* method getUpdateSum() */
        /// <summary>
        /// Method to compute compute row and column sums and total.
        /// </summary>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 08/08/94   tb    Initial coding
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static int getUpdateSum( int numRows, int numCols, int[,] matrx, int[] rowSum, int[] colSum )
        {
          int i,j;
          int total = 0;
          // ---------------------------------------------------------------------
          for(i = 0; i < numRows; i++ )
            rowSum[i] = 0;
          for(j = 0; j < numCols; j++ )
          {
            colSum[j] = 0;
            for(i = 0; i < numRows; i++ )
            {
              rowSum[i] += matrx[i,j];
              colSum[j] += matrx[i,j];
              total += matrx[i,j];
            }   // end for i
          }   // end for j
          return total;
        }     // End method getUpdateSum()

        /*****************************************************************************/
      
        /* method update() */
        /// <summary>
        /// Method to adjust a positively-valued matrix to specified row and column
        /// totals rounded to nearest integer value.
        /// </summary>
        /// <param name="matrx">The incoming data array</param>
        /// <param name="nuColt">Individual column colTarget total</param>
        /// <param name="numCols">Number of columns in matrix</param>
        /// <param name="numRows">Number of rows in matrix</param>
        /// <param name="nuRowt">Individual row colTarget total</param>
      
        /* Revision History
        * 
        * STR             Date       By    Description
        * --------------------------------------------------------------------------
        *                 08/08/94   tb    Initial coding
        *                 08/16/94   tb    Replaced stand alone with callable proc
        *                 12/09/97   tb    Copied from est_inc
        *                 12/09/97   tb    Copied from est_inc
        *                 06/09/99   tb    Added to pasef
        *                 12/09/97   tb    Copied from est_inc
        *                 06/09/99   tb    Added to pasef
        *                 08/31/99   tb    Modified for new update algorithm
        *                 09/04/03   df    C# revision
        * --------------------------------------------------------------------------
        */
        public static void update( int numRows, int numCols, int[,] matrx, int[]nuRowt, int[] nuColt )
        {
            int colTotal;             // Grand total of colTarget rows and cols
            bool loop = true;
            int loopCount, total, i, j;
        
            // Incoming individual row and column totals.
            int[] rowSum = new int[numRows];
            int[] colSum = new int[numCols];
            int[] newRT = new int[numRows];   // Dummy local to see row totals
            int[] newCT = new int[numCols];   // Dummy local to see col totals
            // ----------------------------------------------------------------------
            /* Check sum of new colTarget row and cols and redistribute if necessary */
            colTotal = checkGrandTotal( numRows, numCols, matrx, nuRowt, nuColt );
            checkBaseTotal( numRows, numCols, matrx, nuRowt, nuColt, colTotal );

            for( i = 0; i < numRows; i++ )
                newRT[i] = nuRowt[i];
            for( j = 0; j < numCols; j++ )
                newCT[j] = nuColt[j];

            // Compute new row and column sums.
            total = getUpdateSum( numRows, numCols, matrx, rowSum, colSum );
            countMatches( numRows, numCols, rowSum, colSum, nuRowt, nuColt );
            loopCount = 0;

            while( loop && loopCount < 10 )
            {
                /* Factor matrix to colTarget totals by iteration.
                * Initialize summation arrays. */
                countMatches( numRows, numCols, rowSum, colSum, nuRowt, nuColt );

                for( j = 0; j < numCols; j++ )
                {
                    for( i = 0; i < numRows; i++ )
                    {
                        if( colSum[j] > 0 )
                            matrx[i,j] = ( int )( 0.5 + ( double )matrx[i,j] * 
                                ( ( double )nuColt[j] / ( double )colSum[j] ) );
                        if( matrx[i,j] < 0 )
                            matrx[i,j] = 0;
                    }   // end for i
                }   // end for j

                /* Check difference between intermediate row totals and projected
                * row totals */
                total = getUpdateSum( numRows, numCols, matrx, rowSum, colSum );
                countMatches( numRows, numCols, rowSum, colSum, nuRowt, nuColt );

                // If the rows match bail out
                if( rowSame == numRows && colSame == numCols )
                {
                    loop = false;
                    continue;
                }   // end if

                // Factor matrix elements to row totals and round.
                for( i = 0; i < numRows; i++ )
                {
                    if( rowSum[i] > 0 )
                    {
                        for( j = 0; j < numCols; j++ )
                            matrx[i,j] = ( int )( 0.5 + ( double )matrx[i,j] * ( double )nuRowt[i] /( double )rowSum[i] );
                    }   // end if
                }   // end for i

                loopCount++;
            }     // End while loop
        }     // End method update()
        // ****************************************************************************
	}     // End class PasefUpdate
}     // End namespace PasefUpdate
