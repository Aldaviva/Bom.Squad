💣 Bom.Squad
===

[![Nuget](https://img.shields.io/nuget/v/Bom.Squad?logo=nuget&color=blue)](https://www.nuget.org/packages/Bom.Squad/) [![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/Bom.Squad/dotnetpackage.yml?branch=master&logo=github)](https://github.com/Aldaviva/Bom.Squad/actions/workflows/dotnetpackage.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:Bom.Squad/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/245919) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/Bom.Squad?logo=coveralls)](https://coveralls.io/github/Aldaviva/Bom.Squad?branch=master)

*Disable writing UTF-8 byte order marks (BOMs)*

<!-- MarkdownTOC autolink="true" bracket="round" levels="1,2,3" bullets="1." -->

1. [Quick Start](#quick-start)
1. [Prerequisites](#prerequisites)
1. [Problem](#problem)
1. [Solutions](#solutions)
    1. [Manually construct `UTF8Encoding` instances](#manually-construct-utf8encoding-instances)
    1. [Cross-cutting program-wide fix](#cross-cutting-program-wide-fix)

<!-- /MarkdownTOC -->

## Quick Start
```cmd
dotnet add package Bom.Squad
```
```cs
using Bom.Squad;

BomSquad.DefuseUtf8Bom();
```

## Prerequisites
- .NET runtime
    - .NET 5 or later
    - .NET Core 2.0 or later
    - .NET Framework 4.5.2 or later

## Problem

When serializing Unicode strings into bytes, there are several different strategies to choose from.

UTF-16 is one such format, and it tranforms each codepoint (character) into two or four bytes. Since there are multiple bytes for each codepoint, it is important for the deserializer to determine the order of those bytes, as they can start with the most significant byte (big endian, network byte order) or the least significant byte (little endian, CPU byte order). To detect the serialized byte order, UTF-16 deserializers look for a well-known prefix of two bytes and use its value to determine the endianness (`0xFEFF` indicates UTF-16 BE, and `0xFFFE` indicates UTF-16 LE). This prefix left by the encoder is called the byte order mark (BOM).

UTF-8 is another Unicode transformation format, and each codepoint can be serialized into one to four bytes. However, unlike UTF-16, bytes in UTF-8 only have one ordering. It is invalid to shuffle the order of UTF-8 bytes. Therefore, UTF-8 does not require a byte order mark, as there is only one possible byte order. It is valid to include a BOM anyway (`0xEFBBBF`), but it doesn't actually mark a byte order — it only indicates that the bytes represent UTF-8 code units, as opposed to those of UTF-16 or ASCII or other encoding formats. This is sometimes called a UTF-8 Signature instead of a BOM.

Unfortunately, Microsoft has decided that UTF-8 byte streams should always be prefixed with a BOM, and most Microsoft products like .NET and PowerShell will add it. This is a very serious interoperability problem because most UTF-8 decoders (excluding those made by Microsoft) do not decode or interpret the BOM prefix, which results in 3 malformed bytes appearing at the beginning of all decoded strings instead of being stripped out by the decoder as intended. Furthermore, these bytes map to unprintable glyphs, so a visual inspection of the decoded string will not reveal the invisible reason the string has malformed data at the beginning, until it occurs to you to open a hex editor. Any further processing of this malformed data, such as pattern matching, parsing, or concatenating with other strings, will result in data corruption and incorrect results.

Examples of software that have encountered confusing, time-consuming errors because Microsoft encoded a BOM into UTF-8 data are
- [Google Calendar](https://twitter.com/Aldaviva/status/1539351969667485696)
- [SpamAssassin](https://twitter.com/Aldaviva/status/1226025597949730816)
- [PagerDuty](https://github.com/Aldaviva/PagerDuty/commit/cf9567f374805f3894faa216873caa4d53ba8900)
- [Debian APT](https://twitter.com/Aldaviva/status/1796978091799286232)

In conclusion, Microsoft's valid but highly-incompatible defaults result in corruption when data is shared between Microsoft and non-Microsoft software.

## Solutions

### Manually construct `UTF8Encoding` instances

The UTF-8 encoding functionality in .NET is contained in the [`System.Text.UTF8Encoding`](https://learn.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.-ctor) class. It takes a constructor parameter, `encoderShouldEmitUTF8Identifier`, that determines whether the encoder should output a BOM or not.

```cs
new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true); // output BOM
new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true); // don't output BOM
```

UTF-8 is most commonly specified in .NET using the static property [`System.Text.Encoding.UTF8`](https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.utf8). This value is based on a `UTF8Encoding` instance that has `encoderShouldEmitUTF8Identifier` set to `true`, which is why it's so common for PowerShell and .NET programs to output BOMs when they encode strings to UTF-8.

```cs
// pseudocode of the built-in .NET behavior
public abstract class Encoding {
    public static Encoding UTF8 { get; } = new UTF8Encoding(true);
}
```

The most straightfoward fix for this problem is to manually construct a `UTF8Encoding` instance with the desired constructor parameters any time you are able to pass an `Encoding` parameter to any method.

```cs
new StreamWriter(stream, new UTF8Encoding(false, true));
```

In previous versions of .NET runtimes, there was a defect where constructing a `UTF8Encoding` with `encoderShouldEmitUTF8Identifier` set to `false` would also prevent it from *decoding* a UTF-8 string that started with a BOM, which required you to create multiple `UTF8Encoding` instances with different constructor parameters, one for encoding and one for decoding. Thankfully, this has been fixed in more recent runtimes — verified in .NET Framework ≥ 4.5.2 and .NET ≥ 6. Therefore, you can just create a `new UTF8Encoding(false, true)` and store it as a public static field somewhere that you can access anywhere in your codebase.

#### Code inspection

If you're worried about forgetting to use `new UTF8Encoding(false, true)` instead of `Encoding.UTF8`, you can create a custom IDE inspection in ReSharper.

1. Go to ReSharper → Options → Code Inspection → Custom Patterns → Add Pattern.
1. Set **Search pattern** to `Encoding.UTF8`.
1. Set **Pattern severity** to `Show as warning` or whatever level you want.
1. Set **Suppression key** to something like `utfbom` and its **Description** to something like `Encoding.UTF8 includes a BOM, which is incompatible with most non-Microsoft UTF-8 decoders.`.
1. Set **Replace pattern** to `new System.Text.UTF8Encoding(false, true)`.
1. Enable **Format after replace**.
1. Enable **Shorten references**.
1. Set **Description** to something like `UTF8 without BOM`.
1. Click **Save**.

![ReSharper custom pattern](https://raw.githubusercontent.com/Aldaviva/Bom.Squad/master/.github/images/highlighting-pattern.png)

### Cross-cutting program-wide fix

*This is the solution provided by this library.*

Managing all of those custom instances of `UTF8Encoding` can be annoying and hard to keep track of. There may not be a good place to share one `Encoding` instance, and some code may not let you specify your own `Encoding`.

To fix this, you can disable UTF-8 BOM encoding for your entire program by modifying the existing shared `Encoding.UTF8` instance in-memory at runtime to set its private readonly boolean field [`_emitUTF8Identifier`](https://source.dot.net/System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Text/UTF8Encoding.cs.html#f0ca9b36d106c6bd) to `false`. This functionality has been packed into the [**Bom.Squad**](https://www.nuget.org/packages/Bom.Squad) NuGet package.

```cs
using Bom.Squad;

public class Program {
    public static void Main(string[] args) {
        BomSquad.DefuseUtf8Bom();

        // the rest of your program
    }
}
```

The process-wide `Encoding.UTF8` instance will now behave as though it had been constructed with its `encoderShouldEmitUTF8Identifier` constructor parameter set to `false` instead of `true`.

No modifications are made to other `UTF8Encoding` instances, other process, or any assembly files on disk.