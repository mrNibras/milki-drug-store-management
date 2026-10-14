using FluentValidation;

namespace MilkiDrugStore.Application.Validators;

public class LoginRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Auth.LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class RegisterRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Auth.RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullName).Must(FullNameValidator.IsValidFullName)
            .WithMessage("Full name must contain only letters, spaces, hyphens, and apostrophes. Digits and special characters are not allowed.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().Must(PasswordValidator.IsValid)
            .WithMessage(p => {
                PasswordValidator.IsValid(p.Password, out var msg);
                return msg;
            });
    }
}

public class CreateUserRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Auth.CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullName).Must(FullNameValidator.IsValidFullName)
            .WithMessage("Full name must contain only letters, spaces, hyphens, and apostrophes. Digits and special characters are not allowed.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().Must(PasswordValidator.IsValid)
            .WithMessage(p => {
                PasswordValidator.IsValid(p.Password, out var msg);
                return msg;
            });
        RuleFor(x => x.RoleId).GreaterThan(0);
    }
}

public class UpdateUserRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Auth.UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullName).Must(FullNameValidator.IsValidFullName)
            .WithMessage("Full name must contain only letters, spaces, hyphens, and apostrophes. Digits and special characters are not allowed.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public class CreateMedicineRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Medicine.CreateMedicineRequest>
{
    public CreateMedicineRequestValidator()
    {
        RuleFor(x => x.BrandName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryId).NotEqual(0).When(x => string.IsNullOrWhiteSpace(x.NewCategoryName));
        RuleFor(x => x.UnitTypeId).NotEqual(0).When(x => string.IsNullOrWhiteSpace(x.NewUnitTypeName));
        RuleFor(x => x.NewCategoryName).Must(name => string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(name.Trim()))
            .WithMessage("Category name must not be blank.");
        RuleFor(x => x.NewUnitTypeName).Must(name => string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(name.Trim()))
            .WithMessage("Unit type name must not be blank.");
    }
}

public class AddBatchRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Medicine.AddBatchRequest>
{
    public AddBatchRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.BatchNumber).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PurchasePrice).GreaterThan(0);
        RuleFor(x => x.SellingPrice).GreaterThan(0);
        RuleFor(x => x.ExpiryDate).GreaterThan(DateTime.UtcNow);
    }
}

public class CreatePurchaseRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Purchase.CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();
        RuleFor(x => x.AmountPaid).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentMethod)
            .Must(status => status == null || new[] { "cash", "bank_transfer", "mobile_money", "credit" }.Contains(status))
            .WithMessage("Invalid payment method");
        RuleFor(x => x.PaymentStatus)
            .Must(status => status == null || new[] { "paid", "partial", "unpaid" }.Contains(status))
            .WithMessage("Invalid payment status");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.PurchasePrice).GreaterThan(0);
            item.RuleFor(i => i.SellingPrice).GreaterThan(0);
            item.RuleFor(i => i.BatchNumber).NotEmpty();
        });
    }
}

public class CreateSaleRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Sale.CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
    }
}

public class CreateSupplierRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Supplier.CreateSupplierRequest>
{
    public CreateSupplierRequestValidator()
    {
        RuleFor(x => x.SupplierName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

public static class FullNameValidator
{
    /// <summary>
    /// Returns true when <paramref name="fullName"/> contains only Unicode letters,
    /// spaces, hyphens, and apostrophes after trimming. Digits, numeral systems,
    /// and any other punctuation are rejected.
    /// </summary>
    public static bool IsValidFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return false;

        var trimmed = fullName.Trim();
        if (trimmed.Length == 0 || trimmed.Length > 100)
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(
            trimmed,
            @"^[\p{L}\s'-]+$",
            System.Text.RegularExpressions.RegexOptions.None,
            TimeSpan.FromMilliseconds(100));
    }
}

public static class PasswordValidator
{
    private const int MinLength = 8;
    private const int MaxLength = 128;
    private static readonly System.Text.RegularExpressions.Regex HasUpper = new(@"[A-Z]", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex HasLower = new(@"[a-z]", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex HasDigit = new(@"\d", System.Text.RegularExpressions.RegexOptions.Compiled);
    private static readonly System.Text.RegularExpressions.Regex HasSpecial = new(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]", System.Text.RegularExpressions.RegexOptions.Compiled);

    public static bool IsValid(string password, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Password is required.";
            return false;
        }

        if (password.Length < MinLength)
        {
            errorMessage = $"Password must be at least {MinLength} characters long.";
            return false;
        }

        if (password.Length > MaxLength)
        {
            errorMessage = $"Password must not exceed {MaxLength} characters.";
            return false;
        }

        if (!HasUpper.IsMatch(password))
        {
            errorMessage = "Password must contain at least one uppercase letter.";
            return false;
        }

        if (!HasLower.IsMatch(password))
        {
            errorMessage = "Password must contain at least one lowercase letter.";
            return false;
        }

        if (!HasDigit.IsMatch(password))
        {
            errorMessage = "Password must contain at least one digit.";
            return false;
        }

        if (!HasSpecial.IsMatch(password))
        {
            errorMessage = "Password must contain at least one special character (!@#$%^&*()_+-=[]{};':\"\\|,.<>/?).";
            return false;
        }

        return true;
    }

    public static bool IsValid(string password)
    {
        return IsValid(password, out _);
    }
}
