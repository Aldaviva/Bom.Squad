using Bom.Squad;
using System.Text;

BomSquad.DefuseUtf8Bom();

const string       message      = "hi";
using MemoryStream memoryStream = new(5);
using (StreamWriter writer = new(memoryStream, Encoding.UTF8)) {
    writer.Write(message);
}

bool isBomDefused = memoryStream.ToArray().Length == message.Length;
Console.WriteLine(isBomDefused ? "BOM is defused" : "BOM is NOT defused!");
return isBomDefused ? 0 : 1;