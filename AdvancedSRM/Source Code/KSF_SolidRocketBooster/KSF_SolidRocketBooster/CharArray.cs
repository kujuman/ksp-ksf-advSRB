/*
 * Kerbal Science Foundation Advanced Solid Rocket Booster v0.6.1 for Kerbal Space Program
 * Released May 4, 2014 under a Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License
 * For attribution, please attribute "kujuman"
 */

using UnityEngine;

namespace KSF_SolidRocketBooster
{
    public class KSF_CharArray
    {
        public System.Collections.Generic.List<Vector2> pixelList = new System.Collections.Generic.List<Vector2>();
        public string charName;
        public int charWidth;
    }

    public static class KSF_CharArrayUtils
    {
        public static KSF_CharArray Spacech = new KSF_CharArray();
        public static KSF_CharArray Sch = new KSF_CharArray();
        public static KSF_CharArray Tch = new KSF_CharArray();
        public static KSF_CharArray Ach = new KSF_CharArray();
        public static KSF_CharArray Cch = new KSF_CharArray();
        public static KSF_CharArray ch1 = new KSF_CharArray();
        public static KSF_CharArray ch2 = new KSF_CharArray();
        public static KSF_CharArray ch3 = new KSF_CharArray();
        public static KSF_CharArray ch4 = new KSF_CharArray();
        public static KSF_CharArray ch5 = new KSF_CharArray();
        public static KSF_CharArray ch6 = new KSF_CharArray();
        public static KSF_CharArray ch7 = new KSF_CharArray();
        public static KSF_CharArray ch8 = new KSF_CharArray();
        public static KSF_CharArray ch9 = new KSF_CharArray();
        public static KSF_CharArray ch0 = new KSF_CharArray();
        public static KSF_CharArray chs = new KSF_CharArray();
        public static KSF_CharArray chk = new KSF_CharArray();
        public static KSF_CharArray cht = new KSF_CharArray();


        public static System.Collections.Generic.List<KSF_CharArray> convertStringToCharArray(string s)
        {
            System.Collections.Generic.List<KSF_CharArray> chArray = new System.Collections.Generic.List<KSF_CharArray>();
            chArray.Clear();

            foreach (char c in s)
            {
                switch (c)
                {
                    case '1':
                        chArray.Add(ch1);
                        break;

                    case '2':
                        chArray.Add(ch2);
                        break;

                    case '3':
                        chArray.Add(ch3);
                        break;

                    case '4':
                        chArray.Add(ch4);
                        break;

                    case '5':
                        chArray.Add(ch5);
                        break;

                    case '6':
                        chArray.Add(ch6);
                        break;

                    case '7':
                        chArray.Add(ch7);
                        break;

                    case '8':
                        chArray.Add(ch8);
                        break;

                    case '9':
                        chArray.Add(ch9);
                        break;

                    case '0':
                        chArray.Add(ch0);
                        break;

                    case 's':
                        chArray.Add(chs);
                        break;

                    case 'A':
                        chArray.Add(Ach);
                        break;

                    case ' ':
                        chArray.Add(Spacech);
                        break;

                    case 'k':
                        chArray.Add(chk);
                        break;

                    case 't':
                        chArray.Add(cht);
                        break;


                    default:
                        break;
                }
            }

            return chArray;
        }

        public static void populateCharArrays()
        {
            Spacech.pixelList.Clear();
            Spacech.charName = "Space";
            Spacech.charWidth = 2;

            Sch.pixelList.Clear();
            Sch.charName = "S";
            Sch.charWidth = 5;
            Sch.pixelList.Add(new Vector2(0, 5));
            Sch.pixelList.Add(new Vector2(0, 4));
            Sch.pixelList.Add(new Vector2(0, 1));
            Sch.pixelList.Add(new Vector2(1, 6));
            Sch.pixelList.Add(new Vector2(1, 3));
            Sch.pixelList.Add(new Vector2(1, 0));
            Sch.pixelList.Add(new Vector2(2, 6));
            Sch.pixelList.Add(new Vector2(2, 3));
            Sch.pixelList.Add(new Vector2(2, 0));
            Sch.pixelList.Add(new Vector2(3, 5));
            Sch.pixelList.Add(new Vector2(3, 4));
            Sch.pixelList.Add(new Vector2(3, 1));

            Tch.pixelList.Clear();
            Tch.charName = "T";
            Tch.charWidth = 6;
            Tch.pixelList.Add(new Vector2(0, 6));
            Tch.pixelList.Add(new Vector2(1, 6));
            Tch.pixelList.Add(new Vector2(2, 6));
            Tch.pixelList.Add(new Vector2(3, 6));
            Tch.pixelList.Add(new Vector2(4, 6));
            Tch.pixelList.Add(new Vector2(2, 1));
            Tch.pixelList.Add(new Vector2(2, 2));
            Tch.pixelList.Add(new Vector2(2, 3));
            Tch.pixelList.Add(new Vector2(2, 4));
            Tch.pixelList.Add(new Vector2(2, 5));
            Tch.pixelList.Add(new Vector2(2, 0));

            Ach.pixelList.Clear();
            Ach.charName = "A";
            Ach.charWidth = 6;
            Ach.pixelList.Add(new Vector2(0, 3));
            Ach.pixelList.Add(new Vector2(0, 2));
            Ach.pixelList.Add(new Vector2(0, 1));
            Ach.pixelList.Add(new Vector2(0, 0));
            Ach.pixelList.Add(new Vector2(4, 3));
            Ach.pixelList.Add(new Vector2(4, 2));
            Ach.pixelList.Add(new Vector2(4, 1));
            Ach.pixelList.Add(new Vector2(4, 0));
            Ach.pixelList.Add(new Vector2(1, 5));
            Ach.pixelList.Add(new Vector2(1, 4));
            Ach.pixelList.Add(new Vector2(1, 2));
            Ach.pixelList.Add(new Vector2(3, 5));
            Ach.pixelList.Add(new Vector2(3, 4));
            Ach.pixelList.Add(new Vector2(3, 2));
            Ach.pixelList.Add(new Vector2(2, 6));
            Ach.pixelList.Add(new Vector2(2, 2));

            Cch.pixelList.Clear();
            Cch.charName = "C";
            Cch.charWidth = 6;
            Cch.pixelList.Add(new Vector2(0, 4));
            Cch.pixelList.Add(new Vector2(0, 3));
            Cch.pixelList.Add(new Vector2(0, 2));
            Cch.pixelList.Add(new Vector2(1, 5));
            Cch.pixelList.Add(new Vector2(1, 1));
            Cch.pixelList.Add(new Vector2(2, 6));
            Cch.pixelList.Add(new Vector2(2, 0));
            Cch.pixelList.Add(new Vector2(3, 6));
            Cch.pixelList.Add(new Vector2(3, 0));
            Cch.pixelList.Add(new Vector2(4, 5));
            Cch.pixelList.Add(new Vector2(4, 1));



            ch1.pixelList.Clear();
            ch1.charName = "1";
            ch1.charWidth = 5;
            ch1.pixelList.Add(new Vector2(0, 0));
            ch1.pixelList.Add(new Vector2(0, 4));
            ch1.pixelList.Add(new Vector2(1, 0));
            ch1.pixelList.Add(new Vector2(1, 1));
            ch1.pixelList.Add(new Vector2(1, 2));
            ch1.pixelList.Add(new Vector2(1, 3));
            ch1.pixelList.Add(new Vector2(1, 4));
            ch1.pixelList.Add(new Vector2(1, 5));
            ch1.pixelList.Add(new Vector2(2, 0));
            ch1.pixelList.Add(new Vector2(2, 1));
            ch1.pixelList.Add(new Vector2(2, 2));
            ch1.pixelList.Add(new Vector2(2, 3));
            ch1.pixelList.Add(new Vector2(2, 4));
            ch1.pixelList.Add(new Vector2(2, 5));
            ch1.pixelList.Add(new Vector2(2, 6));
            ch1.pixelList.Add(new Vector2(3, 0));

            ch2.pixelList.Clear();
            ch2.charName = "2";
            ch2.charWidth = 6;
            ch2.pixelList.Add(new Vector2(0, 0));
            ch2.pixelList.Add(new Vector2(0, 4));
            ch2.pixelList.Add(new Vector2(0, 5));
            ch2.pixelList.Add(new Vector2(1, 0));
            ch2.pixelList.Add(new Vector2(1, 1));
            ch2.pixelList.Add(new Vector2(1, 5));
            ch2.pixelList.Add(new Vector2(1, 6));
            ch2.pixelList.Add(new Vector2(2, 0));
            ch2.pixelList.Add(new Vector2(2, 1));
            ch2.pixelList.Add(new Vector2(2, 2));
            ch2.pixelList.Add(new Vector2(2, 6));
            ch2.pixelList.Add(new Vector2(3, 0));
            ch2.pixelList.Add(new Vector2(3, 2));
            ch2.pixelList.Add(new Vector2(3, 3));
            ch2.pixelList.Add(new Vector2(3, 5));
            ch2.pixelList.Add(new Vector2(3, 6));
            ch2.pixelList.Add(new Vector2(4, 0));
            ch2.pixelList.Add(new Vector2(4, 3));
            ch2.pixelList.Add(new Vector2(4, 4));
            ch2.pixelList.Add(new Vector2(4, 5));

            ch3.pixelList.Clear();
            ch3.charName = "3";
            ch3.charWidth = 6;
            ch3.pixelList.Add(new Vector2(0, 1));
            ch3.pixelList.Add(new Vector2(0, 2));
            ch3.pixelList.Add(new Vector2(0, 4));
            ch3.pixelList.Add(new Vector2(0, 5));
            ch3.pixelList.Add(new Vector2(1, 0));
            ch3.pixelList.Add(new Vector2(1, 1));
            ch3.pixelList.Add(new Vector2(1, 5));
            ch3.pixelList.Add(new Vector2(1, 6));
            ch3.pixelList.Add(new Vector2(2, 0));
            ch3.pixelList.Add(new Vector2(2, 3));
            ch3.pixelList.Add(new Vector2(2, 6));
            ch3.pixelList.Add(new Vector2(3, 0));
            ch3.pixelList.Add(new Vector2(3, 1));
            ch3.pixelList.Add(new Vector2(3, 2));
            ch3.pixelList.Add(new Vector2(3, 3));
            ch3.pixelList.Add(new Vector2(3, 4));
            ch3.pixelList.Add(new Vector2(3, 5));
            ch3.pixelList.Add(new Vector2(3, 6));
            ch3.pixelList.Add(new Vector2(4, 1));
            ch3.pixelList.Add(new Vector2(4, 2));
            ch3.pixelList.Add(new Vector2(4, 4));
            ch3.pixelList.Add(new Vector2(4, 5));

            ch4.pixelList.Clear();
            ch4.charName = "4";
            ch4.charWidth = 6;
            ch4.pixelList.Add(new Vector2(0, 2));
            ch4.pixelList.Add(new Vector2(0, 3));
            ch4.pixelList.Add(new Vector2(0, 4));
            ch4.pixelList.Add(new Vector2(1, 2));
            ch4.pixelList.Add(new Vector2(1, 4));
            ch4.pixelList.Add(new Vector2(1, 5));
            ch4.pixelList.Add(new Vector2(2, 2));
            ch4.pixelList.Add(new Vector2(3, 0));
            ch4.pixelList.Add(new Vector2(3, 1));
            ch4.pixelList.Add(new Vector2(3, 2));
            ch4.pixelList.Add(new Vector2(3, 3));
            ch4.pixelList.Add(new Vector2(3, 4));
            ch4.pixelList.Add(new Vector2(3, 5));
            ch4.pixelList.Add(new Vector2(3, 6));
            ch4.pixelList.Add(new Vector2(4, 0));
            ch4.pixelList.Add(new Vector2(4, 1));
            ch4.pixelList.Add(new Vector2(4, 2));
            ch4.pixelList.Add(new Vector2(4, 3));
            ch4.pixelList.Add(new Vector2(4, 4));
            ch4.pixelList.Add(new Vector2(4, 5));
            ch4.pixelList.Add(new Vector2(4, 6));

            ch5.pixelList.Clear();
            ch5.charName = "5";
            ch5.charWidth = 5;
            ch5.pixelList.Add(new Vector2(3, 1));
            ch5.pixelList.Add(new Vector2(3, 2));
            ch5.pixelList.Add(new Vector2(3, 6));
            ch5.pixelList.Add(new Vector2(1, 0));
            ch5.pixelList.Add(new Vector2(1, 3));
            ch5.pixelList.Add(new Vector2(1, 6));
            ch5.pixelList.Add(new Vector2(2, 0));
            ch5.pixelList.Add(new Vector2(2, 1));
            ch5.pixelList.Add(new Vector2(2, 2));
            ch5.pixelList.Add(new Vector2(2, 3));
            ch5.pixelList.Add(new Vector2(2, 6));
            ch5.pixelList.Add(new Vector2(0, 0));
            ch5.pixelList.Add(new Vector2(0, 3));
            ch5.pixelList.Add(new Vector2(0, 4));
            ch5.pixelList.Add(new Vector2(0, 5));
            ch5.pixelList.Add(new Vector2(0, 6));

            ch6.pixelList.Clear();
            ch6.charName = "6";
            ch6.charWidth = 6;
            ch6.pixelList.Add(new Vector2(0, 1));
            ch6.pixelList.Add(new Vector2(0, 2));
            ch6.pixelList.Add(new Vector2(0, 3));
            ch6.pixelList.Add(new Vector2(1, 0));
            ch6.pixelList.Add(new Vector2(1, 3));
            ch6.pixelList.Add(new Vector2(1, 4));
            ch6.pixelList.Add(new Vector2(1, 5));
            ch6.pixelList.Add(new Vector2(2, 0));
            ch6.pixelList.Add(new Vector2(2, 3));
            ch6.pixelList.Add(new Vector2(2, 5));
            ch6.pixelList.Add(new Vector2(2, 6));
            ch6.pixelList.Add(new Vector2(3, 0));
            ch6.pixelList.Add(new Vector2(3, 1));
            ch6.pixelList.Add(new Vector2(3, 2));
            ch6.pixelList.Add(new Vector2(3, 3));
            ch6.pixelList.Add(new Vector2(3, 6));
            ch6.pixelList.Add(new Vector2(4, 1));
            ch6.pixelList.Add(new Vector2(4, 2));
            ch6.pixelList.Add(new Vector2(4, 6));

            ch7.pixelList.Clear();
            ch7.charName = "7";
            ch7.charWidth = 5;
            ch7.pixelList.Add(new Vector2(0, 5));
            ch7.pixelList.Add(new Vector2(0, 6));
            ch7.pixelList.Add(new Vector2(1, 0));
            ch7.pixelList.Add(new Vector2(1, 1));
            ch7.pixelList.Add(new Vector2(1, 5));
            ch7.pixelList.Add(new Vector2(1, 6));
            ch7.pixelList.Add(new Vector2(2, 1));
            ch7.pixelList.Add(new Vector2(2, 2));
            ch7.pixelList.Add(new Vector2(2, 3));
            ch7.pixelList.Add(new Vector2(2, 5));
            ch7.pixelList.Add(new Vector2(2, 6));
            ch7.pixelList.Add(new Vector2(3, 3));
            ch7.pixelList.Add(new Vector2(3, 4));
            ch7.pixelList.Add(new Vector2(3, 5));
            ch7.pixelList.Add(new Vector2(3, 6));

            ch8.pixelList.Clear();
            ch8.charName = "8";
            ch8.charWidth = 6;
            ch8.pixelList.Add(new Vector2(0, 1));
            ch8.pixelList.Add(new Vector2(0, 2));
            ch8.pixelList.Add(new Vector2(0, 4));
            ch8.pixelList.Add(new Vector2(0, 5));
            ch8.pixelList.Add(new Vector2(1, 0));
            ch8.pixelList.Add(new Vector2(1, 3));
            ch8.pixelList.Add(new Vector2(1, 6));
            ch8.pixelList.Add(new Vector2(2, 0));
            ch8.pixelList.Add(new Vector2(2, 3));
            ch8.pixelList.Add(new Vector2(2, 6));
            ch8.pixelList.Add(new Vector2(3, 0));
            ch8.pixelList.Add(new Vector2(3, 1));
            ch8.pixelList.Add(new Vector2(3, 2));
            ch8.pixelList.Add(new Vector2(3, 3));
            ch8.pixelList.Add(new Vector2(3, 4));
            ch8.pixelList.Add(new Vector2(3, 5));
            ch8.pixelList.Add(new Vector2(3, 6));
            ch8.pixelList.Add(new Vector2(4, 1));
            ch8.pixelList.Add(new Vector2(4, 2));
            ch8.pixelList.Add(new Vector2(4, 5));
            ch8.pixelList.Add(new Vector2(4, 6));

            ch9.pixelList.Clear();
            ch9.charName = "9";
            ch9.charWidth = 6;
            ch9.pixelList.Add(new Vector2(0, 3));
            ch9.pixelList.Add(new Vector2(0, 4));
            ch9.pixelList.Add(new Vector2(0, 5));
            ch9.pixelList.Add(new Vector2(1, 2));
            ch9.pixelList.Add(new Vector2(1, 3));
            ch9.pixelList.Add(new Vector2(1, 5));
            ch9.pixelList.Add(new Vector2(1, 6));
            ch9.pixelList.Add(new Vector2(2, 2));
            ch9.pixelList.Add(new Vector2(2, 6));
            ch9.pixelList.Add(new Vector2(3, 0));
            ch9.pixelList.Add(new Vector2(3, 1));
            ch9.pixelList.Add(new Vector2(3, 2));
            ch9.pixelList.Add(new Vector2(3, 3));
            ch9.pixelList.Add(new Vector2(3, 4));
            ch9.pixelList.Add(new Vector2(3, 5));
            ch9.pixelList.Add(new Vector2(3, 6));
            ch9.pixelList.Add(new Vector2(4, 0));
            ch9.pixelList.Add(new Vector2(4, 1));
            ch9.pixelList.Add(new Vector2(4, 2));
            ch9.pixelList.Add(new Vector2(4, 3));
            ch9.pixelList.Add(new Vector2(4, 4));
            ch9.pixelList.Add(new Vector2(4, 5));

            ch0.pixelList.Clear();
            ch0.charName = "0";
            ch0.charWidth = 6;
            ch0.pixelList.Add(new Vector2(1, 1));
            ch0.pixelList.Add(new Vector2(1, 1));
            ch0.pixelList.Add(new Vector2(1, 2));
            ch0.pixelList.Add(new Vector2(1, 5));
            ch0.pixelList.Add(new Vector2(1, 6));
            ch0.pixelList.Add(new Vector2(2, 0));
            ch0.pixelList.Add(new Vector2(2, 3));
            ch0.pixelList.Add(new Vector2(2, 6));
            ch0.pixelList.Add(new Vector2(3, 0));
            ch0.pixelList.Add(new Vector2(3, 1));
            ch0.pixelList.Add(new Vector2(3, 4));
            ch0.pixelList.Add(new Vector2(3, 5));
            ch0.pixelList.Add(new Vector2(3, 6));
            ch0.pixelList.Add(new Vector2(4, 1));
            ch0.pixelList.Add(new Vector2(4, 2));
            ch0.pixelList.Add(new Vector2(4, 3));
            ch0.pixelList.Add(new Vector2(4, 4));
            ch0.pixelList.Add(new Vector2(4, 5));
            ch0.pixelList.Add(new Vector2(0, 1));
            ch0.pixelList.Add(new Vector2(0, 2));
            ch0.pixelList.Add(new Vector2(0, 3));
            ch0.pixelList.Add(new Vector2(0, 4));
            ch0.pixelList.Add(new Vector2(0, 5));

            chs.pixelList.Clear();
            chs.charName = "sec";
            chs.charWidth = 4;
            chs.pixelList.Add(new Vector2(0, 0));
            chs.pixelList.Add(new Vector2(0, 2));
            chs.pixelList.Add(new Vector2(0, 3));
            chs.pixelList.Add(new Vector2(0, 4));
            chs.pixelList.Add(new Vector2(2, 0));
            chs.pixelList.Add(new Vector2(2, 2));
            chs.pixelList.Add(new Vector2(2, 1));
            chs.pixelList.Add(new Vector2(2, 4));
            chs.pixelList.Add(new Vector2(1, 0));
            chs.pixelList.Add(new Vector2(1, 2));
            chs.pixelList.Add(new Vector2(1, 4));

            chk.pixelList.Clear();
            chk.charName = "kN";
            chk.charWidth = 10;
            chk.pixelList.Add(new Vector2(0, 0));
            chk.pixelList.Add(new Vector2(0, 1));
            chk.pixelList.Add(new Vector2(0, 2));
            chk.pixelList.Add(new Vector2(0, 3));
            chk.pixelList.Add(new Vector2(0, 4));
            chk.pixelList.Add(new Vector2(1, 1));
            chk.pixelList.Add(new Vector2(2, 0));
            chk.pixelList.Add(new Vector2(2, 2));
            chk.pixelList.Add(new Vector2(4, 0));
            chk.pixelList.Add(new Vector2(4, 1));
            chk.pixelList.Add(new Vector2(4, 2));
            chk.pixelList.Add(new Vector2(4, 3));
            chk.pixelList.Add(new Vector2(4, 4));
            chk.pixelList.Add(new Vector2(4, 5));
            chk.pixelList.Add(new Vector2(5, 4));
            chk.pixelList.Add(new Vector2(5, 5));
            chk.pixelList.Add(new Vector2(6, 2));
            chk.pixelList.Add(new Vector2(6, 3));
            chk.pixelList.Add(new Vector2(7, 1));
            chk.pixelList.Add(new Vector2(7, 0));
            chk.pixelList.Add(new Vector2(8, 0));
            chk.pixelList.Add(new Vector2(8, 1));
            chk.pixelList.Add(new Vector2(8, 2));
            chk.pixelList.Add(new Vector2(8, 3));
            chk.pixelList.Add(new Vector2(8, 4));
            chk.pixelList.Add(new Vector2(8, 5));

            cht.pixelList.Clear();
            cht.charName = "t";
            cht.charWidth = 4;
            cht.pixelList.Add(new Vector2(0, 3));
            cht.pixelList.Add(new Vector2(1, 0));
            cht.pixelList.Add(new Vector2(1, 1));
            cht.pixelList.Add(new Vector2(1, 2));
            cht.pixelList.Add(new Vector2(1, 3));
            cht.pixelList.Add(new Vector2(1, 4));
            cht.pixelList.Add(new Vector2(2, 0));
            cht.pixelList.Add(new Vector2(2, 3));
        }
    }
}

