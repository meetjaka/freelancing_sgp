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
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null 
                    ? (src.Client.FirstName + " " + src.Client.LastName).Trim() 
                    : "Client"))
                .ForMember(dest => dest.ClientCreatedAt, opt => opt.MapFrom(src => src.Client != null ? src.Client.CreatedAt : DateTime.MinValue))
                .ForMember(dest => dest.ClientProjectsCount, opt => opt.MapFrom(src => src.Client != null && src.Client.ClientProjects != null ? src.Client.ClientProjects.Count : 0))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Other"))
                .ForMember(dest => dest.BidsCount, opt => opt.MapFrom(src => src.Bids != null ? src.Bids.Count : 0))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.ProjectSkills != null 
                    ? src.ProjectSkills.Where(ps => ps.Skill != null).Select(ps => ps.Skill.Name).ToList() 
                    : new List<string>()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ContractId, opt => opt.MapFrom(src => src.Contract != null ? (int?)src.Contract.Id : null));

            CreateMap<CreateProjectDto, Project>();
            CreateMap<UpdateProjectDto, Project>();

            // Bid mappings
            CreateMap<Bid, BidDto>()
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer != null 
                    ? (src.Freelancer.FirstName + " " + src.Freelancer.LastName).Trim() 
                    : "Freelancer"))
                .ForMember(dest => dest.FreelancerRating, opt => opt.MapFrom(src => (src.Freelancer != null && src.Freelancer.FreelancerProfile != null) ? src.Freelancer.FreelancerProfile.AverageRating : 0))
                .ForMember(dest => dest.FreelancerCompletedProjects, opt => opt.MapFrom(src => (src.Freelancer != null && src.Freelancer.FreelancerProfile != null) ? src.Freelancer.FreelancerProfile.CompletedProjects : 0))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : "Project"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateBidDto, Bid>();

            // Profile mappings
            CreateMap<FreelancerProfile, FreelancerProfileDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null
                    ? src.User.FirstName + " " + src.User.LastName
                    : "Freelancer"))
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
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => src.Project != null ? src.Project.Title : "Project"))
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null 
                    ? (src.Client.FirstName + " " + src.Client.LastName).Trim() 
                    : "Client"))
                .ForMember(dest => dest.FreelancerId, opt => opt.MapFrom(src => src.FreelancerId))
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer != null 
                    ? (src.Freelancer.FirstName + " " + src.Freelancer.LastName).Trim() 
                    : "Freelancer"))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.AgreedAmount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Contract, ContractDetailsViewModel>()
                .ForMember(dest => dest.Contract, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Transactions, opt => opt.MapFrom(src => src.PaymentTransactions))
                .ForMember(dest => dest.CanReview, opt => opt.Ignore());

            CreateMap<CreateContractDto, Contract>();

            // Skill & Category mappings
            CreateMap<Skill, SkillDto>();
            CreateMap<Category, CategoryDto>();

            // Payment Transaction mappings
            CreateMap<PaymentTransaction, PaymentTransactionDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            // File Attachment mappings
            CreateMap<FileAttachment, FileAttachmentDto>()
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy.FirstName + " " + src.UploadedBy.LastName));

            // Milestone mappings
            CreateMap<Milestone, MilestoneDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CreateMilestoneDto, Milestone>();

            // FreelancerService (Gig) mappings
            CreateMap<FreelancerService, FreelancerServiceDto>()
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer != null 
                    ? (src.Freelancer.FirstName + " " + src.Freelancer.LastName).Trim() 
                    : "Freelancer"))
                .ForMember(dest => dest.FreelancerImage, opt => opt.MapFrom(src => src.Freelancer != null ? src.Freelancer.ProfilePictureUrl : null))
                .ForMember(dest => dest.FreelancerRating, opt => opt.MapFrom(src => (src.Freelancer != null && src.Freelancer.FreelancerProfile != null) ? src.Freelancer.FreelancerProfile.AverageRating : 0))
                .ForMember(dest => dest.FreelancerIsVerified, opt => opt.MapFrom(src => src.Freelancer != null && src.Freelancer.IsVerified))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Other"))
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.ServiceSkills != null 
                    ? src.ServiceSkills.Where(ss => ss.Skill != null).Select(ss => ss.Skill.Name).ToList() 
                    : new List<string>()));
            CreateMap<CreateFreelancerServiceDto, FreelancerService>();

            // ServiceOrder mappings
            CreateMap<ServiceOrder, ServiceOrderDto>()
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.FreelancerServiceId))
                .ForMember(dest => dest.ServiceTitle, opt => opt.MapFrom(src => src.FreelancerService != null ? src.FreelancerService.Title : "Service"))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null 
                    ? (src.Client.FirstName + " " + src.Client.LastName).Trim() 
                    : "Client"))
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer != null 
                    ? (src.Freelancer.FirstName + " " + src.Freelancer.LastName).Trim() 
                    : "Freelancer"))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // Dispute mappings
            CreateMap<Dispute, DisputeDto>()
                .ForMember(dest => dest.RaisedByName, opt => opt.MapFrom(src => src.RaisedByUser != null 
                    ? (src.RaisedByUser.FirstName + " " + src.RaisedByUser.LastName).Trim() 
                    : "User"))
                .ForMember(dest => dest.ProjectTitle, opt => opt.MapFrom(src => (src.Contract != null && src.Contract.Project != null) ? src.Contract.Project.Title : "Project"))
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => (src.Contract != null && src.Contract.Client != null) ? (src.Contract.Client.FirstName + " " + src.Contract.Client.LastName).Trim() : "Client"))
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => (src.Contract != null && src.Contract.Freelancer != null) ? (src.Contract.Freelancer.FirstName + " " + src.Contract.Freelancer.LastName).Trim() : "Freelancer"))
                .ForMember(dest => dest.ResolvedByAdminName, opt => opt.MapFrom(src => src.ResolvedByAdmin != null ? (src.ResolvedByAdmin.FirstName + " " + src.ResolvedByAdmin.LastName).Trim() : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // TimeEntry mappings
            CreateMap<TimeEntry, TimeEntryDto>()
                .ForMember(dest => dest.FreelancerName, opt => opt.MapFrom(src => src.Freelancer.FirstName + " " + src.Freelancer.LastName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<CreateTimeEntryDto, TimeEntry>();

            // ConnectsTransaction mappings
            CreateMap<ConnectsTransaction, ConnectsTransactionDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        }
    }
}
