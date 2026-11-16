using Bom.Squad;
using System.Text;

BomSquad.DefuseUtf8Bom();

const string       message      = "hi";
using MemoryStream memoryStream = new(5);
using (StreamWriter writer = new(memoryStream, Encoding.UTF8)) {
    writer.Write(message);
}

byte[] actual       = memoryStream.ToArray();
bool   isBomDefused = actual.Length == message.Length && !actual.StartsWith((ReadOnlySpan<byte>) [0xEF, 0xBB, 0xBF]);
Console.WriteLine(isBomDefused ? "BOM is defused" : "BOM is NOT defused!");
return isBomDefused ? 0 : 1;