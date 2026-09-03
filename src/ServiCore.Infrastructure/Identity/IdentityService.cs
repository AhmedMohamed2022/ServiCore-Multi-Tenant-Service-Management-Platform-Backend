using Microsoft.AspNetCore.Identity;
using ServiCore.Application.Common.Interfaces;
using ServiCore.Application.Common.Results;

namespace ServiCore.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(
            user,
            password);

        if (!result.Succeeded)
        {
            var error = string.Join(
                "; ",
                result.Errors.Select(x => x.Description));

            return Result<Guid>.Failure(error);
        }

        return Result<Guid>.Success(user.Id);
    }

    public async Task<Result<Guid>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Result<Guid>.Failure(
                "Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            password,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Result<Guid>.Failure(
                "Invalid email or password.");
        }

        return Result<Guid>.Success(user.Id);
    }
}