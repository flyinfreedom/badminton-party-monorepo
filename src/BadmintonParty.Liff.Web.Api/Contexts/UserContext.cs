using BadmintonParty.Infrastructure.Entities;
using BadmintonParty.Infrastructure.Repositories;
using BadmintonParty.Infrastructure.Enums;
using BadmintonParty.Infrastructure.Contexts;
using BadmintonParty.Infrastructure.Extensions;
namespace BadmintonParty.Liff.Web.Api.Contexts;

using BadmintonParty.Liff.Web.Api.Models;
using BadmintonParty.Infrastructure.Models;

public interface IUserContext
{
    public string MemberId { get; }
    public string LineUserId { get; }
    public string PictureUrl { get; }
    public string DisplayName { get; }

    public void SetUserProfile(MemberProfile memberProfile);
    public void SetUserPictureUrl(string url);
}

public class UserContext : IUserContext
{
    private string _memberId = string.Empty;
    private string _lineUserId = string.Empty;
    private string _pictureUrl = string.Empty;
    private string _displayName = string.Empty;

    public string MemberId => _memberId;
    public string LineUserId => _lineUserId;
    public string PictureUrl => _pictureUrl;
    public string DisplayName => _displayName;

    public void SetUserProfile(MemberProfile memberProfile)
    {
        _memberId = memberProfile.MemberId;
        _lineUserId = memberProfile.LineUserId;
        _pictureUrl = memberProfile.PictureUrl;
        _displayName = memberProfile.DisplayName;
    }

    public void SetUserPictureUrl(string url)
    {
        _pictureUrl = url;
    }
}

