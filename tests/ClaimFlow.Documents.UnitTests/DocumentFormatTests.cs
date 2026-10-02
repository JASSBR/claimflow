using ClaimFlow.Documents.Domain;

namespace ClaimFlow.Documents.UnitTests;

public sealed class DocumentFormatTests
{
    [Fact]
    public void Detect_RecognisesSupportedFormats_FromTheirSignature()
    {
        DocumentFormat.Detect("%PDF-1.7\n"u8).ShouldBe(DocumentFormat.Pdf);
        DocumentFormat.Detect([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]).ShouldBe(DocumentFormat.Png);
        DocumentFormat.Detect([0xFF, 0xD8, 0xFF, 0xE0]).ShouldBe(DocumentFormat.Jpeg);
    }

    [Fact]
    public void Detect_RejectsAnExecutable_WhateverItsName()
    {
        DocumentFormat.Detect("MZ\u0090\0\u0003\0\0\0"u8).ShouldBeNull();
    }

    [Fact]
    public void Detect_RejectsTruncatedInput()
    {
        DocumentFormat.Detect("%PD"u8).ShouldBeNull();
        DocumentFormat.Detect([]).ShouldBeNull();
    }
}
