using Bom.Squad;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
#if !NET452
using Utf8Json;
#endif

namespace Test;

public class WorkaroundsTest: IDisposable {

    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    public void Dispose() {
        BomSquad.RearmUtf8Bom();
    }

#if !NET452
    [Fact]
    public async Task ZcsUtf8Json() {
        BomSquad.DefuseUtf8Bom();

        MemoryStream stream       = new(Utf8.GetBytes("""{"hello":"world"}"""));
        var          deserialized = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
        deserialized["hello"].Should().Be("world");
    }
#endif

}