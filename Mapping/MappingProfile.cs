using AutoMapper;
using SGP_Freelancing.Models.DTOs;
using SGP_Freelancing.Models.Entities;
using SGP_Freelancing.Models.ViewModels;

namespace SGP_Freelancing.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<ApplicationUser, RegisterDto>().ReverseMap();

            // Project mappings
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.FirstName + " " + src.Client.LastName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.BidsCount, opt => opt.MapFrom(src => src.Bids.Count))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.ProjectSkills.Select(ps => ps.Skill.Name).ToList()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateProjectDto, Project>();
            CreateMap<UpdateProjectDto, Project>();

            // Bid mappings
            CreateMap<Bid, BidDto>()
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer.FirstName + " " + src.Freelancer.LastName))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateBidDto, Bid>();

            // Profile mappings
            CreateMap<FreelancerProfile, FreelancerProfileDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.FreelancerSkills.Select(fs => new SkillDto
                {
                    Id = fs.Skill.Id,
                    Name = fs.Skill.Name,
                    Description = fs.Skill.Description
                }).ToList()));

            CreateMap<UpdateFreelancerProfileDto, FreelancerProfile>();

            CreateMap<ClientProfile, ClientProfileDto>();
            CreateMap<UpdateClientProfileDto, ClientProfile>();

            // Message mappings
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.FirstName + " " + src.Sender.LastName))
                .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.Receiver.FirstName + " " + src.Receiver.LastName));

            CreateMap<SendMessageDto, Message>();

            // Review mappings
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer.FirstName + " " + src.Reviewer.LastName));

            CreateMap<CreateReviewDto, Review>();

            // Contract mappings
            CreateMap<Contract, ContractDto>()
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project.Title))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.FirstName + " " + src.Client.LastName))
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer.FirstName + " " + src.Freelancer.LastName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateContractDto, Contract>();

            // Skill & Category mappings
            CreateMap<Skill, SkillDto>();
            CreateMap<Category, CategoryDto>();

            // Payment Transaction mappings
            CreateMap<PaymentTransaction, PaymentTransactionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
