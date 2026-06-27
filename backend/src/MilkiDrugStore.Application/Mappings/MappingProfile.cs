using MilkiDrugStore.Application.DTOs.Auth;
using MilkiDrugStore.Application.DTOs.Medicine;
using MilkiDrugStore.Application.DTOs.Purchase;
using MilkiDrugStore.Application.DTOs.Report;
using MilkiDrugStore.Application.DTOs.Sale;
using MilkiDrugStore.Application.DTOs.Supplier;
using MilkiDrugStore.Domain.Entities;

namespace MilkiDrugStore.Application.Mappings;

public class MappingProfile : AutoMapper.Profile
{
    public MappingProfile()
    {
        CreateMap<CreateMedicineRequest, Medicine>();
        CreateMap<UpdateMedicineRequest, Medicine>();
        CreateMap<Medicine, MedicineResponse>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : ""))
            .ForMember(d => d.Batches, opt => opt.MapFrom(s => s.Batches));

        CreateMap<MedicineBatch, BatchResponse>();

        CreateMap<CreatePurchaseRequest, Purchase>();
        CreateMap<PurchaseItem, PurchaseItemResponse>()
            .ForMember(d => d.MedicineName, opt => opt.MapFrom(s => s.Medicine != null ? s.Medicine.MedicineName : ""));

        CreateMap<CreateSaleRequest, Sale>();
        CreateMap<SaleItem, SaleItemResponse>()
            .ForMember(d => d.MedicineName, opt => opt.MapFrom(s => s.Medicine != null ? s.Medicine.MedicineName : ""))
            .ForMember(d => d.BatchNumber, opt => opt.MapFrom(s => s.Batch != null ? s.Batch.BatchNumber : ""));

        CreateMap<CreateSupplierRequest, Supplier>();
        CreateMap<UpdateSupplierRequest, Supplier>();
        CreateMap<Supplier, SupplierResponse>();

        CreateMap<Sale, SaleResponse>()
            .ForMember(d => d.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.FullName : ""));
    }
}
