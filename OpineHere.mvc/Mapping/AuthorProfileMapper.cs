using OpineHere.Data.entity;
using OpineHere.mvc.Models;

namespace OpineHere.mvc.Mapping;
public class AuthorProfileMapper
{
    /// <summary>
    /// Maps an AuthorProfile entity to AuthorProfileDto
    /// </summary>
    public static AuthorProfileDto ToDto(AuthorProfile entity)
    {
        if (entity == null)
            return null;

        return new AuthorProfileDto
        {
            Id = entity.Id,
            Forename = entity.GivenName,
            Surname = entity.Surname
        };
    }

    /// <summary>
    /// Maps an AuthorProfileDto to AuthorProfile entity
    /// </summary>
    public static AuthorProfile MapToEntity(AuthorProfileDto dto)
    {
        if (dto == null)
            return null;

        return new AuthorProfile
        {
            Id = dto.Id,
            GivenName = dto.Forename,
            Surname = dto.Surname
        };
    }

    /// <summary>
    /// Maps an AuthorProfileDto to AuthorProfile entity with UserId
    /// </summary>
    public static AuthorProfile MapToEntity(AuthorProfileDto dto, Guid userId)
    {
        if (dto == null)
            return null;

        return new AuthorProfile
        {
            Id = dto.Id,
            GivenName = dto.Forename,
            Surname = dto.Surname,
            UserId = userId
        };
    }

    /// <summary>
    /// Updates an existing AuthorProfile entity with values from AuthorProfileDto
    /// </summary>
    public static void MapToEntity(AuthorProfileDto dto, AuthorProfile entity)
    {
        if (dto == null || entity == null)
            return;

        entity.GivenName = dto.Forename;
        entity.Surname = dto.Surname;
    }

    /// <summary>
    /// Maps a collection of AuthorProfile entities to AuthorProfileDtos
    /// </summary>
    public static IEnumerable<AuthorProfileDto> ToDto(IEnumerable<AuthorProfile> entities)
    {
        if (entities == null)
            return Enumerable.Empty<AuthorProfileDto>();

        return entities.Select(ToDto);
    }

    /// <summary>
    /// Maps a collection of AuthorProfileDtos to AuthorProfile entities
    /// </summary>
    public static IEnumerable<AuthorProfile> MapToEntityCollection(IEnumerable<AuthorProfileDto> dtos, Guid userId)
    {
        if (dtos == null)
            return Enumerable.Empty<AuthorProfile>();

        return dtos.Select(dto => MapToEntity(dto, userId));
    }
}