using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Inazumahax
{
    class Program
    {
        static byte[] WriteBytes(byte[] src, int offset, byte[] patch)
        {
            for(int i = 0; i < patch.Length; i++)
            {
                src[offset + i] = patch[i];
            }
            return src;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Inazumahax patcher v0.1 by SwareJonge");
            byte[] overflow = File.ReadAllBytes(@"nameOverflow.bin"); // This contains a string in shift JIS that says "inazumahax by SwareJonge" about 6 times and adds some extra bytes at the end

            int overflowlengthPAL = 0x116;
            int overflowlengthJP = 0x11D;
            byte[] save;

            string[] titleIDTable = { "STQJ", "STQP", "STQX", "SEZJ", "S5SJ" };
            string[] patchTable = { @"LoaderJP", @"loaderPAL", @"loaderPAL", @"loader2012", @"loaderGO" };
            int[] nameOffset = {0x134, 0x185, 0x185, 0x16c, 0x2790}; // Credits to Obluda for the offset in GO Strikers
            uint[] g_SaveDataAddress = { 0x804e7608, 0x805256d0, 0x805256d0, 0x8056b910, 0x805f9120};
            /* 0 = JP
             * 1 = PAL ENG, FRA, GER
             * 2 = PAL ENG, ITA, SPA
             * 2 = PAL ENG, FRA, GER
             * 3 = JP Strikers 2012 Extreme
             * 4 = JP Go Strikers 2012      
             */
            
            for (int i = 0; i < titleIDTable.Length; i++)
            {
                char[] titleID = titleIDTable[i].ToArray(); ;
                string path = @"src/private/wii/title/" + titleIDTable[i] + "/data/";
                if (titleIDTable[i] == "STQJ")
                    path = path + "inazuma.sav";
                else
                    path = path + "inazuma2.sav";
                Console.WriteLine("Patching {0} out of 5", i + 1);

                if (File.Exists(path))
                {
                    Console.WriteLine("Found {0}", path);
                    int overflowlength = overflowlengthPAL;
                    if (titleID[3] == 'J') // the Japanese versions of the game have a lightly different base string so the string has to be a little bit bigger in order to overwrite the return register in the stack
                        overflowlength = overflowlengthJP;

                    //Console.WriteLine("Overflow Length is: 0x{0}", overflowlength.ToString("X2"));

                    int returnaddress = int.Parse(File.ReadAllLines(@"src/" + patchTable[i] + ".lds")[10].Replace("	. = 0x", "").Replace(";", ""), System.Globalization.NumberStyles.HexNumber); // at line 11 the return address is specified
                    Console.WriteLine("Currently Patching {0}", titleIDTable[i]);
                    save = File.ReadAllBytes(path);
                    save = WriteBytes(save, nameOffset[i], overflow.Take(overflowlength).ToArray()); // Write the Savefilename                    
                    save = WriteBytes(save, nameOffset[i] + overflowlength, BitConverter.GetBytes(returnaddress).Reverse().ToArray());

                    int SaveFileOffset = (int)(returnaddress - g_SaveDataAddress[i]);
                    Console.WriteLine("Offset in Savefile is: 0x{0}", SaveFileOffset.ToString("X2"));
                    save = WriteBytes(save, SaveFileOffset, File.ReadAllBytes(@"src/" + patchTable[i] + ".bin")); // this contains the savezelda code, perhaps a modloader coud be made out of this in the future
                    File.WriteAllBytes(path, save);
                    Console.WriteLine("Saved {0}", path);
                }
                else Console.WriteLine("Din't find {0}", path);
            }
            Console.WriteLine("Done!");

        }
    }
}
