using FluentAssertions;
using NATS.Client.Core;
using Weda.Core.Infrastructure.Audit;

namespace Weda.Protocol.UnitTests;

public class TracingTests
{
    #region ShortId Tests

    [Fact]
    public void ShortId_Generate_ReturnsCorrectLength()
    {
        var id = ShortId.Generate();
        id.Length.Should().Be(12);
    }

    [Fact]
    public void ShortId_Generate_WithCustomLength_ReturnsCorrectLength()
    {
        var id = ShortId.Generate(16);
        id.Length.Should().Be(16);
    }

    [Fact]
    public void ShortId_Generate_ReturnsBase62Characters()
    {
        var id = ShortId.Generate();
        id.All(c => char.IsLetterOrDigit(c)).Should().BeTrue();
    }

    [Fact]
    public void ShortId_Generate_ReturnsUniqueIds()
    {
        var ids = Enumerable.Range(0, 1000).Select(_ => ShortId.Generate()).ToList();
        ids.Count.Should().Be(ids.Distinct().Count());
    }

    #endregion

    #region TraceContext Tests

    [Fact]
    public void TraceContext_Create_GeneratesNewIds()
    {
        var ctx = TraceContext.Create();

        ctx.TraceId.Should().NotBeNullOrEmpty();
        ctx.RequestId.Should().NotBeNullOrEmpty();
        ctx.TraceId.Length.Should().Be(12);
        ctx.RequestId.Length.Should().Be(12);
        ctx.Timestamp.Should().BeGreaterThan(0);
    }

    [Fact]
    public void TraceContext_Create_PreservesExistingTraceId()
    {
        var existingTraceId = "existing123x";
        var ctx = TraceContext.Create(existingTraceId);

        ctx.TraceId.Should().Be(existingTraceId);
        ctx.RequestId.Should().NotBe(existingTraceId);
    }

    [Fact]
    public void TraceContext_Create_GeneratesValidTimestamp()
    {
        var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var ctx = TraceContext.Create();
        var after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        ctx.Timestamp.Should().BeInRange(before, after);
    }

    #endregion

    #region NatsHeadersExtensions Tests

    [Fact]
    public void NatsHeaders_GetTraceContext_FromNullHeaders_CreatesNewContext()
    {
        NatsHeaders? headers = null;
        var ctx = headers.GetTraceContext();

        ctx.TraceId.Should().NotBeNullOrEmpty();
        ctx.RequestId.Should().NotBeNullOrEmpty();
        ctx.Timestamp.Should().BeGreaterThan(0);
    }

    [Fact]
    public void NatsHeaders_GetTraceContext_FromEmptyHeaders_CreatesNewContext()
    {
        var headers = new NatsHeaders();
        var ctx = headers.GetTraceContext();

        ctx.TraceId.Should().NotBeNullOrEmpty();
        ctx.RequestId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void NatsHeaders_WithTraceContext_SetsAllHeaders()
    {
        var ctx = TraceContext.Create();
        var headers = new NatsHeaders().WithTraceContext(ctx);

        headers[TraceConstants.TraceIdHeader].First().Should().Be(ctx.TraceId);
        headers[TraceConstants.RequestIdHeader].First().Should().Be(ctx.RequestId);
        headers[TraceConstants.TimestampHeader].First().Should().Be(ctx.Timestamp.ToString());
    }

    [Fact]
    public void NatsHeaders_Roundtrip_PreservesContext()
    {
        var original = TraceContext.Create();
        var headers = new NatsHeaders().WithTraceContext(original);
        var restored = headers.GetTraceContext();

        restored.TraceId.Should().Be(original.TraceId);
        restored.RequestId.Should().Be(original.RequestId);
        restored.Timestamp.Should().Be(original.Timestamp);
    }

    [Fact]
    public void CreateWithTraceContext_ReturnsHeadersWithContext()
    {
        var ctx = TraceContext.Create();
        var headers = NatsHeadersExtensions.CreateWithTraceContext(ctx);

        headers[TraceConstants.TraceIdHeader].First().Should().Be(ctx.TraceId);
        headers[TraceConstants.RequestIdHeader].First().Should().Be(ctx.RequestId);
        headers[TraceConstants.TimestampHeader].First().Should().Be(ctx.Timestamp.ToString());
    }

    [Fact]
    public void NatsHeaders_GetTraceContext_ExtractsExistingValues()
    {
        var headers = new NatsHeaders
        {
            { TraceConstants.TraceIdHeader, "trace123" },
            { TraceConstants.RequestIdHeader, "request456" },
            { TraceConstants.TimestampHeader, "1234567890" }
        };

        var ctx = headers.GetTraceContext();

        ctx.TraceId.Should().Be("trace123");
        ctx.RequestId.Should().Be("request456");
        ctx.Timestamp.Should().Be(1234567890);
    }

    [Fact]
    public void NatsHeaders_GetTraceContext_PartialHeaders_GeneratesMissingValues()
    {
        var headers = new NatsHeaders
        {
            { TraceConstants.TraceIdHeader, "trace123" }
        };

        var ctx = headers.GetTraceContext();

        ctx.TraceId.Should().Be("trace123");
        ctx.RequestId.Should().NotBeNullOrEmpty();
        ctx.RequestId.Length.Should().Be(12);
    }

    #endregion
}