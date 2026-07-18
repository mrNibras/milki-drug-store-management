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
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}

public class CreateMedicineRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Medicine.CreateMedicineRequest>
{
    public CreateMedicineRequestValidator()
    {
        RuleFor(x => x.MedicineName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.UnitType).NotEmpty();
    }
}

public class AddBatchRequestValidator : AbstractValidator<MilkiDrugStore.Application.DTOs.Medicine.AddBatchRequest>
{
    public AddBatchRequestValidator()
    {
        RuleFor(x => x.MedicineId).GreaterThan(0);
        RuleFor(x => x.BatchNumber).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.PurchasePrice).GreaterThan(0);
        RuleFor(x => x.SellingPrice).GreaterThan(0);
        RuleFor(x => x.ExpiryDate).GreaterThan(DateTime.Now);
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
