using System.Text.Json;
using AutoMapper;
using RulesEngine.Application.Dtos;
using RulesEngine.Models;

namespace RulesEngine.Application.Mapping;

public sealed class WorkflowMappingProfile : Profile
{
    public WorkflowMappingProfile()
    {
        CreateMap<ScopedParam, ScopedParamDto>().ReverseMap();

        CreateMap<ActionInfo, ActionInfoDto>()
            .ForMember(
                dest => dest.Context,
                opt => opt.MapFrom(src => src.Context.ToDictionary(
                    kvp => kvp.Key,
                    kvp => JsonSerializer.SerializeToElement(kvp.Value))));

        CreateMap<ActionInfoDto, ActionInfo>()
            .ForMember(
                dest => dest.Context,
                opt => opt.MapFrom(src => src.Context.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value)));

        CreateMap<RuleActions, RuleActionsDto>().ReverseMap();

        CreateMap<Rule, RuleDto>()
            .ForMember(dest => dest.RuleGuidId, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<Workflow, WorkflowDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Version, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.Comments, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveFromUtc, opt => opt.Ignore())
            .ForMember(dest => dest.EffectiveToUtc, opt => opt.Ignore())
            .ReverseMap();

        CreateMap<RuleResultTree, RuleResultDto>()
            .ForMember(dest => dest.RuleName, opt => opt.MapFrom(src => src.Rule != null ? src.Rule.RuleName : string.Empty))
            .ForMember(dest => dest.SuccessEvent, opt => opt.MapFrom(src => src.Rule != null ? src.Rule.SuccessEvent : null))
            .ForMember(dest => dest.ActionOutput, opt => opt.MapFrom(src =>
                src.ActionResult != null && src.ActionResult.Output != null
                    ? JsonSerializer.Serialize(src.ActionResult.Output)
                    : null))
            .ForMember(dest => dest.ChildResults, opt => opt.MapFrom(src => src.ChildResults));
    }
}
