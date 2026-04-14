using System;
using Microsoft.AspNetCore.Identity;
using OpineHere.Data.entity;
using OpineHere.mvc.Models;

namespace OpineHere.mvc.Mapping;

public static class MarkdownPostMapper
{
    /// <summary>
    /// Maps a MarkdownPost Entity to a MarkdownPostDto.
    /// </summary>
    public static MarkdownPostDto ToDto(this MarkdownPost entity)
    {
        if (entity == null) return null!;

        return new MarkdownPostDto
        {
            Id = entity.Id,
            Title = entity.Title,
            RawContent = entity.Content, // Mapping Content to RawContent
            UserId = entity.UserId,
            AuthorName = entity.PenName, // Mapping PenName to AuthorName
            PostDate = entity.PostDate,
            LastUpdate = entity.LastUpdate
        };
    }

    /// <summary>
    /// Maps a MarkdownPostDto back to a MarkdownPost Entity.
    /// </summary>
    public static MarkdownPost ToEntity(this MarkdownPostDto dto)
    {
        if (dto == null) return null!;
        return new MarkdownPost
        {
            Id = dto.Id,
            Title = dto.Title,
            Content = dto.RawContent, // Mapping RawContent back to Content
            UserId = dto.UserId,
            PenName = dto.AuthorName, // Mapping AuthorName back to PenName
            PostDate = dto.PostDate,
            LastUpdate = dto.LastUpdate
        };
    }

    public static ICollection<MarkdownPostDto> ToDto(this ICollection<MarkdownPost> entities)
    {
        ICollection<MarkdownPostDto> dtos = new List<MarkdownPostDto>();
        foreach (var entity in entities)
        {
            dtos.Add(ToDto(entity));
        }
        return dtos;
    }

    public static ICollection<MarkdownPost> ToEntities(this ICollection<MarkdownPostDto> dtos)
    {
        ICollection<MarkdownPost> entities = new List<MarkdownPost>();
        foreach (var dto in dtos)
        {
            entities.Add(ToEntity(dto));
        }
        return entities;
    }
    /// <summary>
    /// Updates an existing entity with values from a DTO (Useful for PUT/Update operations).
    /// </summary>
    public static void UpdateEntity(this MarkdownPost entity, MarkdownPostDto dto)
    {
        entity.Title = dto.Title;
        entity.Content = dto.RawContent;
        entity.PenName = dto.AuthorName;
        entity.LastUpdate = DateTimeOffset.UtcNow; // Usually updated on the fly
    }
}