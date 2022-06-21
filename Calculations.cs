using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace nrlmsise
{
    internal static class Calculations
    {
        //Based on original C script https://github.com/magnific0/nrlmsise-00
        //Converted to C# by Adam Rusaw https://github.com/awasur04

        #region Global Variables
        //PARMB
        public static double gsurf;
        public static double re;

        //GTS3C
        public static double dd;

        /* DMIX */
        static double dm04, dm16, dm28, dm32, dm40, dm01, dm14;

        //MESO7
        public static double[] meso_tn1 = new double[5];
        public static double[] meso_tn2 = new double[4];
        public static double[] meso_tn3 = new double[5];
        public static double[] meso_tgn1 = new double[2];
        public static double[] meso_tgn2 = new double[2];
        public static double[] meso_tgn3 = new double[2];

        //POWER7 (Defined in Data.cs)
        public static double[] pt;
        public static double[][] pd;
        public static double[] ps;
        public static double[][] pdl;
        public static double[][] ptl;
        public static double[][] pma;
        public static double[] sam;

        //LOWER7 (Defined in Data.cs)
        public static double[] ptm;
        public static double[][] pdm;
        public static double[] pavgm;

        //LPOLY
        public static double dfa;
        public static double[][] plg;
        public static double ctloc, stloc;
        public static double c2tloc, s2tloc;
        public static double s3tloc, c3tloc;
        public static double apdf;
        public static double[] apt = new double[4];
        #endregion

        #region GTD7
        static Output GTD7(Input inputParams, Flags inputFlags)
        {
            //Unknown variables for calculation
            double xlat;
            double xmm;
            int mn3 = 5;
            double[] zn3 = new double[] { 32.5, 20.0, 15.0, 10.0, 0.0 };
            int mn2 = 4;
            double[] zn2 = new double[] { 72.5, 55.0, 45.0, 32.5 };
            double altt;
            double zmix = 62.5;
            double tmp;
            double dm28m;
            double tz = 0.0;
            double dmc;
            double dmr;
            double dz28;
            Output gtsOutput = new Output();
            Output gtdOutput = new Output();

            inputFlags.computeFlags();

            //Latitude variation of gravity (none for sw[2] = 0)
            xlat = inputParams.G_lat;
            if (inputFlags.Sw[2] == 0)
            {
                xlat = 45.0;
            }
            Glatf(xlat);

            xmm = pdm[2][4];

            //Thermosphere / Mesosphere (above zn2[0])
            if (inputParams.Altitude > zn2[0])
            {
                altt = inputParams.Altitude;
            }
            else
            {
                altt = zn2[0];
            }

            tmp = inputParams.Altitude;
            inputParams.Altitude = altt;
            gtsOutput = GTS7(inputParams, inputFlags);
            altt = inputParams.Altitude;
            inputParams.Altitude = tmp;

            if (inputFlags.Sw[0] != 0)
            {
                //metric adjustment
                dm28m = dm28 * 1.0E6;
            }
            else
            {
                dm28m = dm28;
            }

            gtdOutput.Temperature[0] = gtsOutput.Temperature[0];
            gtdOutput.Temperature[1] = gtsOutput.Temperature[1];

            if (inputParams.Altitude >= zn2[0])
            {
                for (int i = 0; i < 9; i++)
                {
                    gtdOutput.Densities[i] = gtsOutput.Densities[i];
                }
                    
                return gtdOutput;
            }


            //LOWER MESOSPHERE/UPPER STRATOSPHERE (between zn3[0] and zn2[0])
            //Temperature at nodes and gradients at end nodes
            //Inverse temperature a linear function of spherical harmonics
            meso_tgn2[0] = meso_tgn1[1];
            meso_tn2[0] = meso_tn1[4];
            meso_tn2[1] = pma[0][0] * pavgm[0] / (1.0 - inputFlags.Sw[20] * Glob7s(pma[0], inputParams, inputFlags));
            meso_tn2[2] = pma[1][0] * pavgm[1] / (1.0 - inputFlags.Sw[20] * Glob7s(pma[1], inputParams, inputFlags));
            meso_tn2[3] = pma[2][0] * pavgm[2] / (1.0 - inputFlags.Sw[20] * inputFlags.Sw[22] * Glob7s(pma[2], inputParams, inputFlags));
            meso_tgn2[1] = pavgm[8] * pma[9][0] * (1.0 + inputFlags.Sw[20] * inputFlags.Sw[22] * Glob7s(pma[9], inputParams, inputFlags)) * meso_tn2[3] * meso_tn2[3] / (Math.Pow((pma[2][0] * pavgm[2]), 2.0));
            meso_tn3[0] = meso_tn2[3];

            if (inputParams.Altitude <= zn3[0])
            {
                //LOWER STRATOSPHERE AND TROPOSPHERE(below zn3[0])
                //Temperature at nodes and gradients at end nodes
                //Inverse temperature a linear function of spherical harmonics
                meso_tgn3[0] = meso_tgn2[1];
                meso_tn3[1] = pma[3][0] * pavgm[3] / (1.0 - inputFlags.Sw[22] * Glob7s(pma[3], inputParams, inputFlags));
                meso_tn3[2] = pma[4][0] * pavgm[4] / (1.0 - inputFlags.Sw[22] * Glob7s(pma[4], inputParams, inputFlags));
                meso_tn3[3] = pma[5][0] * pavgm[5] / (1.0 - inputFlags.Sw[22] * Glob7s(pma[5], inputParams, inputFlags));
                meso_tn3[4] = pma[6][0] * pavgm[6] / (1.0 - inputFlags.Sw[22] * Glob7s(pma[6], inputParams, inputFlags));
                meso_tgn3[1] = pma[7][0] * pavgm[7] * (1.0 + inputFlags.Sw[22] * Glob7s(pma[7], inputParams, inputFlags)) * meso_tn3[4] * meso_tn3[4] / (Math.Pow((pma[6][0] * pavgm[6]), 2.0));
            }

            //Linear transition to full mixing below zn2[0]
            dmc = 0;
            if (inputParams.Altitude > zmix)
            {
                dmc = 1.0 - (zn2[0] - inputParams.Altitude) / (zn2[2] - zmix);
            }
            dz28 = gtsOutput.Densities[2];

            //N2 density
            dmr = gtsOutput.Densities[2] / dm28m - 1.0;
            gtdOutput.Densities[2] = Densm(inputParams.Altitude, dm28m, xmm, ref tz, mn3, zn3, meso_tn3, meso_tgn3, mn2, zn2, meso_tn2, meso_tgn2);
            gtdOutput.Densities[2] = gtdOutput.Densities[2] * (1.0 + dmr * dmc);
            //Console.WriteLine("output: " + Densm(inputParams.Altitude, dm28m, xmm, ref tz, mn3, zn3, meso_tn3, meso_tgn3, mn2, zn2, meso_tn2, meso_tgn2));

            //HE density
            dmr = gtsOutput.Densities[0] / (dz28 * pdm[0][1]) - 1.0;
            gtdOutput.Densities[0] = gtdOutput.Densities[2] * pdm[0][1] * (1.0 + dmr * dmc);

            //O density
            gtdOutput.Densities[1] = 0;
            gtdOutput.Densities[8] = 0;

            //O2 density
            dmr = gtsOutput.Densities[3] / (dz28 * pdm[3][1]) - 1.0;
            gtdOutput.Densities[3] = gtdOutput.Densities[2] * pdm[3][1] * (1.0 + dmr * dmc);

            //AR density
            dmr = gtsOutput.Densities[4] / (dz28 * pdm[4][1]) - 1.0;
            gtdOutput.Densities[4] = gtdOutput.Densities[2] * pdm[4][1] * (1.0 + dmr * dmc);

            //Hydrogen density
            gtdOutput.Densities[6] = 0;

            //Atomic nitrogen density
            gtdOutput.Densities[7] = 0;

            //Total mass density
            gtdOutput.Densities[5] = 1.66E-24 * (4.0 * gtdOutput.Densities[0] + 16.0 * gtdOutput.Densities[1] + 28.0 * gtdOutput.Densities[2] + 32.0 * gtdOutput.Densities[3] + 40.0 * gtdOutput.Densities[4] + gtdOutput.Densities[6] + 14.0 * gtdOutput.Densities[7]);

            if (inputFlags.Sw[0] != 0)
            {
                gtdOutput.Densities[5] = gtdOutput.Densities[5] / 1000;
            }

            //temperature at altitude
            dd = Densm(inputParams.Altitude, 1.0, 0, ref tz, mn3, zn3, meso_tn3, meso_tgn3, mn2, zn2, meso_tn2, meso_tgn2);
            gtdOutput.Temperature[1] = tz;

            return gtdOutput;
        }
        #endregion

        #region GTS7
        //Thermospheric portion of NRLMSISE-00
        //See GTD7 for more extensive comments
        //alt > 72.5 km! 
        static Output GTS7(Input inputParams, Flags inputFlags)
        {
            //Unknown variables for calculations
            double za;
            double ddum, z;
            double[] zn1 = { 120.0, 110.0, 100.0, 90.0, 72.5 };
            double tinf;
            int mn1 = 5;
            double g0;
            double tlb;
            double s;
            double db01, db04, db14, db16, db28, db32, db40;
            double zh28, zh04, zh16, zh32, zh40, zh01, zh14;
            double zhm28, zhm04, zhm16, zhm32, zhm40, zhm01, zhm14;
            double xmd;
            double b28, b04, b16, b32, b40, b01, b14;
            double tz = 0.0;
            double g28, g4, g16, g32, g40, g1, g14;
            double zhf, xmm;
            double zc04, zc16, zc32, zc40, zc01, zc14;
            double hc04, hc16, hc32, hc40, hc01, hc14;
            double hcc16, hcc32, hcc01, hcc14;
            double zcc16, zcc32, zcc01, zcc14;
            double rc16, rc32, rc01, rc14;
            double rl;
            double g16h, db16h, tho, zsht, zmho, zsho;
            double dgtr = 1.74533E-2;
            double dr = 1.72142E-2;
            double[] alpha = new double[] { -0.38, 0.0, 0.0, 0.0, 0.17, 0.0, -0.38, 0.0, 0.0 };
            double[] altl = new double[] { 200.0, 300.0, 160.0, 250.0, 240.0, 450.0, 320.0, 450.0 };
            double dd;
            double hc216, hcc232;
            Output calcOutput = new Output();

            za = pdl[1][15];
            zn1[0] = za;

            for (int i = 0; i < 9; i++)
            {
                calcOutput.Densities[i] = 0.0;
            }

            //TINF variations not important below ZA or ZN1(1)
            if (inputParams.Altitude > zn1[0])
            {
                double hb = Globe7(pt, inputParams, inputFlags);
                double shhhhh = (1.0 + inputFlags.Sw[16] * hb);
                tinf = ptm[0] * pt[0] * shhhhh;
            }
            else
            {
                //1027
                tinf = ptm[0] * pt[0];
            }
            calcOutput.Temperature[0] = tinf;

            //Gradient variations not important below zn1(5)
            if (inputParams.Altitude > zn1[4])
            {
                g0 = ptm[3] * ps[0] * (1.0 + inputFlags.Sw[19] * Globe7(ps, inputParams, inputFlags));
            }
            else
            {
                g0 = ptm[3] * ps[0];
            }

            tlb = ptm[1] * (1.0 + inputFlags.Sw[17] * Globe7(pd[3], inputParams, inputFlags)) * pd[3][0];
            s = g0 / (tinf - tlb);


            //Lower thermosphere temp variations not significant for density above 300 km
            if (inputParams.Altitude < 300.0)
            {
                meso_tn1[1] = ptm[6] * ptl[0][0] / (1.0 - inputFlags.Sw[18] * Glob7s(ptl[0], inputParams, inputFlags));
                meso_tn1[2] = ptm[2] * ptl[1][0] / (1.0 - inputFlags.Sw[18] * Glob7s(ptl[1], inputParams, inputFlags));
                meso_tn1[3] = ptm[7] * ptl[2][0] / (1.0 - inputFlags.Sw[18] * Glob7s(ptl[2], inputParams, inputFlags));
                meso_tn1[4] = ptm[4] * ptl[3][0] / (1.0 - inputFlags.Sw[18] * inputFlags.Sw[20] * Glob7s(ptl[3], inputParams, inputFlags));
                meso_tgn1[1] = ptm[8] * pma[8][0] * (1.0 + inputFlags.Sw[18] * inputFlags.Sw[20] * Glob7s(pma[8], inputParams, inputFlags)) * meso_tn1[4] * meso_tn1[4] / (Math.Pow((ptm[4] * ptl[3][0]), 2.0));
            }
            else
            {
                meso_tn1[1] = ptm[6] * ptl[0][0];
                meso_tn1[2] = ptm[2] * ptl[1][0];
                meso_tn1[3] = ptm[7] * ptl[2][0];
                meso_tn1[4] = ptm[4] * ptl[3][0];
                meso_tgn1[1] = ptm[8] * pma[8][0] * meso_tn1[4] * meso_tn1[4] / (Math.Pow((ptm[4] * ptl[3][0]), 2.0));
            }

            //N2 variation factor at zlb
            g28 = inputFlags.Sw[21] * Globe7(pd[2], inputParams, inputFlags);

            //Variation of turbopause height
            zhf = pdl[1][24] * (1.0 + inputFlags.Sw[5] * pdl[0][24] * Math.Sin(dgtr * inputParams.G_lat) * Math.Cos(dr * (inputParams.DayOfYear - pt[13])));
            calcOutput.Temperature[0] = tinf;
            xmm = pdm[2][4];
            z = inputParams.Altitude;


            //N2 DENSITY
            //Diffusive density at Zlb
            db28 = pdm[2][0] * Math.Exp(g28) * pd[2][0];
            //Diffusive density at Alt
            calcOutput.Densities[2] = Densu(z, db28, tinf, tlb, 28.0, alpha[2], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[2];
            //Turbopause
            zh28 = pdm[2][2] * zhf;
            zhm28 = pdm[2][3] * pdl[1][5];
            xmd = 28.0 - xmm;
            //Mixed density at Zlb
            b28 = Densu(zh28, db28, tinf, tlb, xmd, (alpha[2] - 1.0), ref tz, ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            if ((inputFlags.Sw[15] != 0) && (z <= altl[2]))
            {
                //Mixed density at Alt
                dm28 = Densu(z, b28, tinf, tlb, xmm, alpha[2], ref tz, ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                //Net density at Alt
                calcOutput.Densities[2] = Dnet(calcOutput.Densities[2], dm28, zhm28, xmm, 28.0);
            }


            //HE DENSITY
            //Density variation factor at Zlb
            g4 = inputFlags.Sw[21] * Globe7(pd[0], inputParams, inputFlags);
            //Diffusive density at Zlb
            db04 = pdm[0][0] * Math.Exp(g4) * pd[0][0];
            //Diffusive density at Alt
            calcOutput.Densities[0] = Densu(z, db04, tinf, tlb, 4.0, alpha[0], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[0];

            if ((inputFlags.Sw[15] != 0) && (z < altl[0]))
            {
                //Turbopause
                zh04 = pdm[0][2];
                //Mixed density at Zlb
                b04 = Densu(zh04, db04, tinf, tlb, 4.0 - xmm, alpha[0] - 1.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                //Mixed density at Alt
                dm04 = Densu(z, b04, tinf, tlb, xmm, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                zhm04 = zhm28;
                //Net density at Alt
                calcOutput.Densities[0] = Dnet(calcOutput.Densities[0], dm04, zhm04, xmm, 4.0);
                /*  Correction to specified mixing ratio at ground */
            rl = Math.Log(b28 * pdm[0][1] / b04);
                zc04 = pdm[0][4] * pdl[1][0];
                hc04 = pdm[0][5] * pdl[1][1];
                /*  Net density corrected at Alt */
                calcOutput.Densities[0] = calcOutput.Densities[0] * Ccor(z, rl, hc04, zc04);
            }


            //O DENSITY
            //Density variation factor at zlb
            g16 = inputFlags.Sw[21] * Globe7(pd[1], inputParams, inputFlags);
            //Diffusive density at Zlb
            db16 = pdm[1][0] * Math.Exp(g16) * pd[1][0];
            //Diffusive density at Alt
            calcOutput.Densities[1] = Densu(z, db16, tinf, tlb, 16.0, alpha[1], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[1];

            if ((inputFlags.Sw[15] != 0) && (z <= altl[1]))
            {
                //Turbopause
                zh16 = pdm[1][2];
                //Mixed density at Zlb
                b16 = Densu(zh16, db16, tinf, tlb, 16.0 - xmm, (alpha[1] - 1.0), ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                //Mixed density at Alt
                dm16 = Densu(z, b16, tinf, tlb, xmm, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                zhm16 = zhm28;
                //Net density at alt
                calcOutput.Densities[1] = Dnet(calcOutput.Densities[1], dm16, zhm16, xmm, 16.0);
                rl = pdm[1][1] * pdl[1][16] * (1.0 + inputFlags.Sw[1] * pdl[0][23] * (inputParams.F107A - 150.0));
                hc16 = pdm[1][5] * pdl[1][3];
                zc16 = pdm[1][4] * pdl[1][2];
                hc216 = pdm[1][5] * pdl[1][4];
                calcOutput.Densities[1] = calcOutput.Densities[1] * Ccor2(z, rl, hc16, zc16, hc216);
                //Chemistry correction
                hcc16 = pdm[1][7] * pdl[1][13];
                zcc16 = pdm[1][6] * pdl[1][12];
                rc16 = pdm[1][3] * pdl[1][14];
                //Net density corrected at Alt
                calcOutput.Densities[1] = calcOutput.Densities[1] * Ccor(z, rc16, hcc16, zcc16);
            }


            //O2 DENSITY
            //Density variation factor at zlb
            g32 = inputFlags.Sw[21] * Globe7(pd[4], inputParams, inputFlags);
            //Diffusive density at zlb
            db32 = pdm[3][0] * Math.Exp(g32) * pd[4][0];
            //Diffusive density at alt
            calcOutput.Densities[3] = Densu(z, db32, tinf, tlb, 32.0, alpha[3], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[3];

            if (inputFlags.Sw[15] != 0)
            {
                if (z <= altl[3])
                {
                    //Turbopause
                    zh32 = pdm[3][2];
                    //Mixed density at zlb
                    b32 = Densu(zh32, db32, tinf, tlb, 32.0 - xmm, alpha[3] - 1.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                    //Mixed density at alt
                    dm32 = Densu(z, b32, tinf, tlb, xmm, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                    zhm32 = zhm28;
                    //Net density at Alt
                    calcOutput.Densities[3] = Dnet(calcOutput.Densities[3], dm32, zhm32, xmm, 32.0);
                    //Correction to specified meixing ration at ground
                    rl = Math.Log(b28 * pdm[3][1] / b32);
                    hc32 = pdm[3][5] * pdl[1][7];
                    zc32 = pdm[3][4] * pdl[1][6];
                    calcOutput.Densities[3] = calcOutput.Densities[3] * Ccor(z, rl, hc32, zc32);
                }
                //Correction for general departure from diffusive equilibrium above zlb
                hcc32 = pdm[3][7] * pdl[1][22];
                hcc232 = pdm[3][7] * pdl[0][22];
                zcc32 = pdm[3][6] * pdl[1][21];
                rc32 = pdm[3][3] * pdl[1][23] * (1.0 + inputFlags.Sw[1] * pdl[0][23] * (inputParams.F107A - 150.0));
                //Net density corrected at alt
                calcOutput.Densities[3] = calcOutput.Densities[3] * Ccor2(z, rc32, hcc32, zcc32, hcc232);
            }


            //AR DENSITY
            //Density variation factor at zlb
            g40 = inputFlags.Sw[21] * Globe7(pd[5], inputParams, inputFlags);
            //Diffusive density at zlb
            db40 = pdm[4][0] * Math.Exp(g40) * pd[5][0];
            //Diffusive density at zlb
            calcOutput.Densities[4] = Densu(z, db40, tinf, tlb, 40.0, alpha[4], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[4];

            if ( (inputFlags.Sw[15] != 0) && (z <= altl[4]) )
            {
                //Turbopause
                zh40 = pdm[4][2];
                //Mixed density at zlb
                b40 = Densu(zh40, db40, tinf, tlb, 40.0 - xmm, alpha[4] - 1.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                //Mixed density at alt
                dm40 = Densu(z, b40, tinf, tlb, xmm, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                zhm40 = zhm28;
                //New density at alt
                calcOutput.Densities[4] = Dnet(calcOutput.Densities[4], dm40, zhm40, xmm, 40.0);
                //Correction to specified mixing ratio at ground
                rl = Math.Log(b28 * pdm[4][1] / b40);
                hc40 = pdm[4][5] * pdl[1][9];
                zc40 = pdm[4][4] * pdl[1][8];
                //Net density corrected at alt
                calcOutput.Densities[4] = calcOutput.Densities[4] * Ccor(z, rl, hc40, zc40);
            }


            //HYDROGEN DENSITY
            //Density variation factor at zlb
            g1 = inputFlags.Sw[21] * Globe7(pd[6], inputParams, inputFlags);
            //Diffusive density at zlb
            db01 = pdm[5][0] * Math.Exp(g1) * pd[6][0];
            //Diffusive density at alt
            calcOutput.Densities[6] = Densu(z, db01, tinf, tlb, 1.0, alpha[6], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[6];

            if ( (inputFlags.Sw[15] != 0) && (z <= altl[6]) )
            {
                //Turbopause
                zh01 = pdm[5][2];
                //Mixed density at zlb
                b01 = Densu(zh01, db01, tinf, tlb, 1.0 - xmm, alpha[6] - 1.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                //Mixed density at Alt
                dm01 = Densu(z, b01, tinf, tlb, xmm, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                zhm01 = zhm28;
                //Net density at Alt
                calcOutput.Densities[6] = Dnet(calcOutput.Densities[6], dm01, zhm01, xmm, 1.0);
                //Correction to specified mixing ratio at ground
                rl = Math.Log(b28 * pdm[5][1] * Math.Sqrt(pdl[1][17] * pdl[1][17]) / b01);
                hc01 = pdm[5][5] * pdl[1][11];
                zc01 = pdm[5][4] * pdl[1][10];
                calcOutput.Densities[6] = calcOutput.Densities[6] * Ccor(z, rl, hc01, zc01);
                //Chemistry correction
                hcc01 = pdm[5][7] * pdl[1][19];
                zcc01 = pdm[5][6] * pdl[1][18];
                rc01 = pdm[5][3] * pdl[1][20];
                //Net density corrected at Alt
                calcOutput.Densities[6] = calcOutput.Densities[6] * Ccor(z, rc01, hcc01, zcc01);
            }


            //ATOMIC NITROGEN DENSITY
            //Density variation factor at zlb
            g14 = inputFlags.Sw[21] * Globe7(pd[7], inputParams, inputFlags);
            //Diffusive density at zlb
            db14 = pdm[6][0] * Math.Exp(g14) * pd[7][0];
            //Diffusive density at alt
            calcOutput.Densities[7] = Densu(z, db14, tinf, tlb, 14.0, alpha[7], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            dd = calcOutput.Densities[7];

            if ( (inputFlags.Sw[15] != 0) && (z <= altl[7]) )
            {
                //Turbopause
                zh14 = pdm[6][2];
                //Mixed density at Zlb
                b14 = Densu(zh14, db14, tinf, tlb, 14.0 - xmm, alpha[7] - 1.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                //Mixed density at Alt
                dm14 = Densu(z, b14, tinf, tlb, xmm, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
                zhm14 = zhm28;
                //Net density at Alt
                calcOutput.Densities[7] = Dnet(calcOutput.Densities[7], dm14, zhm14, xmm, 14.0);
                //Correction to specified mixing ratio at ground
                rl = Math.Log(b28 * pdm[6][1] * Math.Sqrt(pdl[0][2] * pdl[0][2]) / b14);
                hc14 = pdm[6][5] * pdl[0][1];
                zc14 = pdm[6][4] * pdl[0][0];
                calcOutput.Densities[7] = calcOutput.Densities[7] * Ccor(z, rl, hc14, zc14);
                //Chemistry correction
                hcc14 = pdm[6][7] * pdl[0][4];
                zcc14 = pdm[6][6] * pdl[0][3];
                rc14 = pdm[6][3] * pdl[0][5];
                //Net density corrected at Alt
                calcOutput.Densities[7] = calcOutput.Densities[7] * Ccor(z, rc14, hcc14, zcc14);
            }


            //ANOMALOUS OXYGEN DENSITY
            g16h = inputFlags.Sw[21] * Globe7(pd[8], inputParams, inputFlags);
            db16h = pdm[7][0] * Math.Exp(g16h) * pd[8][0];
            tho = pdm[7][9] * pdl[0][6];
            dd = Densu(z, db16h, tho, tho, 16.0, alpha[8], ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            zsht = pdm[7][5];
            zmho = pdm[7][4];
            zsho = Scalh(zmho, 16.0, tho);
            calcOutput.Densities[8] = dd * Math.Exp(-zsht / zsho * (Math.Exp(-(z - zmho) / zsht) - 1.0));


            //Total mass density
            calcOutput.Densities[5] = 1.66e-24 * (4.0 * calcOutput.Densities[0] + 16.0 * calcOutput.Densities[1] + 28.0 * calcOutput.Densities[2] + 32.0 * calcOutput.Densities[3] + 40.0 * calcOutput.Densities[4] + calcOutput.Densities[6] + 14.0 * calcOutput.Densities[7]);

            //Temperature
            z = Math.Sqrt(inputParams.Altitude * inputParams.Altitude);
            ddum = Densu(z, 1.0, tinf, tlb, 0.0, 0.0, ref calcOutput.Temperature[1], ptm[5], s, mn1, zn1, meso_tn1, meso_tgn1);
            if (inputFlags.Sw[0] != 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    calcOutput.Densities[i] = calcOutput.Densities[i] * 1.0e6;
                }
                calcOutput.Densities[5] = calcOutput.Densities[5] / 1000;
            }
            return calcOutput;
        }
        #endregion

        #region Test Function
        public static Output[] TestGTD7()
        {
            Output[] testOutput = new Output[17];
            Input[] testInput = new Input[17];
            Flags testFlags = new Flags();
            AP aph = new AP();

            for (int i = 0; i < 7; i++)
            {
                aph.Ap_Array[i] = 100;
            }

            testFlags.Switches[0] = 0;

            for (int i = 1; i < 24; i++)
            {
                testFlags.Switches[i] = 1;
            }

            for (int i = 0; i < 17; i++)
            {
                testInput[i] = new Input();
                testInput[i].DayOfYear = 172;
                testInput[i].Year = 0;
                testInput[i].Seconds = 29000;
                testInput[i].Altitude = 400;
                testInput[i].G_lat = 60;
                testInput[i].G_long = -70;
                testInput[i].Lst = 16;
                testInput[i].F107A = 150;
                testInput[i].F107 = 150;
                testInput[i].Ap = 4;
            }

            testInput[1].DayOfYear = 81;
            testInput[2].Seconds = 75000;
            testInput[2].Altitude = 1000;
            testInput[3].Altitude = 100;
            testInput[10].Altitude = 0;
            testInput[11].Altitude = 10;
            testInput[12].Altitude = 30;
            testInput[13].Altitude = 50;
            testInput[14].Altitude = 70;
            testInput[16].Altitude = 100;
            testInput[4].G_lat = 0;
            testInput[5].G_long= 0;
            testInput[6].Lst = 4;
            testInput[7].F107A = 70;
            testInput[8].F107 = 180;
            testInput[9].Ap = 40;
            testInput[15].Ap_array = aph;
            testInput[16].Ap_array = aph;

            //Evaluate 0 to 14
            for (int i = 0; i < 15; i++)
            {
                testOutput[i] = GTD7(testInput[i], testFlags);
            }

            testFlags.Switches[9] = -1;

            for (int i = 15; i < 17; i++)
            {
                testOutput[i] = GTD7(testInput[i], testFlags);
            }
            return testOutput;
        }
        #endregion

        #region Utility Functions
        static void Glatf(double lat)
        {
            double dgtr = 1.74533E-2;
            double c2 = Math.Cos(2.0 * dgtr * lat);
            gsurf = 980.616 * (1.0 - 0.0026373 * c2);
            re = 2.0 * (gsurf) / (3.085462E-6 + 2.27E-9 * c2) * 1.0E-5;
        }

        //3hr Magnetic activity functions
        static double G0(double a, double[] p)
        {
            //Eq. A24d
            double step1 = Math.Sqrt(p[24] * p[24]);
            double step2 = a - 4.0;
            double step3 = (step2 + (p[25] - 1.0) * (step2 + (Math.Exp((-1 * step1) * step2) - 1.0) / step1));
            return step3;
        }
        static double Sumex(double ex)
        {
            //Eq. A24c
            return (1.0 + (1.0 - Math.Pow(ex, 19.0)) / (1.0 - ex) * Math.Pow(ex, 0.5));
        }
        static double Sg0(double ex, double[] p, double[] ap)
        {
            //Eq. A24a
            double numerator = (G0(ap[1], p) + (G0(ap[2], p) * ex + G0(ap[3], p) * ex * ex + G0(ap[4], p) * Math.Pow(ex, 3.0) + (G0(ap[5], p) * Math.Pow(ex, 4.0) + G0(ap[6], p) * Math.Pow(ex, 12.0)) * (1.0 - Math.Pow(ex, 8.0)) / (1.0 - ex)));
            double denominator = Sumex(ex);
            return (numerator / denominator);
        }

        static double Globe7(double[] p, Input inputParams, Flags inputFlags)
        {
            //CALCULATE G(L) FUNCTION 
            //Upper Thermosphere Parameters */
            double[] t = new double[15];
            double apd;
            double tloc;
            double c, s, c2, c4, s2;
            double sr = 7.2722E-5;
            double dgtr = 1.74533E-2;
            double dr = 1.72142E-2;
            double hr = 0.2618;
            double cd32, cd18, cd14, cd39;
            double df;
            double f1, f2;
            double tinf;
            AP apr = new AP();

            tloc = inputParams.Lst;
            for (int i = 0; i < 14; i++)
            {
                t[i] = 0.0;
            }

            //Calculate legendre polynomials
            c = Math.Sin(inputParams.G_lat * dgtr);
            s = Math.Cos(inputParams.G_lat * dgtr);
            c2 = c * c;
            c4 = c2 * c2;
            s2 = s * s;

            plg[0][1] = c;
            plg[0][2] = 0.5 * (3.0 * c2 - 1.0);
            plg[0][3] = 0.5 * (5.0 * c * c2 - 3.0 * c);
            plg[0][4] = (35.0 * c4 - 30.0 * c2 + 3.0) / 8.0;
            plg[0][5] = (63.0 * c2 * c2 * c - 70.0 * c2 * c + 15.0 * c) / 8.0;
            plg[0][6] = (11.0 * c * plg[0][5] - 5.0 * plg[0][4]) / 6.0;

            plg[1][1] = s;
            plg[1][2] = 3.0 * c * s;
            plg[1][3] = 1.5 * (5.0 * c2 - 1.0) * s;
            plg[1][4] = 2.5 * (7.0 * c2 * c - 3.0 * c) * s;
            plg[1][5] = 1.875 * (21.0 * c4 - 14.0 * c2 + 1.0) * s;
            plg[1][6] = (11.0 * c * plg[1][5] - 6.0 * plg[1][4]) / 5.0;

            plg[2][2] = 3.0 * s2;
            plg[2][3] = 15.0 * s2 * c;
            plg[2][4] = 7.5 * (7.0 * c2 - 1.0) * s2;
            plg[2][5] = 3.0 * c * plg[2][4] - 2.0 * plg[2][3];
            plg[2][6] = (11.0 * c * plg[2][5] - 7.0 * plg[2][4]) / 4.0;
            plg[2][7] = (13.0 * c * plg[2][6] - 8.0 * plg[2][5]) / 5.0;

            plg[3][3] = 15.0 * s2 * s;
            plg[3][4] = 105.0 * s2 * s * c;
            plg[3][5] = (9.0 * c * plg[3][4] - 7.0 * plg[3][3]) / 2.0;
            plg[3][6] = (11.0 * c * plg[3][5] - 8.0 * plg[3][4]) / 3.0;

            if ( !(((inputFlags.Sw[7] == 0) && (inputFlags.Sw[8] == 0)) && (inputFlags.Sw[14] == 0)) )
            {
                stloc = Math.Sin(hr * tloc);
                ctloc = Math.Cos(hr * tloc);
                s2tloc = Math.Sin(2.0 * hr * tloc);
                c2tloc = Math.Cos(2.0 * hr * tloc);
                s3tloc = Math.Sin(3.0 * hr * tloc);
                c3tloc = Math.Cos(3.0 * hr * tloc);
            }

            cd32 = Math.Cos(dr * (inputParams.DayOfYear - p[31]));
            cd18 = Math.Cos(2.0 * dr * (inputParams.DayOfYear - p[17]));
            cd14 = Math.Cos(dr * (inputParams.DayOfYear - p[13]));
            cd39 = Math.Cos(2.0 * dr * (inputParams.DayOfYear - p[38]));

            //F10.7 effect
            df = inputParams.F107 - inputParams.F107A;
            dfa = inputParams.F107A - 150.0;
            t[0] = p[19] * df * (1.0 + p[59] * dfa) + p[20] * df * df + p[21] * dfa + p[29] * Math.Pow(dfa, 2.0);
            f1 = 1.0 + (p[47] * dfa + p[19] * df + p[20] * df * df) * inputFlags.Swc[1];
            f2 = 1.0 + (p[49] * dfa + p[19] * df + p[20] * df * df) * inputFlags.Swc[1];

            //Time independent
            t[1] = (p[1] * plg[0][2] + p[2] * plg[0][4] + p[22] * plg[0][6]) + (p[14] * plg[0][2]) * dfa * inputFlags.Swc[1] + p[26] * plg[0][1];

            //Symmetrical annual
            t[2] = p[18] * cd32;

            //Symmetrical semiannual
            t[3] = (p[15] + p[16] * plg[0][2]) * cd18;

            //Asymmetrical annual
            t[4] = f1 * (p[9] * plg[0][1] + p[10] * plg[0][3]) * cd14;

            //Asymmetrical semiannual
            t[5] = p[37] * plg[0][1] * cd39;


            //Diurnal
            if (inputFlags.Sw[7] != 0)
            {
                double t71, t72;
                t71 = (p[11] * plg[1][2]) * cd14 * inputFlags.Swc[5];
                t72 = (p[12] * plg[1][2]) * cd14 * inputFlags.Swc[5];
                t[6] = f2 * ((p[3] * plg[1][1] + p[4] * plg[1][3] + p[27] * plg[1][5] + t71) * ctloc + (p[6] * plg[1][1] + p[7] * plg[1][3] + p[28] * plg[1][5] + t72) * stloc);
            }

            //Semidiurnal
            if (inputFlags.Sw[8] != 0)
            {
                double t81, t82;
                t81 = (p[23] * plg[2][3] + p[35] * plg[2][5]) * cd14 * inputFlags.Swc[5];
                t82 = (p[33] * plg[2][3] + p[36] * plg[2][5]) * cd14 * inputFlags.Swc[5];
                t[7] = f2 * ((p[5] * plg[2][2] + p[41] * plg[2][4] + t81) * c2tloc + (p[8] * plg[2][2] + p[42] * plg[2][4] + t82) * s2tloc);
            }

            //Terdiurnal
            if (inputFlags.Sw[14] != 0)
            {
                t[13] = f2 * ((p[39] * plg[3][3] + (p[93] * plg[3][4] + p[46] * plg[3][6]) * cd14 * inputFlags.Swc[5]) * s3tloc + (p[40] * plg[3][3] + (p[94] * plg[3][4] + p[48] * plg[3][6]) * cd14 * inputFlags.Swc[5]) * c3tloc);
            }

            //Magnetic activity based on daily ap
            if (inputFlags.Sw[9] == -1)
            {
                apr = inputParams.Ap_array;
                if (p[51] != 0)
                {
                    double exp1;
                    exp1 = Math.Exp(-10800.0 * Math.Sqrt(p[51] * p[51]) / (1.0 + p[138] * (45.0 - Math.Sqrt(inputParams.G_lat * inputParams.G_lat))));
                    if (exp1 > 0.99999)
                    {
                        exp1 = 0.99999;
                    }

                    if (p[24] < 1.0e-4)
                    {
                        p[24] = 1.0e-4;
                    }

                    apt[0] = Sg0(exp1, p, apr.Ap_Array);

                    if (inputFlags.Sw[9] != 0)
                    {
                        t[8] = apt[0] * (p[50] + p[96] * plg[0][2] + p[54] * plg[0][4] + (p[125] * plg[0][1] + p[126] * plg[0][3] + p[127] * plg[0][5]) * cd14 * inputFlags.Swc[5] + (p[128] * plg[1][1] + p[129] * plg[1][3] + p[130] * plg[1][5]) * inputFlags.Swc[7] * Math.Cos(hr * (tloc - p[131])));
                    }
                }
            }
            else
            {
                double p44, p45;
                apd = inputParams.Ap - 4.0;
                p44 = p[43];
                p45 = p[44];

                if (p44 < 0)
                {
                    p44 = 1.0e-5;
                }

                apdf = apd + (p45 - 1.0) * (apd + (Math.Exp(-p44 * apd) - 1.0) / p44);

                if (inputFlags.Sw[9] != 0)
                {
                    t[8] = apdf * (p[32] + p[45] * plg[0][2] + p[34] * plg[0][4] + (p[100] * plg[0][1] + p[101] * plg[0][3] + p[102] * plg[0][5]) * cd14 * inputFlags.Swc[5] + (p[121] * plg[1][1] + p[122] * plg[1][3] + p[123] * plg[1][5]) * inputFlags.Swc[7] * Math.Cos(hr * (tloc - p[124])));
                }
            }

            if ((inputFlags.Sw[10] != 0) && (inputParams.G_long > -1000.0))
            {
                //Longitudinal
                if (inputFlags.Sw[11] != 0)
                {
                    t[10] = (1.0 + p[80] * dfa * inputFlags.Swc[1]) * ((p[64] * plg[1][2] + p[65] * plg[1][4] + p[66] * plg[1][6] + p[103] * plg[1][1] + p[104] * plg[1][3] + p[105] * plg[1][5] + inputFlags.Swc[5] * (p[109] * plg[1][1] + p[110] * plg[1][3] + p[111] * plg[1][5]) * cd14) * 
                        Math.Cos(dgtr * inputParams.G_long) + (p[90] * plg[1][2] + p[91] * plg[1][4] + p[92] * plg[1][6] + p[106] * plg[1][1] + p[107] * plg[1][3] + p[108] * plg[1][5] + inputFlags.Swc[5] * (p[112] * plg[1][1] + p[113] * plg[1][3] + p[114] * plg[1][5]) * cd14)* Math.Sin(dgtr * inputParams.G_long));
                }

                //ut and mixed ut, longitude
                if (inputFlags.Sw[12] != 0)
                {
                    t[11] = (1.0 + p[95] * plg[0][1]) * (1.0 + p[81] * dfa * inputFlags.Swc[1]) * (1.0 + p[119] * plg[0][1] * inputFlags.Swc[5] * cd14) * ((p[68] * plg[0][1] + p[69] * plg[0][3] + p[70] * plg[0][5]) * Math.Cos(sr * (inputParams.Seconds - p[71])));
                    t[11] += inputFlags.Swc[11] * (p[76] * plg[2][3] + p[77] * plg[2][5] + p[78] * plg[2][7]) * Math.Cos(sr * (inputParams.Seconds - p[79]) + 2.0 * dgtr * inputParams.G_long) * (1.0 + p[137] * dfa * inputFlags.Swc[1]);
                }

                //ut, longitude magnetic activity
                if (inputFlags.Sw[13] != 0)
                {
                    if (inputFlags.Sw[9] == -1)
                    {
                        if (p[51] != 0)
                        {
                            t[12] = apt[0] * inputFlags.Swc[11] * (1.0 + p[132] * plg[0][1]) * ((p[52] * plg[1][2] + p[98] * plg[1][4] + p[67] * plg[1][6]) * Math.Cos(dgtr * (inputParams.G_long - p[97]))) + apt[0] * inputFlags.Swc[11] * inputFlags.Swc[5] *
                                (p[133] * plg[1][1] + p[134] * plg[1][3] + p[135] * plg[1][5]) * cd14 * Math.Cos(dgtr * (inputParams.G_long - p[136])) + apt[0] * inputFlags.Swc[12] *
                                (p[55] * plg[0][1] + p[56] * plg[0][3] + p[57] * plg[0][5]) * Math.Cos(sr * (inputParams.Seconds - p[58]));
                        }
                        
                    }
                    else
                    {
                        t[12] = apdf * inputFlags.Swc[11] * (1.0 + p[120] * plg[0][1]) * ((p[60] * plg[1][2] + p[61] * plg[1][4] + p[62] * plg[1][6]) * Math.Cos(dgtr * (inputParams.G_long - p[63])))
					        + apdf * inputFlags.Swc[11] * inputFlags.Swc[5] *  (p[115] * plg[1][1] + p[116] * plg[1][3] + p[117] * plg[1][5]) * cd14 * Math.Cos(dgtr*(inputParams.G_long - p[118]))
					        + apdf * inputFlags.Swc[12] * (p[83] * plg[0][1] + p[84] * plg[0][3] + p[85] * plg[0][5]) * Math.Cos(sr * (inputParams.Seconds - p[75]));
                    }
                }
            }

            //params not used: 82, 89, 99, 139-149
            tinf = p[30];

            for (int i = 0; i < 14; i++)
            {
                tinf = tinf + Math.Abs(inputFlags.Sw[i + 1]) * t[i];
            }
            return tinf;

        }

        static double Glob7s(double[] p, Input inputParams, Flags inputFlags)
        {
            //VERSION OF GLOBE FOR LOWER ATMOSPHERE 10 / 26 / 99
            double pset = 2.0;
            double[] t = new double[14];
            double tt;
            double cd32, cd18, cd14, cd39;
            double dr = 1.72142E-2;
            double dgtr = 1.74533E-2;

            /* confirm parameter set */
            if (p[99] == 0)
            {
                p[99] = pset;
            }

            if (p[99] != pset)
            {
                Console.Write("Wrong parameter set for glob7s\n");
                return -1;
            }

            for (int i = 0; i < 14; i++)
            {
                t[i] = 0.0;
            }    
                
            cd32 = Math.Cos(dr * (inputParams.DayOfYear - p[31]));
            cd18 = Math.Cos(2.0 * dr * (inputParams.DayOfYear - p[17]));
            cd14 = Math.Cos(dr * (inputParams.DayOfYear - p[13]));
            cd39 = Math.Cos(2.0 * dr * (inputParams.DayOfYear - p[38]));

            /* F10.7 */
            t[0] = p[21] * dfa;

            /* time independent */
            t[1] = p[1] * plg[0][2] + p[2] * plg[0][4] + p[22] * plg[0][6] + p[26] * plg[0][1] + p[14] * plg[0][3] + p[59] * plg[0][5];

            /* SYMMETRICAL ANNUAL */
            t[2] = (p[18] + p[47] * plg[0][2] + p[29] * plg[0][4]) * cd32;

            /* SYMMETRICAL SEMIANNUAL */
            t[3] = (p[15] + p[16] * plg[0][2] + p[30] * plg[0][4]) * cd18;

            /* ASYMMETRICAL ANNUAL */
            t[4] = (p[9] * plg[0][1] + p[10] * plg[0][3] + p[20] * plg[0][5]) * cd14;

            /* ASYMMETRICAL SEMIANNUAL */
            t[5] = (p[37] * plg[0][1]) * cd39;

            /* DIURNAL */
            if (inputFlags.Sw[7] != 0)
            {
                double t71, t72;
                t71 = p[11] * plg[1][2] * cd14 * inputFlags.Swc[5];
                t72 = p[12] * plg[1][2] * cd14 * inputFlags.Swc[5];
                t[6] = ((p[3] * plg[1][1] + p[4] * plg[1][3] + t71) * ctloc + (p[6] * plg[1][1] + p[7] * plg[1][3] + t72) * stloc);
            }

            /* SEMIDIURNAL */
            if (inputFlags.Sw[8] != 0)
            {
                double t81, t82;
                t81 = (p[23] * plg[2][3] + p[35] * plg[2][5]) * cd14 * inputFlags.Swc[5];
                t82 = (p[33] * plg[2][3] + p[36] * plg[2][5]) * cd14 * inputFlags.Swc[5];
                t[7] = ((p[5] * plg[2][2] + p[41] * plg[2][4] + t81) * c2tloc + (p[8] * plg[2][2] + p[42] * plg[2][4] + t82) * s2tloc);
            }

            /* TERDIURNAL */
            if (inputFlags.Sw[14] != 0)
            {
                t[13] = p[39] * plg[3][3] * s3tloc + p[40] * plg[3][3] * c3tloc;
            }

            /* MAGNETIC ACTIVITY */
            if (inputFlags.Sw[9] != 0)
            {
                if (inputFlags.Sw[9] == 1)
                {
                    t[8] = apdf * (p[32] + p[45] * plg[0][2] * inputFlags.Swc[2]);
                }
                    
                if (inputFlags.Sw[9] == -1)
                {
                    t[8] = (p[50] * apt[0] + p[96] * plg[0][2] * apt[0] * inputFlags.Swc[2]);
                }
                    
            }

            /* LONGITUDINAL */
            if (!((inputFlags.Sw[10] == 0) || (inputFlags.Sw[11] == 0) || (inputParams.G_long <= -1000.0)))
            {
                t[10] = (1.0 + plg[0][1] * (p[80] * inputFlags.Swc[5] * Math.Cos(dr * (inputParams.DayOfYear - p[81])) + p[85] * inputFlags.Swc[6] * Math.Cos(2.0 * dr * (inputParams.DayOfYear - p[86])))
			    + p[83] * inputFlags.Swc[3] * Math.Cos(dr * (inputParams.DayOfYear - p[84])) + p[87] * inputFlags.Swc[4] * Math.Cos(2.0 * dr * (inputParams.DayOfYear - p[88]))) * ((p[64] * plg[1][2] + p[65] * plg[1][4] + p[66] * plg[1][6]
			    + p[74] * plg[1][1] + p[75] * plg[1][3] + p[76] * plg[1][5]) * Math.Cos(dgtr * inputParams.G_long) + (p[90] * plg[1][2] + p[91] * plg[1][4] + p[92] * plg[1][6]
			    + p[77] * plg[1][1] + p[78] * plg[1][3] + p[79] * plg[1][5]) * Math.Sin(dgtr * inputParams.G_long));
            }

            tt = 0;
            for (int i = 0; i < 14; i++)
                tt += Math.Abs(inputFlags.Sw[i + 1]) * t[i];
            return tt;
        }
        
        static double Densu(double alt, double dlb, double tinf, double tlb, double xm, double alpha, ref double tz, double zlb, double s2, int mn1, double[] zn1, double[] tn1, double[] tgn1)
        {
            //Calculate temperature and density profiles for MSIS models
            //New lower thermo polynomial
            double yd2, yd1, x = 0, y;
            double rgas = 831.4;
            double densu_temp = 1.0;
            double za, z, zg2, tt, ta;
            double dta, z1 = 0, z2, t1 = 0, t2, zg, zgdif = 0;
            int mn = 0;
            int k;
            double glb;
            double expl;
            double yi;
            double densa;
            double gamma, gamm;
            double[] xs = new double[5];
            double[] ys = new double[5];
            double[] y2out = new double[5];

            //Joining altitudes of Bates and spline
            za = zn1[0];
            if (alt > za)
            {
                z = alt;
            }
            else
            {
                z = za;
            }

            //Geopotential altitude difference from ZLB
            zg2 = Zeta(z, zlb);

            //Bates termperature
            tt = tinf - (tinf - tlb) * Math.Exp(-s2 * zg2);
            ta = tt;
            tz = tt;
            densu_temp = tz;

            if (alt < za)
            {
                //Calculate temperature below ZA temperature gradient at ZA from Bates profile
                dta = (tinf - ta) * s2 * Math.Pow(((re + zlb) / (re + za)), 2.0);
                tgn1[0] = dta;
                tn1[0] = ta;

                if (alt > zn1[mn1 - 1])
                {
                    z = alt;
                }
                else
                {
                    z = zn1[mn1 - 1];
                }
                    
                mn = mn1;
                z1 = zn1[0];
                z2 = zn1[mn - 1];
                t1 = tn1[0];
                t2 = tn1[mn - 1];

                /* geopotental difference from z1 */
                zg = Zeta(z, z1);
                zgdif = Zeta(z2, z1);

                /* set up spline nodes */
                for (k = 0; k < mn; k++)
                {
                    xs[k] = Zeta(zn1[k], z1) / zgdif;
                    ys[k] = 1.0 / tn1[k];
                }

                /* end node derivatives */
                yd1 = -tgn1[0] / (t1 * t1) * zgdif;
                yd2 = -tgn1[1] / (t2 * t2) * zgdif * Math.Pow(((re + z2) / (re + z1)), 2.0);

                /* calculate spline coefficients */
                y2out = Spline(xs, ys, mn, yd1, yd2);
                x = zg / zgdif;
                y = Splint(xs, ys, y2out, mn, x);

                /* temperature at altitude */
                tz = 1.0 / y;
                densu_temp = tz;
            }

            if (xm == 0)
            {
                return densu_temp;
            }

            //Calculate density above za
            glb = gsurf / Math.Pow((1.0 + zlb / re), 2.0);
            gamma = xm * glb / (s2 * rgas * tinf);

            expl = Math.Exp(-s2 * gamma * zg2);
            if (expl > 50.0)
            {
                expl = 50.0;
            }
                
            if (tt <= 0)
            {
                expl = 50.0;
            }
              
            //Density at altitude
            densa = dlb * Math.Pow((tlb / tt), ((1.0 + alpha + gamma))) * expl;
            densu_temp = densa;
            if (alt >= za)
            {
                return densu_temp;
            }

            //Calculate density below za
            glb = gsurf / Math.Pow((1.0 + z1 / re), 2.0);
            gamm = xm * glb * zgdif / rgas;

            //Integrate spline temperatures
            yi = Splini(xs, ys, y2out, mn, x);
            expl = gamm * yi;
            if (expl > 50.0)
            {
                expl = 50.0;
            }
            if (tz <= 0)
            {
                expl = 50.0;
            }

            //Density at altitude
            densu_temp = densu_temp * Math.Pow((t1 / tz), (1.0 + alpha)) * Math.Exp(-expl);
            return densu_temp;
        }
        
        static double Densm(double alt, double d0, double xm, ref double tz, int mn3, double[] zn3, double[] tn3, double[] tgn3, int mn2, double[] zn2, double[] tn2, double[] tgn2)
        {
            //Calculate temperature and density profiles for lower atmos
            double[] xs = new double[10];
            double[] ys = new double[10];
            double[] y2out = new double[10];
            double rgas = 831.4;
            double z, z1, z2, t1, t2, zg, zgdif;
            double yd1, yd2;
            double x, y, yi;
            double expl, gamm, glb;
            int mn;
            double densm_tmp = d0;

            if (alt > zn2[0])
            {
                if (xm == 0.0)
                {
                    return tz;
                }
                else
                {
                    return d0;
                }
            }

            //Stratosphere/Mesosphere temperature
            if (alt > zn2[mn2 - 1])
            {
                z = alt;
            }
            else
            {
                z = zn2[mn2 - 1];
            }

            mn = mn2;
            z1 = zn2[0];
            z2 = zn2[mn - 1];
            t1 = tn2[0];
            t2 = tn2[mn - 1];
            zg = Zeta(z, z1);
            zgdif = Zeta(z2, z1);

            //Setup sline nodes
            for (int i = 0; i < mn; i++)
            {
                xs[i] = Zeta(zn2[i], z1) / zgdif;
                ys[i] = 1.0 / tn2[i];
            }
            yd1 = -tgn2[0] / (t1 * t1) * zgdif;
            yd2 = -tgn2[1] / (t2 * t2) * zgdif * (Math.Pow(((re + z2) / (re + z1)), 2.0));

            //Calculate spline coefficients
            y2out = Spline(xs, ys, mn, yd1, yd2);
            x = zg / zgdif;
            y = Splint(xs, ys, y2out, mn, x);

            //Temperature at altitude
            tz = 1.0 / y;
            if (xm != 0.0)
            {
                //Calculate stratosphere / mesosphere density
                glb = gsurf / (Math.Pow((1.0 + z1 / re), 2.0));
                gamm = xm * glb * zgdif / rgas;

                //Integrate temperature profile
                yi = Splini(xs, ys, y2out, mn, x);
                expl = gamm * yi;
                if (expl > 50.0)
                {
                    expl = 50.0;
                }

                //Density at altitude
                densm_tmp = densm_tmp * (t1 / tz) * Math.Exp(-expl);
            }

            if (alt > zn3[0])
            {
                if (xm == 0.0)
                {
                    return tz;
                }
                else
                {
                    return densm_tmp;
                }
            }

            //Troposphere / stratosphere temperature
            z = alt;
            mn = mn3;
            z1 = zn3[0];
            z2 = zn3[mn - 1];
            t1 = tn3[0];
            t2 = tn3[mn - 1];
            zg = Zeta(z, z1);
            zgdif = Zeta(z2, z1);

            //Setup spline nodes
            for (int i = 0; i < mn; i++)
            {
                xs[i] = Zeta(zn3[i], z1) / zgdif;
                ys[i] = 1.0 / tn3[i];
            }
            yd1 = -tgn3[0] / (t1 * t1) * zgdif;
            yd2 = -tgn3[1] / (t2 * t2) * zgdif * (Math.Pow(((re + z2) / (re + z1)), 2.0));

            //Calculate spline coefficients
            y2out = Spline(xs, ys, mn, yd1, yd2);
            x = zg / zgdif;
            y = Splint(xs, ys, y2out, mn, x);

            //Temperature at altitude
            tz = 1.0 / y;
            if (xm != 0.0)
            {
                //Calculate tropospheric / stratosphere density
                glb = gsurf / (Math.Pow((1.0 + z1 / re), 2.0));
                gamm = xm * glb * zgdif / rgas;

                //Intefrate temperature profile
                yi = Splini(xs, ys, y2out, mn, x);
                expl = gamm * yi;
                if (expl > 50.0)
                {
                    expl = 50.0;
                }

                //Density at altitude
                densm_tmp = densm_tmp * (t1 / tz) * Math.Exp(-expl);
            }

            if (xm == 0.0)
            {
                return tz;
            }
            else
            {
                return densm_tmp;
            }
        }
        
        static double Zeta(double zz, double zl)
        {
            return ((zz - zl) * (re + zl) / (re + zz));
        }

        static double[] Spline(double[] x, double[] y, int n, double yp1, double ypn)
        {
            //CALCULATE 2ND DERIVATIVES OF CUBIC SPLINE INTERP FUNCTION
            //ADAPTED FROM NUMERICAL RECIPES BY PRESS ET AL
            //X,Y: ARRAYS OF TABULATED FUNCTION IN ASCENDING ORDER BY X
            //N: SIZE OF ARRAYS X,Y
            //YP1,YPN: SPECIFIED DERIVATIVES AT X[0] AND X[N-1]; VALUES >= 1E30 SIGNAL SIGNAL SECOND DERIVATIVE ZERO
            //RETURN: OUTPUT ARRAY OF SECOND DERIVATIVES

            double sig, p, qn, un;
            double[] u = new double[n];
            double[] output = new double[10];

            if (yp1 > 0.99e30)
            {
                output[0] = 0;
                u[0] = 0;
            }
            else
            {
                output[0] = -0.5;
                u[0] = (3.0 / (x[1] - x[0])) * ((y[1] - y[0]) / (x[1] - x[0]) - yp1);
            }

            for (int i = 1; i < (n - 1); i++)
            {
                sig = (x[i] - x[i - 1]) / (x[i + 1] - x[i - 1]);
                p = sig * output[i - 1] + 2.0;
                output[i] = (sig - 1.0) / p;
                u[i] = (6.0 * ((y[i + 1] - y[i]) / (x[i + 1] - x[i]) - (y[i] - y[i - 1]) / (x[i] - x[i - 1])) / (x[i + 1] - x[i - 1]) - sig * u[i - 1]) / p;
            }

            if (ypn > 0.99E30)
            {
                qn = 0;
                un = 0;
            }
            else
            {
                qn = 0.5;
                un = (3.0 / (x[n - 1] - x[n - 2])) * (ypn - (y[n - 1] - y[n - 2]) / (x[n - 1] - x[n - 2]));
            }
            output[n - 1] = (un - qn * u[n - 2]) / (qn * output[n - 2] + 1.0);

            for (int i = n-2; i >= 0; i--)
            {
                output[i] = output[i] * output[i + 1] + u[i];
            }
            return output;
        }
        
        static double Splint(double[] xa, double[] ya, double[] y2a, int n, double x)
        {
            //CALCULATE CUBIC SPLINE INTERP VALUE
            //ADAPTED FROM NUMERICAL RECIPES BY PRESS ET AL.
            //XA,YA: ARRAYS OF TABULATED FUNCTION IN ASCENDING ORDER BY X
            //Y2A: ARRAY OF SECOND DERIVATIVES
            //N: SIZE OF ARRAYS XA,YA,Y2A
            //X: ABSCISSA FOR INTERPOLATION
            //RETURN: OUTPUT VALUE
            int klo = 0;
            int khi = n - 1;
            int k;
            double h;
            double a, b, yi;

            while ((khi - klo) > 1)
            {
                k = (khi + klo) / 2;
                if (xa[k] > x)
                {
                    khi = k;
                }  
                else
                {
                    klo = k;
                }
            }

            h = xa[khi] - xa[klo];

            if (h == 0.0)
            {
                Console.Write("bad XA input to splint");
            }
 
            a = (xa[khi] - x) / h;
            b = (x - xa[klo]) / h;
            yi = a * ya[klo] + b * ya[khi] + ((a * a * a - a) * y2a[klo] + (b * b * b - b) * y2a[khi]) * h * h / 6.0;
            return yi;
        }

        static double Splini(double[] xa, double[] ya, double[] y2a, int n, double x)
        {
            //INTEGRATE CUBIC SPLINE FUNCTION FROM XA(1) TO X
            //XA,YA: ARRAYS OF TABULATED FUNCTION IN ASCENDING ORDER BY X
            //Y2A: ARRAY OF SECOND DERIVATIVES
            //N: SIZE OF ARRAYS XA,YA,Y2A
            //X: ABSCISSA ENDPOINT FOR INTEGRATION
            //RETURN: OUTPUT VALUE
            double yi = 0;
            int klo = 0;
            int khi = 1;
            double xx, h, a, b, a2, b2;

            while ((x > xa[klo]) && (khi < n))
            {
                xx = x;
                if (khi < (n - 1))
                {
                    if (x < xa[khi])
                    {
                        xx = x;
                    }
                    else
                    {
                        xx = xa[khi];
                    }  
                }

                h = xa[khi] - xa[klo];
                a = (xa[khi] - xx) / h;
                b = (xx - xa[klo]) / h;

                a2 = a * a;
                b2 = b * b;

                yi += ((1.0 - a2) * ya[klo] / 2.0 + b2 * ya[khi] / 2.0 + ((-(1.0 + a2 * a2) / 4.0 + a2 / 2.0) * y2a[klo] + (b2 * b2 / 4.0 - b2 / 2.0) * y2a[khi]) * h * h / 6.0) * h;
               
                klo++;
                khi++;
            }
            return yi;
        }

        static double Dnet(double dd, double dm, double zhm, double xmm, double xm)
        {
            //TURBOPAUSE CORRECTION FOR MSIS MODELS
            //Root mean density
            //DD - diffusive density
            //DM - full mixed density
            //ZHM - transition scale length
            //XMM - full mixed molecular weight
            //XM  - species molecular weight
            //DNET - combined density

            double a;
            double ylog;
            a = zhm / (xmm - xm);
            if (!((dm > 0) && (dd > 0)))
            {
                Console.Write("dnet log error " + dm + " " + dd + " " + xm + "\n");

                if ((dd == 0) && (dm == 0))
                {
                    dd = 1;
                }
                    
                if (dm == 0)
                {
                    return dd;
                }
                    
                if (dd == 0)
                {
                    return dm;
                }
            }

            ylog = a * Math.Log(dm / dd);

            if (ylog < -10)
            {
                return dd;
            }
               
            if (ylog > 10)
            {
                return dm;
            }
                
            a = dd * Math.Pow( (1.0 + Math.Exp(ylog)), (1.0 / a) );
            return a;
        }

        static double Ccor(double alt, double r, double h1, double zh)
        {
            //CHEMISTRY/DISSOCIATION CORRECTION FOR MSIS MODELS
            //ALT - altitude
            //R - target ratio
            //H1 - transition scale length
            //ZH - altitude of 1/2 R
            double e;
            double ex;

            e = (alt - zh) / h1;

            if (e > 70)
            {
                return Math.Exp(0);
            }
                
            if (e < -70)
            {
                return Math.Exp(r);
            }
            
            ex = Math.Exp(e);
            e = r / (1.0 + ex);

            if (Math.Exp(e) < 1.00e-6)
            {
                e = Double.NegativeInfinity;
            }

            return Math.Exp(e);
        }

        static double Ccor2(double alt, double r, double h1, double zh, double h2)
        {
            //CHEMISTRY/DISSOCIATION CORRECTION FOR MSIS MODELS
            //ALT - altitude
            //R - target ratio
            //H1 - transition scale length
            //ZH - altitude of 1/2 R
            //H2 - transition scale length #2 ?
            double e1, e2;
            double ex1, ex2;
            double ccor2v;

            e1 = (alt - zh) / h1;
            e2 = (alt - zh) / h2;

            if ( (e1 > 70) || (e2 > 70) )
            {
                return Math.Exp(0);
            }

            if ( (e1 < -70) && (e2 < -70) )
            {
                return Math.Exp(r);
            }

            ex1 = Math.Exp(e1);
            ex2 = Math.Exp(e2);

            ccor2v = r / (1.0 + 0.5 * (ex1 + ex2));
            return Math.Exp(ccor2v);
        }

        static double Scalh(double alt, double xm, double temp)
        {
            double g;
            double rgas = 831.4;
            g = gsurf / (Math.Pow((1.0 + alt / re), 2.0));
            g = rgas * temp / (g * xm);
            return g;
;        }
        #endregion

    }
}
