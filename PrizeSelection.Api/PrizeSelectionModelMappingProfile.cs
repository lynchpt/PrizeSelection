using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PrizeSelection.Model;
using D = PrizeSelection.Dto.Api;

namespace PrizeSelection.Api
{
    public class PrizeSelectionModelMappingProfile : Profile
    {
        public PrizeSelectionModelMappingProfile()
        {
            CreateMap<PrizeCategorySpecification, D.PrizeCategorySpecification>();
            CreateMap<D.PrizeCategorySpecification, PrizeCategorySpecification>();

            CreateMap<PrizeResultRow, D.PrizeResultRow>();
            CreateMap<D.PrizeResultRow, PrizeResultRow>();

            CreateMap<PrizeSelectionRow, D.PrizeSelectionRow>();
            CreateMap<D.PrizeSelectionRow, PrizeSelectionRow>();

            CreateMap<PrizeSelectionsForSuccessInfo, D.PrizeSelectionsForSuccessInfo>();
            CreateMap<D.PrizeSelectionsForSuccessInfo, PrizeSelectionsForSuccessInfo>();

            CreateMap<SelectionDomain, D.SelectionDomain>();
            CreateMap<D.SelectionDomain, SelectionDomain>();
        }
    }
}
