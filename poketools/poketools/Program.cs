using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace poketools
{
    class Program
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal,
                                                        int size, string filePath);
        static void Main(string[] args)
        {
            try
            {
                if (args[1].ToUpper() == "HEX")
                {
                    byte[] rom = File.ReadAllBytes(args[0]);
                    rom[int.Parse(args[2], System.Globalization.NumberStyles.HexNumber)] = Convert.ToByte(args[3]);
                    File.WriteAllBytes(args[0], rom);
                }
                if (args[1].ToUpper() == "SIZE")
                {
                    byte[] rom = File.ReadAllBytes(args[0]);
                    Array.Resize(ref rom, int.Parse(args[2], System.Globalization.NumberStyles.HexNumber));
                    File.WriteAllBytes(args[0], rom);
                }
                if (args[1].ToUpper() == "CHARMAP")
                {
                    string[] tbl = File.ReadAllLines(args[0]);
                    string output = "";
                    foreach (string line in tbl)
                    {
                        output += "charmap \"" + line.Split('=')[1] + "\", $" + line.Split('=')[0] + "\n";
                    }
                    File.WriteAllText("output.asm", output);
                }
                if (args[1].ToUpper() == "CHARCONVERT")
                {
                    Console.WriteLine("로드된 파일명 : " + args[0]);
                    string[] script = File.ReadAllLines(args[0]);
                    string[] tbl = File.ReadAllLines(args[2]);
                    string convert = "";
                    Dictionary<string, string> table = new Dictionary<string, string>();
                    foreach (string line in tbl)
                    {
                        string h = "$";
                        for (int i = 0; i < line.Split('=')[0].Length; i++)
                        {

                            if (i % 2 == 0 && i > 0 && i != line.Split('=')[0].Length - 1) h += ",$";
                            h += line.Split('=')[0][i];
                        }
                        table.Add(line.Split('=')[1], h);
                    }
                    foreach (string rawline in script)
                    {
                        if (rawline.Contains('"'))
                        {
                            try
                            {
                                string head = rawline.Split('"')[0];
                                string line = rawline.Split('"')[1].Split('"')[0];
                                if (head.Contains(";")) convert += rawline + "\n";
                                else if (line != "")
                                {
                                    string hexed = "";
                                    for (int i = 0; i < line.Length; i++)
                                    {
                                        hexed += table[line[i].ToString()];
                                        if (i != line.Length - 1) hexed += ",";
                                    }
                                    hexed.Remove(hexed.Length - 2);
                                    convert += head + hexed + "; RAW DATA : " + rawline + "\n";
                                }
                                else convert += rawline + "\n";
                            }
                            catch (Exception e)
                            {
                                convert += rawline + "\n";
                                Console.WriteLine(" 오류 발생 : " + rawline);
                                Console.WriteLine(" 오류 : " + e.ToString());
                                Console.WriteLine(" 스킵됨");
                            }
                        }
                        else convert += rawline + "\n";
                    }
                    File.WriteAllText(Path.GetFileNameWithoutExtension(args[0]) + ".asm", convert);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("사용법 : poketools.exe [매개변수]");
                Console.WriteLine("제작 : 뇌씨(009342@naver.com)");
                Console.WriteLine("https://github.com/009342/poketools");
                Console.WriteLine("HEX값 변환");
                Console.WriteLine("- poketools.exe [파일경로] HEX [주소(형식 : FFFF00)] [바꿀값(형식 : C0]");
                Console.WriteLine("파일 크기 변경");
                Console.WriteLine("- poketools.exe [파일경로] SIZE [새로운 크기(형식 : FFFF00)]");
                Console.WriteLine("charmap 변환");
                Console.WriteLine("- poketools.exe [파일경로] CHARMAP");
                Console.WriteLine("문자를 HEX값으로");
                Console.WriteLine("- poketools.exe [파일경로] CHARCONVERT [테이블(*.tbl)]");
                Console.WriteLine("");
                Console.WriteLine("오류 : " + e.ToString());
            }

        }
    }
}
