using System.Collections;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

const string INPUT_FOLDER = "/Users/barretoareias/Studies/gameboy/GameClock/assets";
const string OUTPUT_FOLDER = "/Users/barretoareias/Studies/gameboy/GameClock/src/sprites";
const string MAP_TEMPLATE_FILE_NAME = "template_map";
const string DATA_TEMPLATE_FILE_NAME = "template_data";


Console.WriteLine("Running Converter");

var files = Directory.GetFiles(INPUT_FOLDER, "*.png", SearchOption.AllDirectories).ToList();

foreach (var file in files)
{
    var fileName = file.Split("/").Last().Replace(".png", "");
    using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
    using var bitmap = new Bitmap(fileStream);

    if (!bitmap.IsStandardSize())
        throw new Exception($"{file} not on standard size");

    var sprites = bitmap.GetSprites();
    SaveDataFile(fileName, sprites);
}

Console.WriteLine($"{files.Count()} converted!");

void SaveDataFile(string fileName, IList<Sprite> sprites)
{
    var template = File.ReadAllText(DATA_TEMPLATE_FILE_NAME);
    template = template.Replace("[[NUMBER_TILES]]", sprites.Count().ToString());
    template = template.Replace("[[TILE_DATA_NAME]]", fileName);
    var bytes = sprites.Aggregate(new List<byte>(), (acc, s) =>
    {
        acc.AddRange(s.Bytes);
        return acc;
    });
    template = template.Replace("[[TILE_DATA]]", bytes.ToHexString());

    File.WriteAllText(Path.Combine(OUTPUT_FOLDER, fileName + ".c"), template);
}

public static class BitMapExtensions
{
    const int TILE_SIZE = 8;
    public static bool IsStandardSize(this Bitmap bitmap) => bitmap.Width % TILE_SIZE == 0 && bitmap.Height % TILE_SIZE == 0;

    public static IList<Sprite> GetSprites(this Bitmap bitmap)
    {
        var sprites = new List<Sprite>();
        var colors = bitmap.GetColors();

        //each sprite is within a 8x8 pixel square
        int tileWidth = bitmap.Width / TILE_SIZE;
        int tileHeight = bitmap.Height / TILE_SIZE;

        for (var spriteRow = 1; spriteRow <= tileHeight; spriteRow++)
        {
            for (var spriteCol = 1; spriteCol <= tileWidth; spriteCol++)
            {
                var sprite = new Sprite();
                //sprite starting row
                for (var y = (spriteRow * TILE_SIZE) - TILE_SIZE; y < spriteRow * TILE_SIZE; y++)
                {
                    var rowBitsOne = new List<bool>();
                    var rowBitsTwo = new List<bool>();

                    //loop sprite collumn
                    for (var x = (spriteCol * TILE_SIZE) - TILE_SIZE; x < spriteCol * TILE_SIZE; x++)
                    {
                        var color = bitmap.GetPixel(x, y);
                        if (color == colors[0])
                        {
                            // darkest color
                            rowBitsOne.Add(true);
                            rowBitsTwo.Add(true);
                        }
                        else if (color == colors[1])
                        {

                            rowBitsOne.Add(false);
                            rowBitsTwo.Add(true);
                        }
                        else if (color == colors[2])
                        {
                            rowBitsOne.Add(true);
                            rowBitsTwo.Add(false);
                        }
                        else
                        {
                            // lightest color
                            rowBitsOne.Add(false);
                            rowBitsTwo.Add(false);
                        }
                    }

                    // revese bits before converting to bytes
                    rowBitsOne.Reverse();
                    rowBitsTwo.Reverse();

                    sprite.Bytes.Add(rowBitsOne.ToByte());
                    sprite.Bytes.Add(rowBitsTwo.ToByte());
                }

                var hasSprite = sprites.Exists(s => s.GetHashCode() == sprite.GetHashCode());
                if (!hasSprite)
                    sprites.Add(sprite);
            }
        }

        return sprites;
    }

    private static IList<Color> GetColors(this Bitmap bitmap)
    {
        List<Color> colors = new List<Color>();

        for (var i = 0; i < bitmap.Width; i++)
        {
            for (var j = 0; j < bitmap.Height; j++)
            {
                var pixelColor = bitmap.GetPixel(i, j);
                if (!colors.Contains(pixelColor))
                    colors.Add(pixelColor);
            }
        }

        if (colors.Count > 4)
            throw new Exception("File contains more than 4 colors");

        colors.Sort((a, b) => a.GetBrightness().CompareTo(b.GetBrightness()));

        return colors;
    }

    private static byte ToByte(this IList<bool> booleanBits)
    {
        var bits = new BitArray(booleanBits.ToArray());
        byte[] bytes = new byte[1];
        bits.CopyTo(bytes, 0);
        return bytes[0];
    }

    public static string ToHexString(this IList<byte> bytes)
    {
        var hex = new StringBuilder(bytes.Count() * 2);

        for (var i = 0; i < bytes.Count(); i++)
        {
            hex.AppendFormat("0x{0:X2}", bytes[i]);
            var isLastItem = i < bytes.Count() - 1;
            if (isLastItem)
            {
                hex.Append(",");
                var isNextMultipleOfSixteen = (i + 1) % 16 == 0;
                if (isNextMultipleOfSixteen)
                {
                    hex.AppendLine();
                    hex.Append("\t");
                }
            }
        }

        return hex.ToString();
    }

}