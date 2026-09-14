using ServiCore.Domain.Entities;
using ServiCore.Domain.Enums;

namespace ServiCore.UnitTests;

public sealed class InvitationTests
{
    [Fact]
    public void OrganizationInvitation_ShouldNormalizeEmailAndBeUsable()
    {
        var invitation = new OrganizationInvitation(
            Guid.NewGuid(),
            "  USER@EXAMPLE.COM ",
            OrganizationRole.Agent,
            Guid.NewGuid(),
            "hashed-token",
            DateTime.UtcNow.AddDays(1));

        Assert.Equal("user@example.com", invitation.Email);
        Assert.True(invitation.IsUsable);
        Assert.False(invitation.IsAccepted);
        Assert.False(invitation.IsRevoked);
    }

    [Fact]
    public void OrganizationInvitation_Accept_ShouldMakeItUnavailable()
    {
        var invitation = new OrganizationInvitation(
            Guid.NewGuid(),
            "user@example.com",
            OrganizationRole.Manager,
            Guid.NewGuid(),
            "hashed-token",
            DateTime.UtcNow.AddDays(1));

        invitation.Accept();

        Assert.True(invitation.IsAccepted);
        Assert.False(invitation.IsUsable);
        Assert.NotNull(invitation.AcceptedAt);
        Assert.Throws<InvalidOperationException>(() => invitation.Accept());
    }

    [Fact]
    public void OrganizationInvitation_Revoke_ShouldMakeItUnavailable()
    {
        var invitation = new OrganizationInvitation(
            Guid.NewGuid(),
            "user@example.com",
            OrganizationRole.Agent,
            Guid.NewGuid(),
            "hashed-token",
            DateTime.UtcNow.AddDays(1));

        invitation.Revoke();

        Assert.True(invitation.IsRevoked);
        Assert.False(invitation.IsUsable);
        Assert.NotNull(invitation.RevokedAt);
        Assert.Throws<InvalidOperationException>(() => invitation.Accept());
    }

    [Fact]
    public void CustomerInvitation_ShouldNormalizeEmailAndBeUsable()
    {
        var invitation = new CustomerInvitation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "  CUSTOMER@EXAMPLE.COM ",
            "hashed-token",
            DateTime.UtcNow.AddDays(1));

        Assert.Equal("customer@example.com", invitation.Email);
        Assert.True(invitation.IsUsable);
    }

    [Fact]
    public void OrganizationInvitation_ShouldRejectOwnerRole()
    {
        Assert.Throws<ArgumentException>(() => new OrganizationInvitation(
            Guid.NewGuid(),
            "user@example.com",
            OrganizationRole.Owner,
            Guid.NewGuid(),
            "hashed-token",
            DateTime.UtcNow.AddDays(1)));
    }
}
