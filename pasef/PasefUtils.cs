/* Filename:    PasefUtils.cs
 * Program:     Pasef
 * Version:     4.0
 * Programmers: Terry Beckhelm
 *              Daniel Flyte (C# revision)
 * Description: This class includes various utilities and tools that are 
 *              used with Pasef.
 * Methods:     
 *              applySRAChgShr()
 *              buildDetailedTotals()
 *              checkZero()
 *              controlCTSpop()
 *              ctBaseControl()
 *              descendingSortMulti()
 *              deriveSRAPop()
 *              ff1()
 *              ff2()
 *              getArrayStats()
 *              getIndex()
 *              inSpecialPop()
 *              loadVector()
 *              MGRABaseControls()
 *              pachinko()
 *              pachinkoMGRA()
 *              pachinkoWithMasterDecrement()
 *              popTotals()
 *              printCalcAge()
 *              printCalcEthSex()
 *              printSRAPop()
 *              printSRAsPop()
 *              printSRAnsPop()
 *              sortAscending()
 *              sraBaseControls()
 *              update()
 *              writeCTASCII()
 *              writeMGRASCII()
 *              writeSRAASCII()
 *              
 *                      
 * Revision History
 * STR             Date       By    Description
 * --------------------------------------------------------------------------
 *                 06/07/98   tb    Initial coding
 *                 09/04/03   df    C# revision
 *                 01/13/06   tb    version 4 - series 11
 *                 08/24/09   tb    version 5 - series 12 changing to mgras; new .net
 *                 11/11/12   tb    version 7 - series 13
 * --------------------------------------------------------------------------
 */


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;

namespace Sandag.TechSvcs.RegionalModels
{
	/// <summary>
	/// Summary description for PasefUtils.
	/// </summary>
	public class PasefUtils
	{
      

    /*****************************************************************************/
    /* method applySRAChgShr() */
    /// <summary>
    /// Method to adjust the ethnicity and sex shares at the SRA level.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/16/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */

    public static void applySRAChgShare( Pasef p, Master[] s )
    {
      int i,j,k;
      double shr, newShr;
      // -----------------------------------------------------------------------
      for(i = 0; i < p.NUM_SRA; i++ )
      {
        for(j = 1; j < p.NUM_ETH; j++ )
        {
          for(k = 1; k < p.NUM_SEX; k++ )
          {
            if( s[i].estimated.nsPop[0,0] > 0 )
              shr = ( double )s[i].estimated.nsPop[j,k] /( double )s[i].estimated.nsPop[0,0];
            else
              shr = 0;
            newShr = shr + s[i].chgShare[j,k];
            s[i].estimated.nsPop[j,k] = (int)(newShr*(double)s[i].control.nsPopTotal);
          }     /* End for k */
        }   // end for j
      }   // end for i

    }     /* End method applySRAChgShare() */

    /*****************************************************************************/

    /* method buildDetailedTotals() */
    /// <summary>
    /// Method to fill SRA totals, age, sex, and ethnicity.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/15/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void buildDetailedTotals( Pasef p, Master ar )
    {
      int j, k, l;
      // -----------------------------------------------------------------

      /* Fill nsPop total. */
      for( j = 1; j < p.NUM_ETH; j++ )
      {
        for( k = 1; k < p.NUM_SEX; k++ )
        {
          for( l = 0; l < p.NUM_AGE; l++ )
          {
            ar.nsPop[0,0,l] = 0;
            ar.nsPop[j,0,l] = 0;
            ar.nsPop[0,k,l] = 0;
            ar.sPop[0,0,l] = 0;
            ar.sPop[j,0,l] = 0;
            ar.sPop[0,k,l] = 0;
          }   // end for l
        }   // end for k
      }   // end for j
          
      for( j = 1; j < p.NUM_ETH; j++ )
      {
        for( k = 1; k < p.NUM_SEX; k++ )
        {
          for( l = 0; l < p.NUM_AGE; l++ )
          {
            ar.nsPop[0,0,l] += ar.nsPop[j,k,l];
            ar.nsPop[j,0,l] += ar.nsPop[j,k,l];
            ar.nsPop[0,k,l] += ar.nsPop[j,k,l];
            ar.sPop[0,0,l] += ar.sPop[j,k,l];
            ar.sPop[j,0,l] += ar.sPop[j,k,l];
            ar.sPop[0,k,l] += ar.sPop[j,k,l];
          }   // end for l
        }   // end for k
      }   // end for j
      
      for( j = 0; j < p.NUM_ETH; j++ )
        for( k = 0; k < p.NUM_SEX; k++ )
          for( l = 0; l < p.NUM_AGE; l++ )
            ar.pop[j,k,l] = ar.nsPop[j,k,l] + ar.sPop[j,k,l];

    }     /* End method buildDetailedTotals() */         
        
    /*****************************************************************************/

    // procedure chkBaseTot()

    /* estimate values for rows with control totals and no base data */

    /* Revision History
        SCR            Date       By   Description
        -------------------------------------------------------------------------
                        8/08/94    tb   initial coding
        -------------------------------------------------------------------------
    */

    public static void chkBaseTot(int num_rows, int num_cols, int[,] matrx, int[] nurowt, int[] nucolt, int coltot)
    {
        int[] rowsum = new int[num_rows];
        int[] colsum = new int[num_cols];

        /* compute row and column totals */
        int total = GetRowColTots(num_rows, num_cols, matrx, rowsum, colsum);
        for (int j = 0; j < num_cols; j++)
        {
            for (int i = 0; i < num_rows; i++)
            {
                if (rowsum[i] == 0 && nurowt[i] > 0)
                {
                    if (coltot > 0)
                    {
                        matrx[i, j] = (int)(((double)(nurowt[i] * nucolt[j]) / (double)coltot));
                    }  // end if
                    else
                    {
                        matrx[i, j] = 0;
                    }  // end else
                }  // end if
            }  // end for i
        }  // end for j
    }  // end procedure chkBaseTot()

    // **********************************************************************************************
    // procedure chkGrandTot()

    /*  new row and column totals and redistribute if unequal return coltot */
    /* Revision History
        SCR            Date       By   Description
        -------------------------------------------------------------------------
                        8/08/94    tb   initial coding
        -------------------------------------------------------------------------
    */

    public static int chkGrandTot(int num_rows, int num_cols, int[,] matrx, int[] nurowt, int[] nucolt)
    {

        int rowtot, coltot;
        int rsum, total, rt, rtot;
        int ii, i, j;
        int[] rowsum = new int[num_rows];
        int[] colsum = new int[num_cols];

        /* initialize some data */
        total = rowtot = coltot = 0;
        rsum = 0;

        /* compute total of row control vector */
        for (i = 0; i < num_rows; ++i)
            rowtot += nurowt[i];

        total = GetRowColTots(num_rows, num_cols, matrx, rowsum, colsum);

        /*total col vectors */
        for (j = 0; j < num_cols; ++j)
            coltot += nucolt[j];

        /* check new row and col control totals.  if not equal adjust, 
            otherwise, continue with allocation */
        if (coltot != rowtot)
        {
            /* the grand totals are different, adjust the rows */
            for (i = 0; i < num_rows; ++i)
            {
                if (rowsum[i] == nurowt[i])     /* keep track of cases where
                                                        old row tot = new row tot 
                                                        don't want to messs with these */
                    rsum += nurowt[i];
            }  // end for i
            total = 0;         /*reset total */
            rt = 0;
            ii = 0;
            for (i = 0; i < num_rows; ++i)
            {
                rtot = rowtot - rsum;
                if (rtot == 0 || (nurowt[i] == rowsum[i]))
                    continue;
                else
                {
                    /* redistribute based on column grand total */
                    if (rtot > 0)
                        nurowt[i] = (int)(.5 + (double)nurowt[i] * (double)(coltot - rsum) /
                            (double)rtot);
                    else
                        nurowt[i] = 0;
                    total += nurowt[i];
                    if (nurowt[i] <= rt)
                        continue;
                    else
                    {
                        rt = nurowt[i];
                        ii = i;
                    }  // end else
                }  // end else
            }  // end for i
            /* force row control totals to grand control total*/
            nurowt[ii] += coltot - total - rsum;
        }  // end if
        return coltot;

    } // end procedure chkGrandTot()

    //*********************************************************************************************

    /* method checkZero() */
    /// <summary>
    /// Method to look for zeros in the passer array.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/09/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static bool checkZero( int[,] ar, int rowCount, int colCount )
    {
      int i,j;
      // ---------------------------------------------------------------------
      for(i = 0; i < rowCount; i++ )
        for(j = 0; j < colCount; j++ )
          if( ar[i,j] < 0 )
            return true;
      return false;
    }     /* End method checkZero() */

    // ***************************************************************************

    /* method controlCTSpop() */
    /// <summary>
    /// Method to control the initial special population estimates.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/08/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void controlCTSpop( Pasef p,Master[] c, StreamWriter sw, SpecialMaster [] specialCT ,int num_special)
    {
      int pIndex;
      int g,i,j,k;
      int[] passer = new int[p.MAX_CHAR];     /* Temp array to store age, sex, and ethnicity in array. */
      // ----------------------------------------------------------------------------
      for(g = 0; g < p.NUM_CTS; g++ )
      {
       
        //p.writeToStatusBox("...Processing CT # " + (g+1).ToString());
        if( c[g].special )
        {
          /* Build a temp array for pachinko. */
          pIndex = 0;
          for(i = 1; i < p.NUM_ETH; i++ )
            for(j = 1; j < p.NUM_SEX; j++ )
              for(k = 0; k < p.NUM_AGE; k++ )
                passer[pIndex++] = c[g].sPop[i,j,k];

          pachinko(p.MAX_MGRAS_IN_CTS,p.MAX_CHAR,c[g].control.popTotal, passer );

          /* Restore temp array to original struct. */
          pIndex = 0;
          for(i = 1; i < p.NUM_ETH; i++ )
            for(j = 1; j < p.NUM_SEX; j++ )
              for(k = 0; k < p.NUM_AGE; k++ )
                c[g].sPop[i,j,k] = passer[pIndex++];
        }   // end if
        
        /* Derive the non-special population. */
        for(i = 1; i < p.NUM_ETH; i++ )
        {
          for(j = 1; j < p.NUM_SEX; j++ )
          {
            for(k = 0; k < p.NUM_AGE; k++ )
            {
              if( inSpecialPop( c[g].id ,specialCT,num_special) )
              {
                c[g].nsPop[i,j,k] = 0;
                c[g].pop[i,j,k] = c[g].sPop[i,j,k];
              }   // end if
              else
                c[g].nsPop[i,j,k] = c[g].pop[i,j,k];
          
              /* Constrain the ns pop to >= 0 */
              if( c[g].nsPop[i,j,k] < 0 )
                c[g].nsPop[i,j,k] = 0;
            }   // end for k
          }     // end for j
        }   // end for i
          
        popTotals( p,c[g].estimated.nsPop, c[g].nsPop );
        popTotals( p,c[g].estimated.sPop, c[g].sPop );

        for (i = 1;i < p.NUM_ETH; ++i)
        {
          for (j = 1; j < p.NUM_SEX; ++j)
          {
            for (k = 0; k < p.NUM_AGE; ++k)
            {
              c[g].pop[0,0,k] = 0;
              c[g].pop[0,j,k] = 0;
              c[g].pop[i,0,k] = 0;
              c[g].sPop[0,0,k] = 0;
              c[g].sPop[0,j,k] = 0;
              c[g].sPop[i,0,k] = 0;
              c[g].nsPop[0,0,k] = 0;
              c[g].nsPop[0,j,k] = 0;
              c[g].nsPop[i,0,k] = 0;
            }   // end for k
          }  // end for j
        }  // end for i

        for(i = 1; i < p.NUM_ETH; i++ )
        {
          for(j = 1; j < p.NUM_SEX; j++ )
          {
            for(k = 0; k < p.NUM_AGE; k++ )
            {
              c[g].pop[0,0,k] += c[g].pop[i,j,k];
              c[g].pop[0,j,k] += c[g].pop[i,j,k];
              c[g].pop[i,0,k] += c[g].pop[i,j,k];
              c[g].sPop[0,0,k] += c[g].sPop[i,j,k];
              c[g].sPop[0,j,k] += c[g].sPop[i,j,k];
              c[g].sPop[i,0,k] += c[g].sPop[i,j,k];
              c[g].nsPop[0,0,k] += c[g].nsPop[i,j,k];
              c[g].nsPop[0,j,k] += c[g].nsPop[i,j,k];
              c[g].nsPop[i,0,k] += c[g].nsPop[i,j,k];
            }   // end for k
          }   // end for j
        }   // end for i
      }     /* End for g */
    }     /* End method controlCTSpop() */

    /*****************************************************************************/

    /* method ctBaseControls() */
    /// <summary>
    /// Method to control the control the CT base estimates.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/15/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void ctBaseControls( Pasef p,StreamWriter swCT, Master[] s, Master[] c, int[] sraList, 
                                            int[] ctList, string ct_ascii, int scenarioID, int year )
    {
      int g,i,j,k,l,id;
      int colCount, rowCount;
      int[,] passer = new int[p.MAX_CTS_IN_SRA,p.MAX_AGE_GROUPS];
      int[] targetRowTotal = new int[p.MAX_CTS_IN_SRA];
      int[] targetColTotal = new int[p.MAX_AGE_GROUPS];
      int[] keepCTid = new int[p.MAX_CTS_IN_SRA];
      // ----------------------------------------------------------------------

      /* Control the CT sex and ethnicity to CT nsPop and SRA sex and ethnicity.  Two-way using update */

      for( g = 0; g <p. NUM_SRA; g++ )
      {
        p.writeToStatusBox("CT CONTROLLING PHASE 1 - SRA " + sraList[g].ToString());
        swCT.WriteLine( "CT CONTROLLING PHASE 1 - SRA {0,2}", sraList[g] );
        rowCount = 0;     /* Initialize the row counter. */
      
        /* Scan the CT list and find all in this SRA. */
        for( i = 0; i < p.NUM_CTS; i++ )
        {
          /* If the i CT is in the g SRA */
          if( c[i].controlIndex == g )
          {
            keepCTid[rowCount] = i;
            colCount = 0;     /* Initialize the column counter. */
            for( j = 1; j < p.NUM_ETH; j++ )
            {
              /* Build a temp array CT X (sex and ethnicity) in cols () */
              
              for( k = 1; k < p.NUM_SEX; k++ )
              {
                  swCT.Write( "{0,7}", c[i].estimated.nsPop[j,k] );
                  swCT.Flush();
                  passer[rowCount, colCount++] = c[i].estimated.nsPop[j,k];
              }   // end for k
            }   // end for j

            targetRowTotal[rowCount] = c[i].control.nsPopTotal;
            swCT.WriteLine( "{0,7}", targetRowTotal[rowCount] );
            swCT.Flush();
            rowCount++;
          }     /* End if */
        }     /* End for i */

        colCount = 0;
        for( j = 1; j < p.NUM_ETH; j++ )
        {
          for( k = 1; k < p.NUM_SEX; k++ )
          {
            targetColTotal[colCount++] = s[g].estimated.nsPop[j,k];
            swCT.Write( "{0,7}", s[g].estimated.nsPop[j,k] );
            swCT.Flush();
          }   // end for k
        }   // end for j

        swCT.WriteLine();
        swCT.WriteLine();
        swCT.Flush();
        //p.writeToStatusBox("... before update");
        PasefUpdate.update( rowCount, p.NUM_MIX, passer, targetRowTotal,targetColTotal );
        if( PasefUpdate.getFinal( rowCount, p.NUM_MIX, passer, targetRowTotal,targetColTotal) )
          PasefUpdate.finish1( rowCount, p.NUM_MIX, passer, targetRowTotal,targetColTotal );

        if( checkZero( passer, rowCount, p.NUM_MIX ) )
        {
          MessageBox.Show("FATAL ERROR - UPDATE on sex/eth got a negative SRA " + sraList[g]);
          p.Close();
        }  // end if
        //p.writeToStatusBox("...before restore");
        /* Restore the controlled data to original array. */
        for( i = 0; i < rowCount; i++ )
        {
          //p.writeToStatusBox("... processing row " + (i+1).ToString() + " of " + rowCount.ToString());
          id = keepCTid[i];
          colCount = 0;
          for( j = 1; j < p.NUM_ETH; j++ )
              for( k = 1; k < p.NUM_SEX; k++ )
                  c[id].estimated.nsPop[j,k] = passer[i,colCount++];
          //p.writeToStatusBox("......before print");
          printCalcEthSex( p,swCT, c[id].estimated.nsPop, id );
          //p.writeToStatusBox("......after print");
        }   // end for i

        // ---------------------------------------------------------------------------------
        /* Control the CT age to CT sex and eth and SRA age, sex, and 
        * ethnicity.  Two-way using update. */
        
        p.writeToStatusBox("CT CONTROLLING PHASE 2 - SRA " + sraList[g].ToString());
        swCT.WriteLine( "CT CONTROLLING PHASE 2 - SRA {0,2}", sraList[g] );
      
        /* Get the CT IDs from the keep array. */
        /* Build a temp array CT X (age) in cols (18). */
        for( j = 1; j < p.NUM_ETH; j++ )
        {
          for( k = 1; k < p.NUM_SEX; k++ )
          {
            swCT.WriteLine( "ETH = " + j + " SEX = " + k );
            for( i = 0; i < rowCount; i++ )
            {
              colCount = 0;
              id = keepCTid[i];
              for( l = 0; l < p.NUM_AGE; l++ )
              {
                  passer[i,colCount] = c[id].nsPop[j,k,l];
                  targetColTotal[colCount++] = s[g].nsPop[j,k,l];
              }   // end for l

              targetRowTotal[i] = c[id].estimated.nsPop[j,k];
              for( l = 0; l < p.NUM_AGE; l++ )
                  swCT.Write( "{0,6}", passer[i,l] );
              swCT.WriteLine( "{0,6}", targetRowTotal[i] );
            }     /* End for i */

            for( l = 0; l < p.NUM_AGE; l++ )
              swCT.Write( "{0,6}", targetColTotal[l] );

            swCT.WriteLine();
            swCT.WriteLine();
            swCT.Flush();

            PasefUpdate.update( rowCount, p.NUM_AGE, passer, targetRowTotal,targetColTotal );
            if( PasefUpdate.getFinal( rowCount, p.NUM_AGE, passer, targetRowTotal,targetColTotal ) )
              PasefUpdate.finish1(rowCount, p.NUM_AGE, passer, targetRowTotal,targetColTotal );

            if( checkZero( passer, rowCount, p.NUM_AGE ) )
            {
              MessageBox.Show("FATAL ERROR - UPDATE on age got a negative SRA " + sraList[g]);
              p.Close();
            }    // end if

            /* Restore */
            for( i = 0; i < rowCount; i++ )
            {
              id = keepCTid[i];
              colCount = 0;
              for( l = 0; l < p.NUM_AGE; l++ )
                  c[id].nsPop[j,k,l] = passer[i,colCount++];
              for( l = 0; l < p.NUM_AGE; l++ )
                  swCT.Write( "{0,6}", passer[i,l] );
              swCT.WriteLine( "{0,6}", targetRowTotal[i] );
            }   // end for i

            for( l = 0; l < p.NUM_AGE; l++ )
              swCT.Write( "{0,6}", targetColTotal[l] );

            swCT.WriteLine();
            swCT.Flush();
          }     /* End for k */
        }     /* End for j */

        swCT.WriteLine( "CT AGE AFTER PHASE 2 CONTROLLING" );
        for( i = 0; i < rowCount; i++ )
        {
          id = keepCTid[i];
          printCalcAge( p,swCT, c[id].nsPop, ctList[id] );
          buildDetailedTotals( p,c[id] );
        }   // end for i
      }     /* End for g */

      writeCTASCII( p, c, sraList, ct_ascii, scenarioID, year );
    }     /* End method ctBaseControls() */

    /*****************************************************************************/

    /* method deriveSRAPop() */
    /// <summary>
    /// Method to compute the SRA population variables.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/08/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void deriveSRAPop( Pasef p, Master[] s, Master[] c, Master reg, StreamWriter sw, int[] sraList )
    {
      int sraIndex;
      int i,j,k,l;
      // ---------------------------------------------------------------------
      /* Compute SRA and regional pops. */
      for(i = 0; i < p.NUM_CTS; i++ )
      {
        sraIndex = c[i].controlIndex;
        for(j = 0; j < p.NUM_ETH; j++ )
        {
          for(k = 0; k < p.NUM_SEX; k++ )
          {
            for(l = 0; l < p.NUM_AGE; l++ )
            {
              s[sraIndex].pop[j,k,l] += c[i].pop[j,k,l];
              s[sraIndex].nsPop[j,k,l] += c[i].nsPop[j,k,l];
              s[sraIndex].sPop[j,k,l] += c[i].sPop[j,k,l];
              reg.sPop[j,k,l] += c[i].sPop[j,k,l];
             
            }   // end for l
          }   // end for k
        }   // end for j
      }   // end for i

      for(j = 0; j < p.NUM_ETH; j++ )
      {
        for(k = 0; k < p.NUM_SEX; k++ )
        {
          for(l = 0; l < p.NUM_AGE; l++ )
          {
            reg.nsPop[j,k,l] = reg.pop[j,k,l] - reg.sPop[j,k,l];
            if (j > 0 && k > 0)
            {
              sw.WriteLine(j + "," + k + "," + l + "," + reg.pop[j,k,l] + "," + reg.sPop[j,k,l] + "," + reg.nsPop[j,k,l]);
              sw.Flush();
            }
          }   // end for l
        }   // end for k
      }   // end for j
        
      printSRAPop( p,sraList, s, reg, sw );
      printSRAsPop( p, sraList, s, reg, sw );
      printSRAnsPop( p, sraList, s, reg, sw );
    }     /* End method deriveSRAPop() */

    /*****************************************************************************/

    /* ff1()*/
    /* final rounding for distribution estimates */
    /* this one for sets with row totals not matching controls */
    // THERE ARE TWO FINISH1 ROUTINES - THIS ONE IS USED FOR THE UPDATE PROCESSING IN PDHH HENCE THE RENAME TO FF1
    // THERE IS ANOTHERE FINISH1 ROUTINE IN PASEFUPDATE.CS THAT IS USED FOR OTHER UPDATE PROCESSING
    /* Revision History
       Date       By   Description
       -------------------------------------------------------------------------
       07/27/98    tb   initial coding
       -------------------------------------------------------------------------
    */
    public static void ff1(int[,] matrix, int[] row_controls, int[] col_controls, int nrows, int ncols)
    {
        int row_total, col_total;
        int[] col_diff = new int[ncols];
        int[] col_sum = new int[ncols];
        int[] row_sum = new int[nrows];
        int[] row_suma = new int[nrows];
        int[] row_diff = new int[nrows];

        int i, j;
        Random ran = new Random(0);

        row_total = col_total = 0;
        /* compute differences in column (sector) sums and regional controls */
        col_diff.Initialize();
        row_diff.Initialize();
        col_sum.Initialize();
        row_sum.Initialize();

        for (j = 0; j < ncols; ++j)
        {
            for (i = 0; i < nrows; ++i)
            {
                col_sum[j] += matrix[i, j];
            }  // end for i

            col_diff[j] = col_controls[j] - col_sum[j];
            col_total += col_diff[j];
        }  // end for j

        for (i = 0; i < nrows; ++i)
        {
            row_sum[i] = 0;
            for (j = 0; j < ncols; ++j)
            {
                row_sum[i] += matrix[i, j];
            }  // end for j

            row_diff[i] = row_controls[i] - row_sum[i];
            row_total += row_diff[i];
        }  // end for i

        // adjust rows by factor
        for (i = 0; i < nrows; ++i)
        {
            row_suma[i] = 0;
            for (j = 0; j < ncols; ++j)
            {
                if (row_sum[i] > 0)
                {
                    matrix[i, j] = (int)((double)matrix[i, j] * (double)row_controls[i] / (double)row_sum[i]);
                }  // end if

                row_suma[i] += matrix[i, j];
                row_diff[i] = row_controls[i] - row_suma[i];
            }  // end for j
        }  // end for i

        /* now run through rows, get negatives (subtract to meet control) and make adjustment in first col with neg regional control difference */
        for (i = 0; i < nrows; ++i)
        {
            while (row_diff[i] != 0)
            {
                j = ran.Next(0, ncols);
                if (row_diff[i] > 0)
                {
                    ++matrix[i, j];
                    ++col_diff[j];
                    --row_diff[i];
                    if (row_diff[i] == 0)
                        break;
                }  // end if
                else
                {
                    if (matrix[i, j] > 0)
                    {
                        --matrix[i, j];
                        --col_diff[j];
                        ++row_diff[i];
                        if (row_diff[i] == 0)
                            break;
                    }  // end if
                }  // end else
            }  // end while
        }  // end for i

        /* recompute sums and compare to totals */
        row_total = col_total = 0;
        /* compute differences in column (sector) sums and regional controls */
        col_diff.Initialize();
        row_diff.Initialize();

        for (j = 0; j < ncols; ++j)
        {
            col_sum[j] = 0;
            for (i = 0; i < nrows; ++i)
            {
                col_sum[j] += matrix[i, j];

            }  // end for i
            col_diff[j] = col_controls[j] - col_sum[j];
            col_total += col_diff[j];
        }
        for (i = 0; i < nrows; ++i)
        {
            row_sum[i] = 0;
            for (j = 0; j < ncols; ++j)
            {
                row_sum[i] += matrix[i, j];
            }  // end for j
            row_diff[i] = row_controls[i] - row_sum[i];
            row_total += row_diff[i];
        }  // end for i
    }  // end procedure ff1()

    //***********************************************************************************

    // ff2()

    /* final rounding for distribution estimates */
    /* this one for sets with col totals not matching controls */
    /* this is the normal case - all the row dists add up, but the regional
       column totals are slightly off from regional totals */
    /* Revision History
       Date       By   Description
       -------------------------------------------------------------------------
       07/27/98    tb   initial coding
       -------------------------------------------------------------------------
    */
    public static void ff2(int[,] matrix, int[] col_controls, int nrows, int ncols)
    {
        int i, j, k, countk;
        bool found_another_col, found_col;
        int[] col_tot = new int[ncols];
        int[] col_diff = new int[ncols];
        Random ran = new Random(0);

        col_tot.Initialize();
        col_diff.Initialize();

        for (j = 0; j < ncols; ++j)
        {
            for (i = 0; i < nrows; ++i)
            {
                col_tot[j] += matrix[i, j];
            }  // end for i

            col_diff[j] = col_controls[j] - col_tot[j];
        }  // end for j

        for (j = 0; j < ncols; ++j)
        {
            while (col_diff[j] != 0)
            {
                if (col_diff[j] < 0)
                {
                    for (i = 0; i < nrows; ++i)
                    {
                        if (matrix[i, j] > 0)
                        {
                            --matrix[i, j];
                            ++col_diff[j];

                            /* now find the pos col in this row i to do the offset addition */
                            found_col = false;
                            while (!found_col)
                            {
                                k = ran.Next(0, ncols);
                                if (k == j)
                                    continue;
                                if (col_diff[k] > 0)
                                {
                                    ++matrix[i, k];
                                    --col_diff[k];
                                    found_col = true;
                                } // end if
                            }  // end while
                        }  // end if
                        if (col_diff[j] == 0) break;
                    }  // end for i
                }  // end if
                else
                {
                    /* col diff are > 0 */
                    for (i = 0; i < nrows; ++i)
                    {
                        ++matrix[i, j];
                        --col_diff[j];
                        found_another_col = false;
                        /* now find the neg col in this row i to do the offset subtraction */
                        /* there may not be any - so we have to set a flag and check it, 
                            if the flag is cleared ok, otherwise we have to reset the matrix and
                            col_diff done above and go to the next row */
                        found_col = false;
                        countk = 0;
                        while (!found_col && countk < ncols)
                        {
                            k = ran.Next(0, ncols);
                            if (k == j) continue;
                            ++countk;
                            if (col_diff[k] < 0)
                            {
                                if (matrix[i, k] > 0)
                                {
                                    found_another_col = true;
                                    --matrix[i, k];
                                    ++col_diff[k];
                                    found_col = true;
                                    break;
                                }  // end if
                            }  // end if
                        }  // end while

                        if (found_another_col && col_diff[j] == 0)
                            break;
                        else if (!found_another_col)
                        {
                            /* no additional column was found in this row to make an adjustment -
                                restore the matrix and col_diff values and go to next row */
                            --matrix[i, j];
                            ++col_diff[j];
                        }  // ens else if
                    }  // end for i
                } // end else
            }  // end while
        } // end for j

    } // end procedure ff2()

    //**********************************************************************************************************************
   
        
    /* method getArraySum() */
    /// <summary>
    /// Method to sum array used in pachinko method.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    private static int getArraySum( int[] arr, int count )
    {
      int sum = 0;
      for( int i = 0; i < count; i++ )
          sum += arr[i];
      return sum;
    }   // end getArraySum()

    /*****************************************************************************/

    /* method getArrayStats() */
    /// <summary>
    /// Method to perform array distribution and cumulative probability used in
    /// pachinko method.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    private static void getArrayStats( int[] arr, int lclSum, double[] arrDist, double[] arrProb, int count )
    {
      int i;
      double sum = 0;
      // ---------------------------------------------------------------------
      for(i = 0; i < count; i++ )
      {
        arrDist[i] = ( double )arr[i] / ( double )lclSum * 100;
        sum += arrDist[i];
        arrProb[i] = sum;
      }   // end for i
    }     /* End method getArrayStats() */

    /*****************************************************************************/

    /* method getIndex() */
    /// <summary>
    /// Method to get the index for the SRA or CT argument.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/99   tb    Added to pasef
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static int getIndex( int id, int[] list, int nlist )
    {
      int index = 999;
      int i;
      // ---------------------------------------------------------------------
      for(i = 0; i < nlist; i++ )
        if( id == list[i] )
            return i;
      return index;
    }   // end getIndex()

    /*****************************************************************************/

    // GetRowColTots()
    /* compute row and column sums and total */
    /* Revision History
        SCR            Date       By   Description
        -------------------------------------------------------------------------
                        8/08/94    tb   initial coding
        -------------------------------------------------------------------------
    */
    public static int GetRowColTots(int num_rows, int num_cols, int[,] matrx, int[] rowsum, int[] colsum)
    {
        int i, j, total;
        for (i = 0; i < num_rows; ++i)
            rowsum[i] = 0;
        total = 0;

        for (j = 0; j < num_cols; ++j)
        {
            colsum[j] = 0;
            for (i = 0; i < num_rows; ++i)
            {
                rowsum[i] += matrx[i, j];
                colsum[j] += matrx[i, j];
                total += matrx[i, j];
            }     /* end for i*/
        }     /* end for j */
        return (total);
    }  // end procedure GetRowColTots()

    //***************************************************************************************

    /* method inSpecialPop() */
    /// <summary>
    /// Method to determine if CT is in special pop list.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static bool inSpecialPop( int ctIndex ,SpecialMaster [] specialCT,int num_special)
    {
      int i;
      // --------------------------------------------------------------------------------
      for(i = 0; i < num_special; i++ )
        if( ctIndex == specialCT[i].ctID )
          return true;
      return false;
    }   // end inSpecialPop()

    /*****************************************************************************/

    /* method loadVector() */
    /// <summary>
    /// Method to distribute the ethnicity, sex, and age array to the master
    /// (1-dimension) array.
    /// </summary>
    /// <param name="control">Load = 1, unload = 2</param>
    /// <param name="esa">Age, sex, ethnicity array</param>
    /// <param name="vector">Master vector</param>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/02/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void loadVector( Pasef p, int[,,] esa, int[] vector, int control )    
    {
      int index = 0;
      int i,j,k;
      // --------------------------------------------------------------------
      // Load the vector
      if( control == 1 )
      {
        for(i = 1; i < p.NUM_ETH; i++ )
          for(j = 1; j < p.NUM_SEX; j++ )
            for(k = 0; k < p.NUM_AGE; k++ )
              vector[index++] = esa[i,j,k];
      }   // end if
      else
      {
        for(i = 1; i < p.NUM_ETH; i++ )
          for(j = 1; j < p.NUM_SEX; j++ )
            for(k = 0; k < p.NUM_AGE; k++ )
              esa[i,j,k] = vector[index++];
      }   // end else
    }     // End procedure loadVector()

    /*****************************************************************************/

    /* method MGRABaseControls() */
    /// <summary>
    /// Method to perform pachinko to CT cells to MGRAs.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/02/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void MGRABaseControls( Pasef p, Master[] c, Master[] m,
                                        int[] ctList, int[,] MGRAIDs,
                                        int[,] MGRAPop, int[] masterVector,
                                        int[] MGRAIndex,string mgra_ascii,string mgra_tab_ascii,
                                        string mgra_debug, int scenarioID, int year)
    {
      StreamWriter MGRADebug = null;
      int i, j, k, l,mid;
      int[] MGRAVector = new int[p.MAX_CHAR];
      // ------------------------------------------------------------------------
      // Rebuild CT pop to ensure totals
      for( i = 0; i < p.NUM_CTS; i++ )
        for( j = 1; j < p.NUM_ETH; j++ )
          for( k = 1; k < p.NUM_SEX; k++ )
            for( l = 0; l < p.NUM_AGE; l++ )
              c[i].pop[j,k,l] = c[i].sPop[j,k,l] + c[i].nsPop[j,k,l];
      try
      {
        MGRADebug = new StreamWriter( new FileStream( mgra_debug,FileMode.Create ) );
        MGRADebug.AutoFlush = true;
      }
      catch(Exception e )
      {
        System.Windows.Forms.MessageBox.Show( e.ToString(), e.GetType().ToString() );
        System.Windows.Forms.Application.Exit();
      }

      for( i = 0; i < p.NUM_CTS; i++ )
      {
          try
          {
              p.writeToStatusBox("Processing CT #" + (i + 1).ToString() + " " + ctList[i].ToString());
              // Distribute the detailed characteristics to the master vector
              PasefUtils.loadVector(p, c[i].pop, masterVector, 1);
              if (MGRAIndex[i] > 1)
              {

                  for (j = 0; j < MGRAIndex[i] - 1; j++)
                  {
                      if (MGRAPop[i, j] > 0)
                      {
                          Array.Clear(MGRAVector, 0, MGRAVector.Length);
                          PasefUtils.pachinkoMGRA(p.MAX_CHAR, i, j, masterVector, MGRAPop[i, j], MGRAVector, p.MAX_CHAR);
                          mid = MGRAIDs[i, j] - 1;
                          PasefUtils.loadVector(p, m[mid].pop, MGRAVector, 2);
                      }   // end if
                  }     // end for j
              }     // End if

              // Load remaining MGRA with remainder
              j = MGRAIndex[i] - 1;
              mid = MGRAIDs[i, j] - 1;
              PasefUtils.loadVector(p, m[mid].pop, masterVector, 2);
          }
          catch (Exception e)
          {
              System.Windows.Forms.MessageBox.Show(e.ToString(), e.GetType().ToString());
              
          }
      }     // End for i

      MGRADebug.Close();      
      p.writeToStatusBox( "Writing MGRA to ASCII..." );
      writeMGRAASCII( p,m,mgra_ascii,mgra_tab_ascii, scenarioID, year );

    }     // End method MGRABaseControls()
    
    /*****************************************************************************/
    
    /* method pachinko() */
    /// <summary>
    /// Method to perform +1 distribution scheme using random number and 
    /// cumulative distribution to assign values to elements in an array.
    /// </summary>
    /// <param name="count">CT data array to be distributed</param>
    /// <param name="data">Pop control total of target assignment</param>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/07/99   tb    Initial coding
    *                 09/04/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void pachinko( int M, int count, int target, int[] data )
    {
      int diff, lclSum,i1 = 0, loopCount = 0, where;
      int i;    
      double[] localDist = new double[M];     /* Computed distribution, max 20 elements. */
      double[] cumProb = new double[M];       /* Cumulative prob */
      // ---------------------------------------------------------------------

      lclSum = getArraySum( data, count );  
      getArrayStats( data, lclSum, localDist, cumProb, count );
      Random rand = new Random(0);

      /* Keep doing this until the target population is met. */
      while( lclSum != target && loopCount < 40000 )
      {
          lclSum = getArraySum( data, count );
          diff = target - lclSum;
          if( diff == 0 )
              continue;
          /* Get the random number between 1 and 100 and convert to xx.xx
          * decimal */
          where = rand.Next( 1, 100 ) % 100;
            
          /* Look for the index of the cumProb <= the random number. */
          for( i = 0; i < count; i++ )
          {
              /* Is the random number > this cumProb */
              if( where > cumProb[i] )
                  continue;
              else      /* Otherwise, use this index to occrement. */
              {
                  i1 = i;     /* Save the index in the cumProb. */
                  break;
              }   // end else
          }   // end for
          if( diff > 0 )
              data[i1]++;
          else if( data[i1] > 0 )
              data[i1]--;
          ++loopCount;
      }     /* End while */
            
    }     /* End method pachinko() */

    /*****************************************************************************/

    /* method pachinkoMGRA() */
    /// <summary>
    /// Method to perform +1 distribution scheme using random number and 
    /// cumulative distribution to assign values to elements in an array.
    /// </summary>
    /// <param name="MGRA">Target area array to receive distribution</param>
    /// <param name="starter">CT data array to be distributed</param>
    /// <param name="target">Control total of target assignment</param>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 12/07/98   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void pachinkoMGRA( int M, int ct, int MGRAIndex, int[] starter, int target, int[] MGRA, int count )
    { 
      int i1 = 0;
      int i;
      int loopCount = 0;
      int where, localSum;
      double[] localDist = new double[M];      // Computed distribution
      double[] cumProb  = new double[M];       // Cumulative probability
      Random rand = new Random(0);
      // ----------------------------------------------------------------------

      // Reset the computed arrays
      localSum = getArraySum( starter, count );
        
          // Keep doing this until the target pop is met
      while( target != 0 && loopCount < 40000 && localSum > 0 )
      {
          localSum = getArraySum( starter, count );
          getArrayStats( starter, localSum, localDist, cumProb, count );
          
          /* Get the random number between 1 and 10000 and convert to xx.xx decimal. */
          where = ( int )( rand.NextDouble() * 100 );

          // Look for the index of the cumProb <= the random number
          for(i = 0; i < count; i++ )
          {
              if( where > cumProb[i] )
                  continue;
              else
              {
                  i1 = i;     // Save the index in the cumProb
                  break;
              }   // end else
          }   // end for i

          if( starter[i1] > 0 )
          {
              MGRA[i1]++;
              starter[i1]--;
              target--;
          }   // end if
          loopCount++;
          
      }     // End while

      if( loopCount >= 40000 )
          MessageBox.Show( "pachinkoMGRA did not resolve in 40000 iterations for CT " +  ct + " MGRA " + MGRAIndex );
      }     // End procedure pachinkoMGRA()

    /*****************************************************************************/

    /* PachinkoWithMasterDecrement() */

    // Cumulative probability distribution method of +/- controlling/allocation
    // This Pachinko uses the distribution of a controlling (master) to determine the cumulative distribution of a slave
    // this algorithm uses the master distribution to fill an empty array that will sum to a control total
    // the master is decremented;  this is a common 2-way controlling to fill a 2X array with row and col controls
    // this version uses the row control to throttle the evental distribution as long as the master (column control) > 0


    //Revision History
    //   Date       By   Description
    //   ------------------------------------------------------------------
    //   06/22/11   tb   started initial coding for this module
    //   ------------------------------------------------------------------

    public static int PachinkoWithMasterDecrement(int target, int[] master, int[] slave, int ncount)
    {
        int i1, k;
        int loop_count;
        int where;
        int lcl_sum;

        double[] local_dist = new double[ncount];     /* computed distribution , max 20 elements */
        double[] cum_prob = new double[ncount];       /* cumulative probability */
        Random ran = new Random(0);

        /*-------------------------------------------------------------------------*/

        /* reset the computed arrays */
        Array.Clear(local_dist, 0, local_dist.Length);
        Array.Clear(cum_prob, 0, cum_prob.Length);
        i1 = 0;
        loop_count = 0;

        /* keep doing this until the target pop is met */
        lcl_sum = 999;

        while (target != 0 && loop_count < 40000 && lcl_sum > 0)
        {
            lcl_sum = PasefUtils.getArraySum(master, ncount);
            PasefUtils.getArrayStats(master, lcl_sum, local_dist, cum_prob, ncount);
            /* get the random number between 1 and 100*/
            where = ran.Next(1, 100);

            //int tt = Array.BinarySearch(cum_prob, where);
            /* look for the index of the cum_prob <= the random number */
            for (k = 0; k < ncount; ++k)
            {
                if (where <= cum_prob[k])     /* is the random number greater than this cum_prob */
                {
                    i1 = k;     /* save the index in the cum_prob*/
                    break;
                }  // end else
            }     /* end for i */

            if (master[i1] > 0)
            {
                slave[i1] += 1;
                master[i1] -= 1;
                target -= 1;
            }  // end if
            ++loop_count;
        }     /* end while */

        return loop_count;
    }     /* end procedure PachinkoWithMasterDecrement */

    /*****************************************************************************/


    /* method popTotals() */
    /// <summary>
    /// Method to build totals.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/08/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void popTotals( Pasef p, int[,] totalArray, int[,,] popArray )
    {
      int i,j,k;
      // ---------------------------------------------------------------------
      for(i = 1; i < p.NUM_ETH; i++ )
      {
        for(j = 1; j < p.NUM_SEX; j++ )
        {
          for(k = 0; k < p.NUM_AGE; k++ )
          {
            totalArray[i,j] = 0;      /* Total by eth sex */
            totalArray[i,0] = 0;      /* Total by eth */
            totalArray[0,j] = 0;      /* Total by sex */
            totalArray[0,0] = 0;      /* Total */
          }   // end for k
        }   // end for j
      }   // end for i
        
      for(i = 1; i < p.NUM_ETH; i++ )
      {
        for(j = 1; j < p.NUM_SEX; j++ )
        {
          for(k = 0; k < p.NUM_AGE; k++ )
          {
            totalArray[i,j] += popArray[i,j,k];     /* Total by eth sex */
            totalArray[i,0] += popArray[i,j,k];     /* Total by eth */
            totalArray[0,j] += popArray[i,j,k];     /* Total by sex */
            totalArray[0,0] += popArray[i,j,k];     /* Total */
          }   // end for k
        }   // end for j
      }   // end for i
    }     /* End method popTotals() */

    /*****************************************************************************/

    /* method printCalcAge() */
    /// <summary>
    /// Method to print age intermediate data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/11/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void printCalcAge( Pasef p, StreamWriter sw, int[,,] dArray, int id )
    {
      int i,j,k;
      // ------------------------------------------------------------------
      sw.WriteLine( "ID = {0,6}", id );
      for(i = 1; i < p.NUM_ETH; i++ )
      {
        for(j = 1; j < p.NUM_SEX; j++ )
        {
          sw.Write( "ETH = " + i + " SEX = " + j );
          for(k = 0; k < p.NUM_AGE; k++ )
            sw.Write( "{0,6}", dArray[i,j,k] );
          sw.WriteLine();
          sw.Flush();
        }   // end for j
      }   // end for i
    }     /* End method printCalcAge() */

    /*****************************************************************************/

    /* method printCalcEthSex() */
    /// <summary>
    /// Method to print ethnicity and sex intermediate data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/11/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void printCalcEthSex(Pasef p, StreamWriter sw, int[,] dArray, int id )
    {
      int i,j;
      // ---------------------------------------------------------------------
      sw.Write( "ID = {0,2}", id );
      for(i = 1; i < p.NUM_ETH; i++ )
        for(j = 1; j < p.NUM_SEX; j++ )
          sw.Write( "{0,7}", dArray[i,j] );
      sw.WriteLine();
      sw.Flush();
    }     /* End method printCalcEthSex() */

    /*****************************************************************************/

    /* method printSRAPop() */
    /// <summary>
    /// Method to print SRA ethnicitiy, sex, and intermediate data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 08/26/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void printSRAPop( Pasef p, int[] sraList, Master[] s, Master reg, StreamWriter sw )
    {
      int i,j,k,l;
      // --------------------------------------------------------------------
      sw.WriteLine( "SRA POP" );
      for(j = 0; j < p.NUM_ETH; j++ )
      {
        for(k = 0; k < p.NUM_SEX; k++ )
        {
          sw.WriteLine( "ETH = {0,2} SEX = {1,2}", j, k );
          for(i = 0; i < p.NUM_SRA; i++ )
          {
            sw.Write( "SRA {0,5}", sraList[i] );
            for(l = 0; l < p.NUM_AGE; l++ )
              sw.Write( "{0,7}", s[i].pop[j,k,l] );
            sw.WriteLine();
            sw.WriteLine();
            sw.Flush();
          }   // end for k

          sw.Write( "         " );
          for(l = 0; l < p.NUM_AGE; l++ )
            sw.Write( "{0,7}", reg.pop[j,k,l] );
          sw.WriteLine();
          sw.WriteLine();
          sw.Flush();
        }   /* End for k */
      }   // end for j

    }    /* End method printSRAPop() */

    /*****************************************************************************/

    /* method printSRAnsPop() */
    /// <summary>
    /// Method to print SRA non-special ethnicitiy, sex, and intermediate data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 08/24/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void printSRAnsPop( Pasef p, int[] sraList, Master[] s, Master reg, StreamWriter sw )
    {
      int j,k,l,i;
      // ----------------------------------------------------------------------
      sw.WriteLine( "SRA NONSPECIAL POP" );
      for(j = 0; j < p.NUM_ETH; j++ )
      {
        for(k = 0; k < p.NUM_SEX; k++ )
        {
          sw.WriteLine( "ETH = {0,2} SEX = {0,2}", j, k );
          for(i = 0; i < p.NUM_SRA; i++ )
          {
            sw.Write( "SRA {0,5}", sraList[i] );
            for(l = 0; l < p.NUM_AGE; l++ )
              sw.Write( "{0,7}", s[i].nsPop[j,k,l] );
            sw.WriteLine();
            sw.Flush();
          }   // end for i

          sw.Write( "         " );
          for(l = 0; l < p.NUM_AGE; l++ )
            sw.Write( "{0,7}", reg.nsPop[j,k,l] );
          sw.WriteLine();
          sw.WriteLine();
          sw.Flush();
        }     /* End for k */
      }   // end for j

    }     /* End method printSRAnsPop() */

    /*****************************************************************************/

    /* method printSRAsPop() */
    /// <summary>
    /// Method to print SRA special ethnicitiy, sex, and intermediate data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 08/24/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void printSRAsPop( Pasef p, int[] sraList, Master[] s, Master reg, StreamWriter sw )
    {
      int j,k,i,l;
      // ----------------------------------------------------------------------
      sw.WriteLine( "SRA SPECIAL POP" );
      for(j = 0; j < p.NUM_ETH; j++ )
      {
        for(k = 0; k < p.NUM_SEX; k++ )
        {
          sw.WriteLine( "ETH = {0,2} SEX = {1,2}", j, k );
          for(i = 0; i < p.NUM_SRA; i++ )
          {
            sw.Write( "SRA {0,5}", sraList[i] );
            for(l = 0; l < p.NUM_AGE; l++ )
              sw.Write( "{0,7}", s[i].sPop[j,k,l] );
            sw.WriteLine();
            sw.Flush();
          }   // end for i

          sw.Write( "         " );
          for(l = 0; l < p.NUM_AGE; l++ )
            sw.Write( "{0,7}", reg.sPop[j,k,l] );
          sw.WriteLine();
          sw.WriteLine();
          sw.Flush();
        }     /* End for k */
      }   // end for j

    }     /* End method printSRAsPop() */

    /*****************************************************************************/

    /* method sortAscending() */
    /// <summary>
    /// Method to sort the data in ascending order.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/09/99   tb    Added to Pasef
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void sortAscending( int M, int[] ar, int numVars, int[] sortedData )
    {
      int cVal;
      int[] tempData = new int[M];
      int i,j;
      // --------------------------------------------------------------------
      /* We don't want to sort the data here only the index - pass the real data into a temp for the sorting routine */
      for(i = 0; i < numVars; i++ )
      {
        sortedData[i] = i;
        tempData[i] = ar[i];
      }   // end for i

      for(i = 0; i < numVars - 1; i++ )
      {
        for(j = i + 1; j < numVars; j++ )
        {
          if( tempData[j] < tempData[i] )
          {
            cVal = tempData[i];
            tempData[i] = tempData[j];
            tempData[j] = cVal;
            cVal = sortedData[i];
            sortedData[i] = sortedData[j];
            sortedData[j] = cVal;
          }   // end if
        }   // end for j
      }   // end for i
    }     // End method sortAscending()

    /*****************************************************************************/

    /* method sraBaseControls() */
    /// <summary>
    /// Method to control the SRA base estimates.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 06/08/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void sraBaseControls( Pasef p, StreamWriter sraout, Master[] s, Master reg, int year,
                                        int[] sraList, string sra_ascii, int scenarioID )
    {
      int colCount, i, j, k, l;
      int[,] passer = new int[p.NUM_SRA,p.MAX_AGE_GROUPS];
      int[] targetRowTotal = new int[p.NUM_SRA];
      int[] targetColTotal = new int[p.MAX_AGE_GROUPS];
      // ---------------------------------------------------------------------------

      sraout.WriteLine( "SRABASE CONTROLLING" );
      sraout.Flush();
          
      applySRAChgShare(p,  s );

      /* Control the SRA sex and ethnicity to SRA nsPop and regional sex and ethnicity.  Two-way using update. */
      for( i = 0; i < p.NUM_SRA; i++ )
      {
        colCount = 0;
        for( j = 1; j < p.NUM_ETH; j++ )
        {
          /* Build a temp array SRA X (sex and ethnicity) in cols (NUM_MIX) */
          
          for( k = 1; k < p.NUM_SEX; k++ )
          {
            sraout.Write( "{0,7}", s[i].estimated.nsPop[j,k] );
            passer[i,colCount++] = s[i].estimated.nsPop[j,k];
          }   // end for k
        }     // End for j
        targetRowTotal[i] = s[i].control.nsPopTotal;
        sraout.WriteLine( "{0,7}", targetRowTotal[i] );
        sraout.Flush();
      }     // End for i

      colCount = 0;
      for( j = 1; j <p.NUM_ETH; j++ )
      {
        for( k = 1; k < p.NUM_SEX; k++ )
        {
          targetColTotal[colCount++] = reg.estimated.nsPop[j,k];
          sraout.Write( "{0,7}", reg.estimated.nsPop[j,k] );
        }   // end for k
        sraout.WriteLine();
        sraout.Flush();
      }   // end for j

      PasefUpdate.update( p.NUM_SRA, p.NUM_MIX, passer, targetRowTotal, targetColTotal );
      if( PasefUpdate.getFinal( p.NUM_SRA, p.NUM_MIX, passer, targetRowTotal,targetColTotal ) )
          PasefUpdate.finish1( p.NUM_SRA,p.NUM_MIX, passer, targetRowTotal,targetColTotal );

      // Restore the controlled data to original array.
      sraout.WriteLine( "SRA ETH AND SEX AFTER PHASE 1 CONTROLLING" );
    
      for( i = 0; i < p.NUM_SRA; i++ )
      {
        colCount = 0;
        for( j = 1; j < p.NUM_ETH; j++ )
          for( k = 1; k < p.NUM_SEX; k++ )
            s[i].estimated.nsPop[j,k] = passer[i,colCount++];
        printCalcEthSex( p, sraout, s[i].estimated.nsPop, sraList[i] );
      }   // end for i
      // -----------------------------------------------------------------------------

      /* Control the SRA age, sex, eth and regional age, sex, and ethnicity.  Two-way using update. */
      p.writeToStatusBox( "SRA Controlling Phase 2..." );
      sraout.WriteLine( "SRA CONTROLLING PHASE 2 before update" );
      
      // Build a temp array sra X (age) in cols (18)
      for( j = 1; j < p.NUM_ETH; j++ )
      {
        for( k = 1; k < p.NUM_SEX; k++ )
        {
          p.writeToStatusBox( "   ETH = " + j + " SEX = " + k );
          sraout.WriteLine( "ETH = " + j + " SEX = " + k );
          for( i = 0; i < p.NUM_SRA; i++ )
          {
            colCount = 0;
            for( l = 0; l < p.NUM_AGE; l++ )
            {
              passer[i,colCount] = s[i].nsPop[j,k,l];
              targetColTotal[colCount++] = reg.nsPop[j,k,l];
            }   // end for l 
            targetRowTotal[i] = s[i].estimated.nsPop[j,k];
          }   // end for i
          
          for( i = 0; i < p.NUM_SRA; i++ )
          {
            for( l = 0; l < p.NUM_AGE; l++ )
              sraout.Write( passer[i,l].ToString() + "," );
            sraout.WriteLine( targetRowTotal[i].ToString() );
          }   // end for i

          for( l = 0; l < p.NUM_AGE; l++ )
            sraout.Write( targetColTotal[l] + ",");

          sraout.WriteLine();
          sraout.WriteLine();
          sraout.Flush();

          PasefUpdate.update( p.NUM_SRA, p.NUM_AGE, passer, targetRowTotal,targetColTotal );
          if( PasefUpdate.getFinal( p.NUM_SRA, p.NUM_AGE, passer, targetRowTotal,targetColTotal ) )
              PasefUpdate.finish1(p.NUM_SRA, p.NUM_AGE, passer, targetRowTotal,targetColTotal );

          // Restore
          for( i = 0; i < p.NUM_SRA; i++ )
          {
            colCount = 0;
            for( l = 0; l < p.NUM_AGE; l++ )
                s[i].nsPop[j,k,l] = passer[i,colCount++];
          }   // end for i

          sraout.WriteLine( "SRA CONTROLLING PHASE 2 after update" );
          for( i = 0; i < p.NUM_SRA; i++ )
          {
            for( l = 0; l < p.NUM_AGE; l++ )
              sraout.Write( passer[i,l] + "," );
            sraout.WriteLine( targetRowTotal[i].ToString() );
          }   // end for i

          for( l = 0; l < p.NUM_AGE; l++ )
            sraout.Write( targetColTotal[l] + ",");
          sraout.WriteLine();
          sraout.Flush();
        }     // End for k
      }     // End for j

      sraout.WriteLine( "SRA AGE AFTER PHASE 2 CONTROLLING" );
      for( i = 0; i < p.NUM_SRA; i++ )
      {
        printCalcAge( p,sraout, s[i].nsPop, sraList[i] );
        buildDetailedTotals(p, reg );
      }   // end for i
      writeSRAASCII( p,sraList, s,sra_ascii,scenarioID,year);
    }     // End method sraBaseControls()

    /*****************************************************************************/

    
    /* method writeCTASCII() */
    /// <summary>
    /// Method to output intermediate CT data to file.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 08/26/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void writeCTASCII( Pasef p, Master[] c, int[] sraList, string ct_ascii, int scenarioID, int year )
    {
      int sraIndex;
      int g,i,j,k;
      StreamWriter ctASCII = new StreamWriter( new FileStream( ct_ascii, FileMode.Create ) );
      // ---------------------------------------------------------------------
      for(g = 0; g < p.NUM_CTS; g++ )
      {
        sraIndex = c[g].controlIndex;
        for(i = 1; i < p.NUM_ETH; i++ )
        {
          for(j = 1; j < p.NUM_SEX; j++ )
          {
            for(k = 0; k < p.NUM_AGE; k++ )
            {
              if (c[g].pop[i,j,k] > 0)
              {
                ctASCII.WriteLine( scenarioID + "," + year + "," + c[g].id.ToString() + "," + sraList[sraIndex].ToString() +
                    "," + i.ToString() + "," + j.ToString() + "," + k.ToString() + "," + 
                    c[g].pop[i,j,k].ToString() + "," + c[g].sPop[i,j,k].ToString() + "," + 
                    c[g].nsPop[i,j,k].ToString());
                ctASCII.Flush();
              }   // end if
            }   // end for k
          }   // end for j
        }   // end for i
      }     // End for g
      ctASCII.Close();
    }     // End method writeCTASCII()

    /*****************************************************************************/

    /* method writeMGRAASCII() */
    /// <summary>
    /// Method to output MGRA ASCII data.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/02/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void writeMGRAASCII( Pasef p, Master[] m, string mgra_ascii,string mgra_tab_ascii, int scenarioID, int year)
    {
      int g,i,j,k;
      StreamWriter MGRAASCII = null, MGRATabASCII = null;
      // ----------------------------------------------------------------------
      try
      {
        // Controlled table
        MGRAASCII = new StreamWriter( new FileStream(mgra_ascii,FileMode.Create ) );
        // Tabular data MGRA, eth, sex 20 ages table
        MGRATabASCII = new StreamWriter( new FileStream(mgra_tab_ascii,FileMode.Create ) );
        MGRAASCII.AutoFlush = MGRATabASCII.AutoFlush = true;
      }
      catch( Exception e )
      {
        System.Windows.Forms.MessageBox.Show( e.ToString(),e.GetType().ToString() );
        System.Windows.Forms.Application.Exit();
      }
      for(g = 0; g < p.NUM_MGRAS; g++ )
      {
        if (g % 1000 == 0)
            p.writeToStatusBox("   Processing Record # " + g.ToString());
        for(i = 1; i < p.NUM_ETH; i++ )
        {
          MGRATabASCII.Write( scenarioID + "," + year + "," + (g + 1).ToString() + "," + i.ToString() + ",");
          MGRATabASCII.Flush();
          for(j = 0; j < p.NUM_SEX; j++ )
          {
            for(k = 0; k < p.NUM_AGE-1; k++ )   // 1 less than max because last has no following ","
            {
              if( i > 0 && j > 0 && m[g].pop[i,j,k] > 0 )
              {
                // Controlled table
                MGRAASCII.WriteLine(scenarioID + "," + year + "," +  (g + 1) + "," + i + "," + j + "," + k + "," +
                                    m[g].pop[i,j,k] );
              }
            }   // end for k
            if (i>0 && j > 0 && m[g].pop[i,j,p.NUM_AGE-1] > 0)   // pick up last age group 
              MGRAASCII.WriteLine( scenarioID + "," + year + "," + (g + 1) + "," + i + "," + j + "," + (19) + "," + m[g].pop[i,j,19] );
                                            
            // Tabular table
            // write the place holder for total by sex
            MGRATabASCII.Write("0,");
            for (k = 0; k < p.NUM_AGE; ++k)
            {
              if (k == 19 && j ==2)
              {
                MGRATabASCII.Write( m[g].pop[i,j,k] );
                MGRATabASCII.Flush();
              }   // end if
              else
              {
                MGRATabASCII.Write(m[g].pop[i,j,k] + ",");
                MGRATabASCII.Flush();
              }   // end else
            }     // end for k                                         
          }     // End for j
          MGRATabASCII.WriteLine();
          MGRATabASCII.Flush();

        }     // End for i
      }     // End for g
      MGRAASCII.Close();
      MGRATabASCII.Close();
    }     // End method writeMGRAASCII()

    /*****************************************************************************/

    /* method writeSRAASCII() */
    /// <summary>
    /// Method to output intermediate SRA data to file.
    /// </summary>
      
    /* Revision History
    * 
    * STR             Date       By    Description
    * --------------------------------------------------------------------------
    *                 09/08/99   tb    Initial coding
    *                 09/05/03   df    C# revision
    * --------------------------------------------------------------------------
    */
    public static void writeSRAASCII( Pasef p, int[] sraList, Master[] s,string sra_ascii,int scenarioID,int year)
    {
      int g,i,j,k;
      string str;
      StreamWriter sraASCII = new StreamWriter( new FileStream( sra_ascii,FileMode.Create ) );
      // -----------------------------------------------------------------------
      for(g = 0; g < p.NUM_SRA; g++ )
      {
        for(i = 1; i < p.NUM_ETH; i++ )
        {
          for(j = 1; j < p.NUM_SEX; j++ )
          {
            for(k = 0; k < p.NUM_AGE; k++ )
            {
              str = scenarioID +"," + year + "," + sraList[g].ToString() + ',' + i.ToString() + "," + j.ToString() + "," +
                  k.ToString() + "," + s[g].pop[i,j,k].ToString() + "," + 
                  s[g].sPop[i,j,k].ToString() + "," + s[g].nsPop[i,j,k].ToString();
              sraASCII.WriteLine(str);
              sraASCII.Flush();
            }   // end for k
          }   // end for j
        }   // end for i
      }   // end for g

      sraASCII.Close();
    }     // End method writeSRAASCII()

    // ****************************************************************************
  
  }     // End class PasefUtils
}     // End namespace Pasef.Utils
