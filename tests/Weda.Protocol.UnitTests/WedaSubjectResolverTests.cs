using FluentAssertions;

using Weda.Core.Infrastructure.Messaging.Nats.Abstractions;

namespace Weda.Protocol.UnitTests;

public class WedaSubjectResolverTests
{
    private readonly WedaSubjectResolver _sut = new();

    #region CanResolve
    [Theory]
    [InlineData("eco1j.weda.74fe.dm.sh.cfg.req", true)]
    [InlineData("eco1p.weda.74fe.00.evt.alarm", true)]
    [InlineData("eco2j.test.abcd.aa.bb.cmd", true)]
    [InlineData("other.weda.74fe.dm.sh.cfg.req", false)]
    [InlineData("eco.weda.74fe.dm.sh.cfg.req", false)]
    [InlineData("eco1x.weda.74fe.dm.sh.cfg.req", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void CanResolve_ShouldReturnExpected(string? subject, bool expected)
    {
        var result = _sut.CanResolve(subject!);
        result.Should().Be(expected);
    }
    #endregion

    #region Parse
    [Fact]
    public void Parse_ValidProtobufSubject_ShouldReturnCorrectSubjectInfo()
    {
        var subject = "eco1j.weda.74fe.dm.sh.cfg.req";
        var result = _sut.Parse(subject);
        result.Protocol.Should().Be("eco1j");
        result.Version.Should().Be(1);
        result.ContentType.Should().Be("application/json");
        result.Segments["groupId"].Should().Be("weda");
        result.Segments["resourceId"].Should().Be("74fe");
        result.Segments["from"].Should().Be("dm");
        result.Segments["to"].Should().Be("sh");
        result.Segments["messageType"].Should().Be("cfg");
        result.Segments["params"].Should().Be("req");
    }

    [Fact]
    public void Parse_ValidProtobufSubject_ShouldReturnCorrectContentType()
    {
        var subject = "eco1p.weda.74fe.dm.00.evt.alarm.critical";
        var result = _sut.Parse(subject);
        result.Segments["params"].Should().Be("alarm.critical");
    }

    [Fact]
    public void Parse_MinimalSubject_ShouldHaveEmptyParams()
    {
        var subject = "eco1p.weda.74fe.dm.sh.cfg";
        var result = _sut.Parse(subject);
        result.Segments["params"].Should().BeEmpty();
    }

    [Fact]
    public void Parse_TooFewSegments_ShouldThrow()
    {
        var subject = "eco1p.weda.74fe.dm.sh";
        var act = () => _sut.Parse(subject);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_InvalidPrefix_ShouldThrow()
    {
        var subject = "invalid.weda.74fe.dm.sh.cfg.req";
        var act = () => _sut.Parse(subject);
        act.Should().Throw<ArgumentException>();
    }
    #endregion

    #region Build
    [Fact]
    public void Build_ShouldReconstructSubject()
    {
        var info = new SubjectInfo
        {
            Protocol = "eco1j",
            Version = 1,
            ContentType = "application/json",
            Segments = new Dictionary<string, string>
            {
                ["groupId"] = "weda",
                ["resourceId"] = "74fe",
                ["from"] = "dm",
                ["to"] = "sh",
                ["messageType"] = "cfg",
                ["params"] = "req"
            }
        };
        var result = _sut.Build(info);
        result.Should().Be("eco1j.weda.74fe.dm.sh.cfg.req");
    }

    [Fact]
    public void Build_WithEmptyParams_ShouldOmitTrailingDot()
    {
        var info = new SubjectInfo
        {
            Protocol = "eco1j",
            Version = 1,
            Segments = new Dictionary<string, string>
            {
                ["groupId"] = "weda",
                ["resourceId"] = "74fe",
                ["from"] = "dm",
                ["to"] = "sh",
                ["messageType"] = "cfg",
                ["params"] = ""
            }
        };
        var result = _sut.Build(info);
        result.Should().Be("eco1j.weda.74fe.dm.sh.cfg");
    }

    [Theory]
    [InlineData("eco1j.weda.74fe.dm.sh.cfg.req")]
    [InlineData("eco1p.weda.74fe.dm.00.evt.alarm.critical")]
    [InlineData("eco2j.test.abcd.aa.bb.cmd")]
    public void ParseThenBuild_ShouldRoundtrip(string subject)
    {
        var parsed = _sut.Parse(subject);
        var rebuilt = _sut.Build(parsed);
        rebuilt.Should().Be(subject);
    }
    #endregion
}