using AutoMapper;
using ZODs.Api.Repository.Common;
using ZODs.Api.Repository.Entities;
using ZODs.Api.Service.Dtos;
using ZODs.Api.Service.Dtos.User;
using ZODs.Api.Service.Mappers.Profiles;

namespace ZODs.Api.Service.Mappers
{
    public static class UserMapper
    {
        static UserMapper()
        {
            Mapper = new MapperConfiguration(cfg => cfg.AddProfile<UserMapperProfile>()).CreateMapper();
        }

        public static IMapper Mapper { get; }

        /// <summary>
        /// Maps <see cref="User"/> to <see cref="UserDto"/>.
        /// </summary>
        /// <param name="User">Entity to map from.</param>
        /// <returns>Mapped <see cref="UserDto"/>.</returns>
        public static UserDto ToDto(this User User)
        {
            return Mapper.Map<UserDto>(User);
        }

        /// <summary>
        /// Maps collection of <see cref="User"/> to collection of <see cref="UserDto"/>.
        /// </summary>
        /// <param name="entities">Collection of entities to map from.</param>
        /// <returns>Collection of mapped <see cref="UserDto"/>.</returns>
        public static IEnumerable<UserDto> ToDto(this IEnumerable<User> entities)
        {
            return Mapper.Map<IEnumerable<UserDto>>(entities);
        }

        /// <summary>
        /// Maps <see cref="UserDto"/> to <see cref="User"/>.
        /// </summary>
        /// <param name="UserDto">Dto to map from.</param>
        /// <returns>Mapped entity.</returns>
        public static User ToEntity(this UserDto UserDto)
        {
            return Mapper.Map<User>(UserDto);
        }

        /// <summary>
        /// Maps collection of <see cref="UserDto"/> to collection of <see cref="User"/>.
        /// </summary>
        /// <param name="entities">Entities.</param>
        /// <returns>Paged response.</returns>
        public static PagedResponse<UserDto> ToDto(this PagedEntities<User> entities)
        {
            return Mapper.Map<PagedResponse<UserDto>>(entities);
        }

        /// <summary>
        /// Maps <see cref="SignUpDto"/> to <see cref="UserDto"/>.
        /// </summary>
        /// <param name="signupDto"><see cref="SignUpDto"/>.</param>
        /// <returns><see cref="UserDto"/>.</returns>
        public static UserDto ToUserDto(this SignUpDto signupDto)
        {
            return Mapper.Map<UserDto>(signupDto);
        }
    }
}
